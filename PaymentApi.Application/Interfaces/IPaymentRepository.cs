using PaymentApi.Domain.Entities;

namespace PaymentApi.Application.Interfaces;

public interface IPaymentRepository
{
    Task AddAsync(Payment payment);
    Task<List<Payment>> GetByUserIdAsync(Guid userId);
}
