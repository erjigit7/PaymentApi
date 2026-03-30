namespace PaymentApi.Application;

public sealed record LoginResult(
    bool Success,
    string? Token,
    string? ErrorCode,
    string? ErrorMessage)
{
    public static LoginResult Ok(string token) =>
        new(true, token, null, null);

    public static LoginResult Fail(string code, string message) =>
        new(false, null, code, message);
}