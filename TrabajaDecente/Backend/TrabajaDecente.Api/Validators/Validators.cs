using FluentValidation;
using TrabajaDecente.Api.DTOs;
using TrabajaDecente.Api.Helpers;

namespace TrabajaDecente.Api.Validators;

internal static class Rules
{
    public const string PasswordPattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,72}$";
    public static readonly string[] TiposDocumento = { "CC", "TI", "CE", "PPT", "PEP", "Pasaporte" };
    public static readonly string[] TiposContrato = { "Laboral", "Practicas", "Prestacion de servicios", "Aprendizaje", "Temporal" };
    public static readonly string[] Modalidades = { "Presencial", "Remota", "Hibrida" };
    public static readonly string[] EstadosPostulacion = { "Aplicado", "En proceso", "Proceso finalizado" };
    public static readonly string[] Ordenes = { "recientes", "salario_desc", "salario_asc", "cierre" };

    public static IRuleBuilderOptions<T, string> Password<T>(this IRuleBuilder<T, string> rb) =>
        rb.NotEmpty().Matches(PasswordPattern)
          .WithMessage("La contrasena debe tener 8 a 72 caracteres, con mayuscula, minuscula y numero");

    public static IRuleBuilderOptions<T, string> Email<T>(this IRuleBuilder<T, string> rb) =>
        rb.NotEmpty().MaximumLength(254).EmailAddress().WithMessage("Correo electronico no valido");

    public static IRuleBuilderOptions<T, string> Phone<T>(this IRuleBuilder<T, string> rb) =>
        rb.NotEmpty().Matches(@"^\+?[0-9 ]{7,18}$").WithMessage("Telefono no valido");
}

public class LoginValidator : AbstractValidator<LoginRequest>
{
    public LoginValidator()
    {
        RuleFor(x => x.Rol).Must(r => Roles.Todos.Contains(r)).WithMessage("Rol no valido");
        RuleFor(x => x.Correo).NotEmpty().MaximumLength(254);
        RuleFor(x => x.Contrasena).NotEmpty().MaximumLength(72);
    }
}

public class RegistroEstudianteValidator : AbstractValidator<RegistroEstudianteRequest>
{
    public RegistroEstudianteValidator()
    {
        RuleFor(x => x.TipoDocumento).Must(t => Rules.TiposDocumento.Contains(t)).WithMessage("Tipo de documento no valido");
        RuleFor(x => x.NumeroDocumento).Matches(@"^[A-Za-z0-9]{5,20}$").WithMessage("Numero de documento no valido");
        RuleFor(x => x.Nombres).NotEmpty().MaximumLength(100);
        RuleFor(x => x.PrimerApellido).NotEmpty().MaximumLength(50);
        RuleFor(x => x.SegundoApellido).MaximumLength(50);
        RuleFor(x => x.FechaNacimiento)
            .LessThan(_ => DateTime.Today.AddYears(-14)).WithMessage("Debes tener al menos 14 anos")
            .GreaterThan(_ => DateTime.Today.AddYears(-100)).WithMessage("Fecha de nacimiento no valida");
        RuleFor(x => x.CorreoElectronico).Email();
        RuleFor(x => x.Telefono).Phone();
        RuleFor(x => x.Contrasena).Password();
    }
}

public class RegistroEmpresaValidator : AbstractValidator<RegistroEmpresaRequest>
{
    public RegistroEmpresaValidator()
    {
        RuleFor(x => x.NombreEmpresa).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Nit).Matches(@"^[0-9\-\.]{7,20}$").WithMessage("NIT no valido");
        RuleFor(x => x.CorreoElectronico).Email();
        RuleFor(x => x.Telefono).Phone();
        RuleFor(x => x.Direccion).MaximumLength(200);
        RuleFor(x => x.Contrasena).Password();
    }
}

public class OfertaValidator : AbstractValidator<OfertaRequest>
{
    public OfertaValidator()
    {
        RuleFor(x => x.IdCategoria).GreaterThan(0);
        RuleFor(x => x.IdCiudad).GreaterThan(0);
        RuleFor(x => x.Titulo).NotEmpty().MinimumLength(5).MaximumLength(150);
        RuleFor(x => x.Descripcion).NotEmpty().MinimumLength(20).MaximumLength(5000);
        RuleFor(x => x.Salario).InclusiveBetween(0m, 999_999_999m).When(x => x.Salario.HasValue);
        RuleFor(x => x.TipoContrato).Must(t => Rules.TiposContrato.Contains(t)).WithMessage("Tipo de contrato no valido");
        RuleFor(x => x.Modalidad).Must(m => Rules.Modalidades.Contains(m)).WithMessage("Modalidad no valida");
        RuleFor(x => x.Horario).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Vacantes).InclusiveBetween(1, 1000);
        RuleFor(x => x.ExperienciaMinimaMeses).InclusiveBetween(0, 600);
        RuleFor(x => x.FechaCierre).GreaterThan(_ => DateTime.Now).WithMessage("La fecha de cierre debe ser futura");
    }
}

public class OfertaFiltroValidator : AbstractValidator<OfertaFiltro>
{
    public OfertaFiltroValidator()
    {
        RuleFor(x => x.Q).MaximumLength(150);
        RuleFor(x => x.Modalidad).Must(m => string.IsNullOrEmpty(m) || Rules.Modalidades.Contains(m));
        RuleFor(x => x.TipoContrato).Must(t => string.IsNullOrEmpty(t) || Rules.TiposContrato.Contains(t));
        RuleFor(x => x.Orden).Must(o => string.IsNullOrEmpty(o) || Rules.Ordenes.Contains(o));
        RuleFor(x => x.Pagina).GreaterThanOrEqualTo(1);
        RuleFor(x => x.Tamano).InclusiveBetween(1, 50);
    }
}

public class ActualizarPerfilValidator : AbstractValidator<ActualizarPerfilRequest>
{
    public ActualizarPerfilValidator()
    {
        RuleFor(x => x.Nombres).NotEmpty().MaximumLength(100);
        RuleFor(x => x.PrimerApellido).NotEmpty().MaximumLength(50);
        RuleFor(x => x.SegundoApellido).MaximumLength(50);
        RuleFor(x => x.Telefono).Phone();
    }
}

public class EstadoPostulacionValidator : AbstractValidator<EstadoPostulacionRequest>
{
    public EstadoPostulacionValidator() =>
        RuleFor(x => x.Estado).Must(e => Rules.EstadosPostulacion.Contains(e)).WithMessage("Estado no valido");
}

public class CategoriaValidator : AbstractValidator<CategoriaRequest>
{
    public CategoriaValidator() =>
        RuleFor(x => x.NombreCategoria).NotEmpty().MinimumLength(3).MaximumLength(100);
}

public class PostularValidator : AbstractValidator<PostularRequest>
{
    public PostularValidator() => RuleFor(x => x.IdOferta).GreaterThan(0);
}
