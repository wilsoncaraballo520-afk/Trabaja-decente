namespace TrabajaDecente.Api.Helpers;

/// <summary>Error de negocio con código HTTP; el mensaje es seguro para mostrar al usuario.</summary>
public class AppException : Exception
{
    public int Status { get; }
    public AppException(int status, string message) : base(message) => Status = status;

    public static AppException BadRequest(string m) => new(400, m);
    public static AppException Unauthorized(string m) => new(401, m);
    public static AppException Forbidden(string m) => new(403, m);
    public static AppException NotFound(string m) => new(404, m);
    public static AppException Conflict(string m) => new(409, m);
}
