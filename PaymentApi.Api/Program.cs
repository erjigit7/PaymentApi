// File: PaymentApi.Api/Program.cs
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PaymentApi.Api.Extensions;
using PaymentApi.Application;
using PaymentApi.Application.Interfaces;
using PaymentApi.Infrastructure;
using PaymentApi.Infrastructure.Persistence;
using PaymentApi.Infrastructure.Security;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
    ?? throw new InvalidOperationException("Jwt configuration section is missing.");
if (string.IsNullOrWhiteSpace(jwtOptions.SecretKey))
{
    throw new InvalidOperationException("Jwt:SecretKey is not configured.");
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Variant B: don't remap "sub" -> ClaimTypes.NameIdentifier, keep raw JWT claim names.
        options.MapInboundClaims = false;

        options.IncludeErrorDetails = true;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey)),
            ClockSkew = TimeSpan.Zero,

            // Optional: if you use User.Identity.Name anywhere, it will read from "unique_name"
            NameClaimType = JwtRegisteredClaimNames.UniqueName,
            // RoleClaimType = "role" // set if you later add roles
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = ctx =>
            {
                var logger = ctx.HttpContext.RequestServices
                    .GetRequiredService<ILoggerFactory>()
                    .CreateLogger("JwtBearer");
                logger.LogError(ctx.Exception, "JWT authentication failed.");
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Payment API", Version = "v1" });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = JwtBearerDefaults.AuthenticationScheme,
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description =
            "JWT from POST /api/auth/login. Click **Authorize**, enter: `Bearer {yourToken}` or only the token (Swagger adds Bearer).",
    });
    options.OperationFilter<AuthorizeOperationFilter>();
});

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
    var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
    var seederLogger = loggerFactory.CreateLogger("PaymentApi.DbSeeder");
    await DbSeeder.SeedAsync(dbContext, passwordHasher, seederLogger).ConfigureAwait(false);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/debug/jwt", (IOptions<JwtOptions> opts) => new
{
    opts.Value.Issuer,
    opts.Value.Audience,
    SecretKeyLength = opts.Value.SecretKey?.Length,
    SecretKeyFirst5 = opts.Value.SecretKey?[..5]
});

// Optional: inspect claims you actually get
app.MapGet("/debug/claims", [Microsoft.AspNetCore.Authorization.Authorize] (ClaimsPrincipal user) =>
    user.Claims.Select(c => new { c.Type, c.Value }));

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UsePaymentApiMiddleware();
app.MapControllers();

app.Run();


