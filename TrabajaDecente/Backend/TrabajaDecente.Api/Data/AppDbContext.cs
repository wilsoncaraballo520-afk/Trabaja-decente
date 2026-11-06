using Microsoft.EntityFrameworkCore;

namespace TrabajaDecente.Api.Data;

/// <summary>
/// Toda la lógica de datos vive en los procedimientos almacenados de la base TrabajaDecente.
/// El contexto solo administra la conexión (Pomelo + MySqlConnector); no se mapean entidades
/// para no alterar el esquema existente.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
}
