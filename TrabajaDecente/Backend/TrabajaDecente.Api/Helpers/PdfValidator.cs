using System.Text;
using System.Text.RegularExpressions;

namespace TrabajaDecente.Api.Helpers;

public static class PdfValidator
{
    public const long MaxBytes = 5 * 1024 * 1024; // 5 MB

    /// <summary>Valida el contenido real (firma %PDF-), no solo la extensión ni el Content-Type.</summary>
    public static void Validate(byte[] data)
    {
        if (data.Length == 0) throw AppException.BadRequest("El archivo esta vacio");
        if (data.Length > MaxBytes) throw AppException.BadRequest("El archivo supera 5 MB");
        var firma = Encoding.ASCII.GetString(data, 0, Math.Min(5, data.Length));
        if (firma != "%PDF-") throw AppException.BadRequest("Solo se permiten archivos PDF validos");
    }

    public static string SafeName(string? original)
    {
        var name = Path.GetFileName(original ?? "hoja-de-vida.pdf");
        name = Regex.Replace(name, @"[^\w\.\- ]", "_");
        if (name.Length > 150) name = name[^150..];
        if (!name.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase)) name += ".pdf";
        return name;
    }
}
