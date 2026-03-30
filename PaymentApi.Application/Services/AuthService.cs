using PaymentApi.Application.Interfaces;
using PaymentApi.Domain.Entities;

namespace PaymentApi.Application.Services;

public sealed class AuthService : IAuthService
{
    private const int MaxFailedAttempts = 5;
    private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(5);

    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenGenerator _tokenGenerator;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUserSessionRepository _userSessionRepository;
    private readonly ITokenHasher _tokenHasher;

    public AuthService(
        IUserRepository userRepository,
        IJwtTokenGenerator tokenGenerator,
        IPasswordHasher passwordHasher,
        IUserSessionRepository userSessionRepository,
        ITokenHasher tokenHasher)
    {
        _userRepository = userRepository;
        _tokenGenerator = tokenGenerator;
        _passwordHasher = passwordHasher;
        _userSessionRepository = userSessionRepository;
        _tokenHasher = tokenHasher;
    }

    public async Task<LoginResult> LoginAsync(string login, string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(login);
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        var user = await _userRepository.GetByLoginAsync(login).ConfigureAwait(false);
        if (user is null)
        {
            return LoginResult.Fail("invalid_credentials", "Invalid login or password.");
        }

        if (user.LockoutEndUtc.HasValue && user.LockoutEndUtc.Value > DateTime.UtcNow)
        {
            return LoginResult.Fail(
                "user_locked",
                $"Too many failed attempts. Try again after {user.LockoutEndUtc:yyyy-MM-dd HH:mm:ss} UTC.");
        }

        if (!_passwordHasher.Verify(password, user.PasswordHash))
        {
            user.FailedLoginAttempts += 1;

            if (user.FailedLoginAttempts >= MaxFailedAttempts)
            {
                user.LockoutEndUtc = DateTime.UtcNow.Add(LockoutDuration);
                user.FailedLoginAttempts = 0;

                await _userRepository.UpdateAsync(user).ConfigureAwait(false);

                return LoginResult.Fail(
                    "user_locked",
                    $"Too many failed attempts. User is locked until {user.LockoutEndUtc:yyyy-MM-dd HH:mm:ss} UTC.");
            }

            await _userRepository.UpdateAsync(user).ConfigureAwait(false);

            return LoginResult.Fail(
                "invalid_credentials",
                $"Invalid login or password. Remaining attempts before lock: {MaxFailedAttempts - user.FailedLoginAttempts}.");
        }

        user.FailedLoginAttempts = 0;
        user.LockoutEndUtc = null;
        await _userRepository.UpdateAsync(user).ConfigureAwait(false);

        var generatedToken = _tokenGenerator.GenerateToken(user.Id, user.Login);

        var session = new UserSession
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            JwtId = generatedToken.JwtId,
            TokenHash = _tokenHasher.Hash(generatedToken.Token),
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = generatedToken.ExpiresAt
        };

        await _userSessionRepository.AddAsync(session).ConfigureAwait(false);

        return LoginResult.Ok(generatedToken.Token);
    }

    public async Task LogoutByJwtIdAsync(string jwtId)
    {
        if (string.IsNullOrWhiteSpace(jwtId))
            return;

        await _userSessionRepository.RevokeByJwtIdAsync(jwtId).ConfigureAwait(false);
    }
}