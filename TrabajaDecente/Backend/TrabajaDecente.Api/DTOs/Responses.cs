namespace TrabajaDecente.Api.DTOs;

public record AuthResponse(string AccessToken, int ExpiresIn, string Rol, string Nombre, string? EstadoAprobacion);

public record AuthResult(AuthResponse Response, string RefreshToken, DateTime RefreshExpires);

public record PagedResult<T>(IReadOnlyList<T> Items, int Total, int Pagina, int Tamano, int TotalPaginas);

public record MensajeResponse(string Mensaje, long? Id = null);
