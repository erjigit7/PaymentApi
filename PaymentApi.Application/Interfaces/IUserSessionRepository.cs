using PaymentApi.Domain.Entities;

namespace PaymentApi.Application.Interfaces;

public interface IUserSessionRepository
{
    Task AddAsync(UserSession session);
    Task<UserSession?> GetByJwtIdAsync(string jwtId);
    Task RevokeByJwtIdAsync(string jwtId);
}