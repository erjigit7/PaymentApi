using Microsoft.EntityFrameworkCore;
using PaymentApi.Application.Interfaces;
using PaymentApi.Domain.Entities;
using PaymentApi.Infrastructure.Persistence;

namespace PaymentApi.Infrastructure.Repositories;

public sealed class UserSessionRepository : IUserSessionRepository
{
    private readonly ApplicationDbContext _context;

    public UserSessionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(UserSession session)
    {
        await _context.UserSessions.AddAsync(session).ConfigureAwait(false);
        await _context.SaveChangesAsync().ConfigureAwait(false);
    }

    public async Task<UserSession?> GetByJwtIdAsync(string jwtId)
    {
        return await _context.UserSessions
            .FirstOrDefaultAsync(x => x.JwtId == jwtId)
            .ConfigureAwait(false);
    }

    public async Task RevokeByJwtIdAsync(string jwtId)
    {
        var session = await _context.UserSessions
            .FirstOrDefaultAsync(x => x.JwtId == jwtId)
            .ConfigureAwait(false);

        if (session is null || session.RevokedAt.HasValue)
            return;

        session.RevokedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync().ConfigureAwait(false);
    }
}