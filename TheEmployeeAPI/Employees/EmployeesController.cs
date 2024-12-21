using Microsoft.AspNetCore.Mvc;
using TheEmployeeAPI.Abstractions;

namespace TheEmployeeAPI.Employees;

public class EmployeesController : BaseController
{
    private readonly IRepository<Employee> _repository;
    private readonly ILogger<EmployeesController> _logger;

    public EmployeesController(ILogger<EmployeesController> logger, IRepository<Employee> repository)
    {
        _logger = logger;
        _repository = repository;

    }


    [HttpGet]
    public IActionResult GetAll()
    {

        return Ok(_repository.GetAll().Select(employee => new GetEmployeeResponse
        {
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            Address1 = employee.Address1,
            Address2 = employee.Address2,
            City = employee.City,
            State = employee.State,
            ZipCode = employee.ZipCode,
            PhoneNumber = employee.PhoneNumber,
            Email = employee.Email
        }));
    }

    [HttpGet("{id}")]
    public IActionResult GetById([FromRoute] int id)
    {
        var employee = _repository.GetById(id);
        if (employee == null)
        {
            return NotFound();
        }

        return Ok(new GetEmployeeResponse
        {
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            Address1 = employee.Address1,
            Address2 = employee.Address2,
            City = employee.City,
            State = employee.State,
            ZipCode = employee.ZipCode,
            PhoneNumber = employee.PhoneNumber,
            Email = employee.Email
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateEmployeeRequest employee)
    {
        var validationResults = await ValidateAsync(employee);
        if (!validationResults.IsValid)
        {
            return ValidationProblem(validationResults.ToModelStateDictionary());
        }


        var newEmployee = new Employee
        {
            FirstName = employee.FirstName!,
            LastName = employee.LastName!,
            SocialSecurityNumber = employee.SocialSecurityNumber!,
            Address1 = employee.Address1,
            Address2 = employee.Address2,
            City = employee.City,
            State = employee.State,
            ZipCode = employee.ZipCode,
            PhoneNumber = employee.PhoneNumber,
            Email = employee.Email
        };
        _repository.Create(newEmployee);
        return CreatedAtAction(nameof(GetById), new { id = newEmployee.Id }, newEmployee);
    }

    [HttpPut("{id}")]
    public IActionResult Update([FromRoute] int id, [FromBody] UpdateEmployeeRequest employee)
    {
        _logger.LogInformation("Updating employee with ID: {EmployeeId}", id);

        var existingEmployee = _repository.GetById(id);
        if (existingEmployee == null)
        {
            _logger.LogWarning("Employee with ID: {EmployeeId} not found", id);
            return NotFound();
        }

        _logger.LogDebug("Updating employee details for ID: {EmployeeId}", id);
        existingEmployee.Address1 = employee.Address1;
        existingEmployee.Address2 = employee.Address2;
        existingEmployee.City = employee.City;
        existingEmployee.State = employee.State;
        existingEmployee.ZipCode = employee.ZipCode;
        existingEmployee.PhoneNumber = employee.PhoneNumber;
        existingEmployee.Email = employee.Email;

        try
        {
            _repository.Update(existingEmployee);
            _logger.LogInformation("Employee with ID: {EmployeeId} successfully updated", id);
            return Ok(existingEmployee);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating employee with ID: {EmployeeId}", id);
            return StatusCode(500, "An error occurred while updating the employee");
        }

    }

}
