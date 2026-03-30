namespace PaymentApi.Domain.Entities;

public sealed class UserSession : EntityBase
{
    public Guid UserId { get; init; }
    public required string JwtId { get; init; }   // jti
    public required string TokenHash { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime ExpiresAt { get; init; }
    public DateTime? RevokedAt { get; set; }

    public bool IsRevoked => RevokedAt.HasValue;
}