namespace PaymentApi.Application;

public sealed record GeneratedToken(string Token, string JwtId, DateTime ExpiresAt);