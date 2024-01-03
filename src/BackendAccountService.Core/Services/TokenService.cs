using System.Security.Cryptography;
using System.Text;

namespace BackendAccountService.Core.Services;

public class TokenService : ITokenService
{
    public string GenerateInviteToken()
    {
        var secureRandomString =  Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        
        return ToSHA512(secureRandomString);
    }
    
    private static string ToSHA512(string value)
    {
        using var sha = SHA512.Create();
    
        var bytes = Encoding.UTF8.GetBytes(value);
        var hash  = sha.ComputeHash(bytes);
 
        return Convert.ToBase64String(hash).Replace('+', '-').Replace('/', '_');
    }
}