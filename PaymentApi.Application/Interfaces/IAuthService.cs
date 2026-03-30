using PaymentApi.Application;

namespace PaymentApi.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResult> LoginAsync(string login, string password);
    Task LogoutByJwtIdAsync(string jwtId);
}