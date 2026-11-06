using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace TrabajaDecente.Api.Services;

public interface ITokenService
{
    string CreateAccessToken(int id, string rol, string nombre);
    (string Raw, string Hash, DateTime ExpiresUtc) CreateRefreshToken();
    string Hash(string rawToken);
}

public class TokenService : ITokenService
{
    private readonly JwtOptions _o;
    public TokenService(JwtOptions o) => _o = o;

    public string CreateAccessToken(int id, string rol, string nombre)
    {
        var claims = new[]
        {
            new Claim("sub", id.ToString()),
            new Claim("role", rol),
            new Claim("name", nombre),
            new Claim("jti", Guid.NewGuid().ToString())
        };
        var creds = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_o.Key)), SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(_o.Issuer, _o.Audience, claims,
            expires: DateTime.UtcNow.AddMinutes(_o.AccessMinutes), signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public (string Raw, string Hash, DateTime ExpiresUtc) CreateRefreshToken()
    {
        var raw = Base64UrlEncoder.Encode(RandomNumberGenerator.GetBytes(64));
        return (raw, Hash(raw), DateTime.UtcNow.AddDays(_o.RefreshDays));
    }

    // Solo el hash SHA-256 se guarda en la base de datos.
    public string Hash(string rawToken) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken))).ToLowerInvariant();
}
