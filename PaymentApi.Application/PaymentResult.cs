namespace PaymentApi.Application;

public sealed record PaymentResult(
    bool Success,
    decimal ChargedAmount,
    decimal RemainingBalance,
    string? FailureReason)
{
    public static PaymentResult Ok(decimal chargedAmount, decimal remainingBalance)
        => new(true, chargedAmount, remainingBalance, null);

    public static PaymentResult Fail(string reason, decimal chargedAmount, decimal remainingBalance)
        => new(false, chargedAmount, remainingBalance, reason);
}