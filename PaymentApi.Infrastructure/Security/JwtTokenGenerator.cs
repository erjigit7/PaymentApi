using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PaymentApi.Application;
using PaymentApi.Application.Interfaces;

namespace PaymentApi.Infrastructure.Security;

public sealed class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly JwtOptions _options;

    public JwtTokenGenerator(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    public GeneratedToken GenerateToken(Guid userId, string login)
    {
        if (string.IsNullOrWhiteSpace(_options.SecretKey))
            throw new InvalidOperationException("JWT SecretKey is not configured.");

        var keyBytes = Encoding.UTF8.GetBytes(_options.SecretKey);
        if (keyBytes.Length < 32)
            throw new InvalidOperationException("JWT SecretKey must be at least 32 bytes.");

        var signingKey = new SymmetricSecurityKey(keyBytes);
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var jwtId = Guid.NewGuid().ToString();
        var expiresAt = DateTime.UtcNow.AddMinutes(_options.ExpirationMinutes);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, login),
            new Claim(JwtRegisteredClaimNames.Jti, jwtId)
        };

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        var tokenValue = new JwtSecurityTokenHandler().WriteToken(token);

        return new GeneratedToken(tokenValue, jwtId, expiresAt);
    }
}