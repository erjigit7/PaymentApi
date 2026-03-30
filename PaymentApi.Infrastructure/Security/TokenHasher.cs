using System.Security.Cryptography;
using System.Text;
using PaymentApi.Application.Interfaces;

namespace PaymentApi.Infrastructure.Security;

public sealed class TokenHasher : ITokenHasher
{
    public string Hash(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes);
    }
}