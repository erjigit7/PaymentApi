using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentApi.Api.Common;
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
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> MakePayment()
    {
        var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdValue is null || !Guid.TryParse(userIdValue, out var userId))
        {
            return Unauthorized(new ApiError("invalid_token", "User id claim is missing or invalid."));
        }

        var username = User.FindFirst(ClaimTypes.Name)?.Value ?? "unknown";

        var result = await _paymentService.MakePaymentAsync(userId).ConfigureAwait(false);

        if (!result.Success)
        {
            return BadRequest(new
            {
                error = new ApiError("payment_failed", result.FailureReason ?? "Payment failed."),
                username,
                chargedAmount = result.ChargedAmount,
                remainingBalance = result.RemainingBalance
            });
        }

        return Ok(new
        {
            username,
            chargedAmount = result.ChargedAmount,
            remainingBalance = result.RemainingBalance,
            message = "Payment completed successfully."
        });
    }
}