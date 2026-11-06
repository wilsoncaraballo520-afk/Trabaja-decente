namespace TrabajaDecente.Api.Models;

// Filas devueltas por los procedimientos almacenados. Los nombres coinciden con las columnas.

public class CredencialRow
{
    public int IdUsuario { get; set; }
    public string Nombre { get; set; } = "";
    public string CorreoElectronico { get; set; } = "";
    public string ContrasenaHash { get; set; } = "";
    public bool Estado { get; set; }
    public string? EstadoAprobacion { get; set; }
}

public class SesionRow
{
    public int IdUsuario { get; set; }
    public string Nombre { get; set; } = "";
    public bool Estado { get; set; }
    public string? EstadoAprobacion { get; set; }
}

public class RefreshTokenRow
{
    public long IdRefreshToken { get; set; }
    public string Rol { get; set; } = "";
    public int IdUsuario { get; set; }
    public DateTime FechaExpiracion { get; set; }
    public bool Revocado { get; set; }
    public long? SegundosDesdeRevocacion { get; set; }
}

public class NuevoIdRow
{
    public long? IdEstudiante { get; set; }
    public long? IdEmpresa { get; set; }
    public long? IdOferta { get; set; }
    public long? IdPostulacion { get; set; }
    public long? IdCategoria { get; set; }
    public long Valor => IdEstudiante ?? IdEmpresa ?? IdOferta ?? IdPostulacion ?? IdCategoria ?? 0;
}

public class OfertaRow
{
    public int IdOferta { get; set; }
    public string Titulo { get; set; } = "";
    public string Descripcion { get; set; } = "";
    public decimal? Salario { get; set; }
    public string TipoContrato { get; set; } = "";
    public string Modalidad { get; set; } = "";
    public string Horario { get; set; } = "";
    public int Vacantes { get; set; }
    public int ExperienciaMinimaMeses { get; set; }
    public DateTime FechaPublicacion { get; set; }
    public DateTime FechaCierre { get; set; }
    public int IdEmpresa { get; set; }
    public string NombreEmpresa { get; set; } = "";
    public int IdCategoria { get; set; }
    public string NombreCategoria { get; set; } = "";
    public int IdCiudad { get; set; }
    public string NombreCiudad { get; set; } = "";
    public string NombreDepartamento { get; set; } = "";
}

public class FavoritaRow : OfertaRow
{
    public DateTime FechaGuardado { get; set; }
}

public class OfertaEmpresaRow
{
    public int IdOferta { get; set; }
    public int IdCategoria { get; set; }
    public int IdCiudad { get; set; }
    public string Titulo { get; set; } = "";
    public string Descripcion { get; set; } = "";
    public decimal? Salario { get; set; }
    public string TipoContrato { get; set; } = "";
    public string Modalidad { get; set; } = "";
    public string Horario { get; set; } = "";
    public int Vacantes { get; set; }
    public int ExperienciaMinimaMeses { get; set; }
    public DateTime FechaPublicacion { get; set; }
    public DateTime FechaCierre { get; set; }
    public string Estado { get; set; } = "";
    public string NombreCategoria { get; set; } = "";
    public string NombreCiudad { get; set; } = "";
    public long TotalPostulaciones { get; set; }
}

public class CategoriaRow
{
    public int IdCategoria { get; set; }
    public string NombreCategoria { get; set; } = "";
    public bool Estado { get; set; } = true;
    public long TotalOfertas { get; set; }
}

public class CiudadRow
{
    public int IdCiudad { get; set; }
    public string NombreCiudad { get; set; } = "";
    public string NombreDepartamento { get; set; } = "";
}

public class PerfilEstudianteRow
{
    public int IdEstudiante { get; set; }
    public string TipoDocumento { get; set; } = "";
    public string NumeroDocumento { get; set; } = "";
    public string Nombres { get; set; } = "";
    public string PrimerApellido { get; set; } = "";
    public string? SegundoApellido { get; set; }
    public DateTime FechaNacimiento { get; set; }
    public string CorreoElectronico { get; set; } = "";
    public string Telefono { get; set; } = "";
    public DateTime FechaRegistro { get; set; }
}

public class PostulacionEstudianteRow
{
    public int IdPostulacion { get; set; }
    public DateTime FechaPostulacion { get; set; }
    public string EstadoPostulacion { get; set; } = "";
    public int IdOferta { get; set; }
    public string Titulo { get; set; } = "";
    public decimal? Salario { get; set; }
    public string Modalidad { get; set; } = "";
    public string TipoContrato { get; set; } = "";
    public string NombreEmpresa { get; set; } = "";
    public string NombreCiudad { get; set; } = "";
}

public class PostulacionEmpresaRow
{
    public int IdPostulacion { get; set; }
    public DateTime FechaPostulacion { get; set; }
    public string Estado { get; set; } = "";
    public int IdEstudiante { get; set; }
    public string Nombres { get; set; } = "";
    public string PrimerApellido { get; set; } = "";
    public string? SegundoApellido { get; set; }
    public string CorreoElectronico { get; set; } = "";
    public string Telefono { get; set; } = "";
    public string Titulo { get; set; } = "";
    public bool TieneHojaDeVida { get; set; }
}

public class HojaVidaMetaRow
{
    public int IdHojaDeVida { get; set; }
    public string NombreArchivo { get; set; } = "";
    public string TipoArchivo { get; set; } = "";
    public long TamanoBytes { get; set; }
    public DateTime FechaCarga { get; set; }
}

public class HojaVidaArchivoRow
{
    public string NombreArchivo { get; set; } = "";
    public string TipoArchivo { get; set; } = "";
    public long TamanoBytes { get; set; }
    public byte[] ArchivoPDF { get; set; } = Array.Empty<byte>();
}

public class ResumenEstudianteRow
{
    public long TotalPostulaciones { get; set; }
    public long EnProceso { get; set; }
    public long TotalFavoritas { get; set; }
    public bool TieneHojaDeVida { get; set; }
}

public class ResumenEmpresaRow
{
    public long OfertasActivas { get; set; }
    public long TotalPostulaciones { get; set; }
}

public class ResumenAdminRow
{
    public long EmpresasPendientes { get; set; }
    public long EmpresasAprobadas { get; set; }
    public long OfertasActivas { get; set; }
    public long EstudiantesActivos { get; set; }
}

public class EmpresaAdminRow
{
    public int IdEmpresa { get; set; }
    public string NombreEmpresa { get; set; } = "";
    public string NIT { get; set; } = "";
    public string CorreoElectronico { get; set; } = "";
    public string Telefono { get; set; } = "";
    public bool Estado { get; set; }
    public string EstadoAprobacion { get; set; } = "";
    public DateTime FechaRegistro { get; set; }
}
