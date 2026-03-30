namespace PaymentApi.Domain.Entities;

public sealed class Payment : EntityBase
{
    public Guid UserId { get; init; }
    public decimal Amount { get; init; }
    public DateTime CreatedAt { get; init; }
}
