using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;

namespace TrabajaDecente.Api.Data;

public interface IDbExecutor
{
    Task<List<T>> QueryAsync<T>(string procedure, params object?[] args) where T : new();
    Task<T?> FirstOrDefaultAsync<T>(string procedure, params object?[] args) where T : class, new();
    Task ExecuteAsync(string procedure, params object?[] args);
}

/// <summary>
/// Ejecuta procedimientos almacenados con parámetros (nunca concatena valores en el SQL)
/// y mapea las columnas del resultado a las propiedades de T por nombre.
/// </summary>
public class DbExecutor : IDbExecutor
{
    private static readonly Regex ProcName = new(@"^sp_[A-Za-z0-9_]+$", RegexOptions.Compiled);
    private readonly AppDbContext _db;
    public DbExecutor(AppDbContext db) => _db = db;

    public async Task<List<T>> QueryAsync<T>(string procedure, params object?[] args) where T : new()
    {
        var conn = _db.Database.GetDbConnection();
        var opened = await OpenAsync(conn);
        try
        {
            await using var cmd = Build(conn, procedure, args);
            await using var reader = await cmd.ExecuteReaderAsync();
            return await RowMapper<T>.ReadAsync(reader);
        }
        finally { if (opened) await conn.CloseAsync(); }
    }

    public async Task<T?> FirstOrDefaultAsync<T>(string procedure, params object?[] args) where T : class, new()
        => (await QueryAsync<T>(procedure, args)).FirstOrDefault();

    public async Task ExecuteAsync(string procedure, params object?[] args)
    {
        var conn = _db.Database.GetDbConnection();
        var opened = await OpenAsync(conn);
        try
        {
            await using var cmd = Build(conn, procedure, args);
            await cmd.ExecuteNonQueryAsync();
        }
        finally { if (opened) await conn.CloseAsync(); }
    }

    private static async Task<bool> OpenAsync(DbConnection conn)
    {
        if (conn.State == ConnectionState.Open) return false;
        await conn.OpenAsync();
        return true;
    }

    private static DbCommand Build(DbConnection conn, string procedure, object?[] args)
    {
        if (!ProcName.IsMatch(procedure)) throw new ArgumentException("Procedimiento no valido");
        var cmd = conn.CreateCommand();
        var names = new string[args.Length];
        for (var i = 0; i < args.Length; i++)
        {
            var p = cmd.CreateParameter();
            p.ParameterName = "@a" + i;
            p.Value = args[i] ?? DBNull.Value;
            cmd.Parameters.Add(p);
            names[i] = p.ParameterName;
        }
        cmd.CommandType = CommandType.Text;
        cmd.CommandText = $"CALL {procedure}({string.Join(",", names)})";
        return cmd;
    }
}

internal static class RowMapper<T> where T : new()
{
    private static readonly Dictionary<string, PropertyInfo> Props = typeof(T)
        .GetProperties(BindingFlags.Public | BindingFlags.Instance)
        .Where(p => p.CanWrite)
        .ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);

    public static async Task<List<T>> ReadAsync(DbDataReader reader)
    {
        var map = new List<(int Ordinal, PropertyInfo Prop)>();
        for (var i = 0; i < reader.FieldCount; i++)
            if (Props.TryGetValue(reader.GetName(i), out var p)) map.Add((i, p));

        var list = new List<T>();
        while (await reader.ReadAsync())
        {
            var item = new T();
            foreach (var (ordinal, prop) in map)
            {
                if (await reader.IsDBNullAsync(ordinal)) continue;
                prop.SetValue(item, ToTarget(reader.GetValue(ordinal), prop.PropertyType));
            }
            list.Add(item);
        }
        return list;
    }

    private static object ToTarget(object value, Type type)
    {
        var target = Nullable.GetUnderlyingType(type) ?? type;
        if (target.IsInstanceOfType(value)) return value;
        if (target == typeof(bool)) return Convert.ToBoolean(value, CultureInfo.InvariantCulture);
        return Convert.ChangeType(value, target, CultureInfo.InvariantCulture);
    }
}
