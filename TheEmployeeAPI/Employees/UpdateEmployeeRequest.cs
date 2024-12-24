using FluentValidation;

namespace TheEmployeeAPI.Employees;

public class UpdateEmployeeRequest
{
    public string? Address1 { get; set; }
    public string? Address2 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? ZipCode { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
}


// public class UpdateEmployeeRequestValidator : AbstractValidator<UpdateEmployeeRequest>
// {
//     private readonly HttpContext _httpContext;
//     private readonly IRepository<Employee> _repository;

//     public UpdateEmployeeRequestValidator(IHttpContextAccessor httpContextAccessor, IRepository<Employee> repository)
//     {
//         _httpContext = httpContextAccessor.HttpContext!;
//         _repository = repository;

//         RuleFor(x => x).MustAsync(EmployeeExistsAsync).WithMessage("Employee does not exist.").DependentRules(() =>
//         {
//             RuleFor(x => x.Address1).MustAsync(NotBeEmptyIfItIsSetOnEmployeeAlreadyAsync).WithMessage("Address1 cannot be empty.");
//         });
//     }

//     private async Task<bool> EmployeeExistsAsync(UpdateEmployeeRequest updateEmployeeRequest, CancellationToken token)
//     {
//         await Task.CompletedTask;   //we'll not make this async for now!
//         var id = Convert.ToInt32(_httpContext.Request.RouteValues["id"]);
//         return _repository.GetById(id) != null;
//     }

//     private async Task<bool> NotBeEmptyIfItIsSetOnEmployeeAlreadyAsync(string? address, CancellationToken token)
//     {
//         await Task.CompletedTask;   //again, we'll not make this async for now!
//         var id = Convert.ToInt32(_httpContext.Request.RouteValues["id"]);
//         var employee = _repository.GetById(id);


//         if (employee!.Address1 != null && string.IsNullOrWhiteSpace(address))
//         {
//             return false;
//         }

//         return true;
//     }

    
// }