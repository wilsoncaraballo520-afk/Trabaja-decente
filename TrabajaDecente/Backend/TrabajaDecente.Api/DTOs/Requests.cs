namespace TrabajaDecente.Api.DTOs;

public class LoginRequest
{
    public string Rol { get; set; } = "";
    public string Correo { get; set; } = "";
    public string Contrasena { get; set; } = "";
}

public class RegistroEstudianteRequest
{
    public string TipoDocumento { get; set; } = "";
    public string NumeroDocumento { get; set; } = "";
    public string Nombres { get; set; } = "";
    public string PrimerApellido { get; set; } = "";
    public string? SegundoApellido { get; set; }
    public DateTime FechaNacimiento { get; set; }
    public string CorreoElectronico { get; set; } = "";
    public string Telefono { get; set; } = "";
    public string Contrasena { get; set; } = "";
}

public class RegistroEmpresaRequest
{
    public string NombreEmpresa { get; set; } = "";
    public string Nit { get; set; } = "";
    public string CorreoElectronico { get; set; } = "";
    public string Telefono { get; set; } = "";
    public string? Direccion { get; set; }
    public string Contrasena { get; set; } = "";
}

public class OfertaRequest
{
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
    public DateTime FechaCierre { get; set; }
}

public class OfertaFiltro
{
    public string? Q { get; set; }
    public int? Categoria { get; set; }
    public int? Ciudad { get; set; }
    public string? Modalidad { get; set; }
    public string? TipoContrato { get; set; }
    public decimal? SalarioMin { get; set; }
    public decimal? SalarioMax { get; set; }
    public string? Orden { get; set; }
    public int Pagina { get; set; } = 1;
    public int Tamano { get; set; } = 10;
}

public class ActualizarPerfilRequest
{
    public string Nombres { get; set; } = "";
    public string PrimerApellido { get; set; } = "";
    public string? SegundoApellido { get; set; }
    public string Telefono { get; set; } = "";
}

public class PostularRequest { public int IdOferta { get; set; } }
public class EstadoPostulacionRequest { public string Estado { get; set; } = ""; }
public class CategoriaRequest { public string NombreCategoria { get; set; } = ""; }
public class ActivoRequest { public bool Activo { get; set; } }
