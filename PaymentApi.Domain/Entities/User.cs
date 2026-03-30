namespace PaymentApi.Domain.Entities;

public sealed class User : EntityBase
{
    public required string Login { get; init; }
    public required string PasswordHash { get; init; }
    public decimal Balance { get; init; }
    public DateTime CreatedAt { get; init; }
}
