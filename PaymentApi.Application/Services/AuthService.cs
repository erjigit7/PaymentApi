using PaymentApi.Application.Interfaces;
using PaymentApi.Domain.Entities;

namespace PaymentApi.Application.Services;

public sealed class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenGenerator _tokenGenerator;
    private readonly IPasswordHasher _passwordHasher;

    public AuthService(
        IUserRepository userRepository,
        IJwtTokenGenerator tokenGenerator,
        IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _tokenGenerator = tokenGenerator;
        _passwordHasher = passwordHasher;
    }

    public async Task<string?> LoginAsync(string login, string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(login);
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        var user = await _userRepository.GetByLoginAsync(login).ConfigureAwait(false);
        if (user is null)
        {
            return null;
        }

        if (!_passwordHasher.Verify(password, user.PasswordHash))
        {
            return null;
        }

        return _tokenGenerator.GenerateToken(user.Id, user.Login);
    }

    public Task LogoutAsync(string token)
    {
        // JWT is stateless; clients should discard the token. No server-side blacklist in this flow.
        return Task.CompletedTask;
    }
}
