using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AssignmentManagement.Api.Middleware;

public class ValidationFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var serviceProvider = context.HttpContext.RequestServices;

        foreach (var value in context.ActionArguments.Values)
        {
            if (value is null)
            {
                continue;
            }

            var validatorType = typeof(IValidator<>).MakeGenericType(value.GetType());
            var validator = serviceProvider.GetService(validatorType) as IValidator;

            if (validator is null)
            {
                continue;
            }

            var result = await validator.ValidateAsync(new ValidationContext<object>(value));

            if (!result.IsValid)
            {
                context.Result = new BadRequestObjectResult(new
                {
                    status = StatusCodes.Status400BadRequest,
                    message = string.Join("; ", result.Errors.Select(e => e.ErrorMessage))
                });

                return;
            }
        }

        await next();
    }
}
