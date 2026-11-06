using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace TrabajaDecente.Api.Middlewares;

/// <summary>Valida con FluentValidation todo argumento de acción que tenga un validador registrado.</summary>
public class ValidationFilter : IAsyncActionFilter
{
    private readonly IServiceProvider _sp;
    public ValidationFilter(IServiceProvider sp) => _sp = sp;

    public async Task OnActionExecutionAsync(ActionExecutingContext ctx, ActionExecutionDelegate next)
    {
        foreach (var arg in ctx.ActionArguments.Values)
        {
            if (arg is null) continue;
            var type = typeof(IValidator<>).MakeGenericType(arg.GetType());
            if (_sp.GetService(type) is not IValidator validator) continue;

            var result = await validator.ValidateAsync(new ValidationContext<object>(arg));
            if (!result.IsValid)
            {
                var first = result.Errors[0].ErrorMessage;
                ctx.Result = new BadRequestObjectResult(new { error = first, errores = result.ToDictionary() });
                return;
            }
        }
        await next();
    }
}
