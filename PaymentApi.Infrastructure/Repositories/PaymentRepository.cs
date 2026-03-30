using Microsoft.EntityFrameworkCore;
using PaymentApi.Application.Interfaces;
using PaymentApi.Domain.Entities;
using PaymentApi.Infrastructure.Persistence;

namespace PaymentApi.Infrastructure.Repositories;

public sealed class PaymentRepository : IPaymentRepository
{
    private readonly ApplicationDbContext _context;

    public PaymentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Payment payment)
    {
        await _context.Payments.AddAsync(payment).ConfigureAwait(false);
        await _context.SaveChangesAsync().ConfigureAwait(false);
    }

    public async Task<List<Payment>> GetByUserIdAsync(Guid userId)
    {
        return await _context.Payments
            .AsNoTracking()
            .Where(p => p.UserId == userId)
            .ToListAsync()
            .ConfigureAwait(false);
    }
}
