using PaymentApi.Domain.Entities;

public sealed class User : EntityBase
{
    public required string Login { get; init; }
    public required string PasswordHash { get; init; }
    public decimal Balance { get; init; }
    public DateTime CreatedAt { get; init; }

    public int FailedLoginAttempts { get; set; }
    public DateTime? LockoutEndUtc { get; set; }
}