

using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;

namespace TheEmployeeAPI;

[ApiController]
[Route("[controller]")]
public abstract class BaseController : Controller
{
    // protected async Task<ValidationResult> ValidateAsync<T>(T instance){
    //     var validator = HttpContext.RequestServices.GetService<IValidator<T>>();
    //     if (validator == null)
    //     {
    //          throw new InvalidOperationException($"No validator found for {typeof(T).Name}");
    //     }

    //     return await validator.ValidateAsync(instance);
    // }

}
