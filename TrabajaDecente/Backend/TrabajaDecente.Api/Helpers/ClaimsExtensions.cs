using System.Security.Claims;

namespace TrabajaDecente.Api.Helpers;

public static class ClaimsExtensions
{
    /// <summary>La identidad SIEMPRE sale del token, nunca de IDs enviados por el cliente (anti-IDOR).</summary>
    public static int GetUserId(this ClaimsPrincipal user) =>
        int.TryParse(user.FindFirstValue("sub"), out var id)
            ? id
            : throw AppException.Unauthorized("Sesion invalida");
}
