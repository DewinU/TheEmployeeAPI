using FluentValidation;
using Microsoft.EntityFrameworkCore;

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


public class UpdateEmployeeRequestValidator : AbstractValidator<UpdateEmployeeRequest>
{
    private readonly HttpContext _httpContext;
    private readonly AppDbContext _context;

    public UpdateEmployeeRequestValidator(IHttpContextAccessor httpContextAccessor, AppDbContext context)
    {
        _httpContext = httpContextAccessor.HttpContext!;
        _context = context;

        RuleFor(x => x).MustAsync(EmployeeExistsAsync).WithMessage("Employee does not exist.").DependentRules(() =>
        {
            RuleFor(x => x.Address1).MustAsync(NotBeEmptyIfItIsSetOnEmployeeAlreadyAsync).WithMessage("Address1 cannot be empty.");
        });
    }

    private async Task<bool> EmployeeExistsAsync(UpdateEmployeeRequest updateEmployeeRequest, CancellationToken token)
    {
        var id = Convert.ToInt32(_httpContext.Request.RouteValues["id"]);
        var employee = await _context.Employees.SingleOrDefaultAsync(e => e.Id == id);
        return employee != null;
    }

    private async Task<bool> NotBeEmptyIfItIsSetOnEmployeeAlreadyAsync(string? address, CancellationToken token)
    {
        await Task.CompletedTask;   //again, we'll not make this async for now!
        var id = Convert.ToInt32(_httpContext.Request.RouteValues["id"]);
        var employee = await _context.Employees.SingleOrDefaultAsync(e => e.Id == id);

        if (employee!.Address1 != null && string.IsNullOrWhiteSpace(address))
        {
            return false;
        }

        return true;
    }

    
}