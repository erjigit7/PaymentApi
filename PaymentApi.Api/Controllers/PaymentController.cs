using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentApi.Application.Interfaces;

namespace PaymentApi.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/payment")]
public sealed class PaymentController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpPost]
    public async Task<IActionResult> MakePayment()
    {
        var userIdValue = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        if (userIdValue is null || !Guid.TryParse(userIdValue, out var userId))
            return Unauthorized();

        var username = User.FindFirst(JwtRegisteredClaimNames.UniqueName)?.Value ?? "unknown";

        var result = await _paymentService.MakePaymentAsync(userId).ConfigureAwait(false);

        if (!result.Success)
        {
            return BadRequest(new
            {
                username,
                chargedAmount = result.ChargedAmount,
                remainingBalance = result.RemainingBalance,
                message = result.FailureReason
            });
        }

        return Ok(new
        {
            username,
            chargedAmount = result.ChargedAmount,
            remainingBalance = result.RemainingBalance
        });
    }
}