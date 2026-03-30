namespace PaymentApi.Application.Interfaces;

public interface IJwtTokenGenerator
{
    GeneratedToken GenerateToken(Guid userId, string login);
}