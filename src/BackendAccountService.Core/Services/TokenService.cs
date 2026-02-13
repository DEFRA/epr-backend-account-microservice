using System.Security.Cryptography;
using System.Text;

namespace BackendAccountService.Core.Services;

public class TokenService : ITokenService
{
    public string GenerateInviteToken()
    {
        // what a load of baloney. a simple guid would do
        var secureRandomString =  Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        
        return ToSHA512(secureRandomString);
    }
    
    private static string ToSHA512(string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        var hash = SHA512.HashData(bytes);

        return Convert.ToBase64String(hash).Replace('+', '-').Replace('/', '_');
    }
}