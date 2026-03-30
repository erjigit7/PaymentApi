using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentApi.Api.Common;
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
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status423Locked)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.Login) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new ApiError("invalid_request", "Login and password are required."));
        }

        var result = await _authService.LoginAsync(request.Login, request.Password).ConfigureAwait(false);

        if (!result.Success)
        {
            if (result.ErrorCode == "user_locked")
            {
                return StatusCode(StatusCodes.Status423Locked,
                    new ApiError(result.ErrorCode!, result.ErrorMessage!));
            }

            return Unauthorized(new ApiError(
                result.ErrorCode ?? "unauthorized",
                result.ErrorMessage ?? "Unauthorized."));
        }

        return Ok(new LoginResponse(result.Token!));
    }

    [Authorize]
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout()
    {
        var jwtId = User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
        if (string.IsNullOrWhiteSpace(jwtId))
        {
            return Unauthorized(new ApiError("invalid_token", "Token does not contain jti."));
        }

        await _authService.LogoutByJwtIdAsync(jwtId).ConfigureAwait(false);
        return Ok(new { message = "Logged out successfully." });
    }
}

public sealed record LoginRequest(string Login, string Password);
public sealed record LoginResponse(string Token);