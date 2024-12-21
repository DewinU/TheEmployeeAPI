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


    /// <summary>
    /// Get all employees.
    /// </summary>
    /// <returns>An array of all employees.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<GetEmployeeResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult GetAll()
    {
        var employees = _repository.GetAll().Select(EmployeeToGetEmployeeResponse);
        return Ok(employees);
    }


    /// <summary>
    /// Gets an employee by ID.
    /// </summary>
    /// <param name="id">The ID of the employee.</param>
    /// <returns>The single employee record.</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(GetEmployeeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult GetById([FromRoute] int id)
    {
        var employee = _repository.GetById(id);
        if (employee == null)
        {
            return NotFound();
        }

        return Ok(EmployeeToGetEmployeeResponse(employee));
    }


    /// <summary>
    /// Creates a new employee.
    /// </summary>
    /// <param name="employee">The employee to be created.</param>
    /// <returns>A link to the employee that was created.</returns>

    [HttpPost]
    [ProducesResponseType(typeof(GetEmployeeResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]

    public IActionResult Create([FromBody] CreateEmployeeRequest employee)
    {
        // var validationResults = await ValidateAsync(employee);
        // if (!validationResults.IsValid)
        // {
        //     return ValidationProblem(validationResults.ToModelStateDictionary());
        // }


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

    /// <summary>
    /// Updates an employee.
    /// </summary>
    /// <param name="id">The ID of the employee to update.</param>
    /// <param name="employee">The employee data to update.</param>
    /// <returns></returns>

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(GetEmployeeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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


    /// <summary>
    /// Gets the benefits for an employee.
    /// </summary>
    /// <param name="id">The ID to get the benefits for.</param>
    /// <returns>The benefits for that employee.</returns>
    [HttpGet("{id}/benefits")]
    [ProducesResponseType(typeof(IEnumerable<GetEmployeeResponseEmployeeBenefit>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult GetBenefitsForEmployee([FromRoute] int id)
    {
        var employee = _repository.GetById(id);
        if (employee == null)
        {
            return NotFound();
        }
        return Ok(employee.Benefits.Select(BenefitToBenefitResponse));
    }



    private static GetEmployeeResponse EmployeeToGetEmployeeResponse(Employee employee)
    {
        return new GetEmployeeResponse
        {
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            Address1 = employee.Address1,
            Address2 = employee.Address2,
            City = employee.City,
            State = employee.State,
            ZipCode = employee.ZipCode,
            PhoneNumber = employee.PhoneNumber,
            Email = employee.Email,
            Benefits = employee.Benefits.Select(BenefitToBenefitResponse).ToList()
        };
    }

    private static GetEmployeeResponseEmployeeBenefit BenefitToBenefitResponse(EmployeeBenefits benefit)
    {
        return new GetEmployeeResponseEmployeeBenefit
        {
            Id = benefit.Id,
            EmployeeId = benefit.EmployeeId,
            BenefitType = benefit.BenefitType,
            Cost = benefit.Cost
        };
    }

}
