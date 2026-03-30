using Microsoft.AspNetCore.Mvc;
using PaymentApi.Application.Interfaces;

namespace PaymentApi.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.Login) || request.Password is null)
        {
            return Unauthorized();
        }

        var token = await _authService.LoginAsync(request.Login, request.Password).ConfigureAwait(false);
        if (token is null)
        {
            return Unauthorized();
        }

        return Ok(new LoginResponse(token));
    }

    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Logout()
    {
        var authorizationHeader = Request.Headers.Authorization.ToString();
        await _authService.LogoutAsync(authorizationHeader).ConfigureAwait(false);
        return Ok();
    }
}

public sealed record LoginRequest(string Login, string Password);

public sealed record LoginResponse(string Token);
