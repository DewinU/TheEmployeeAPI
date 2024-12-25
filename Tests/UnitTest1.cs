using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using TheEmployeeAPI;
using TheEmployeeAPI.Employees;

namespace Tests;

public class BasicTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public BasicTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }


    [Fact]
    public async Task GetAllEmployees_ReturnsOkResult()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/employees");

        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            throw new Exception($"Failed to get employees: {content}");
        }

        var employees = await response.Content.ReadFromJsonAsync<IEnumerable<GetEmployeeResponse>>() ?? [];
        Assert.NotEmpty(employees);
    }

    [Fact]
    public async Task GetAllEmployees_WithFilter_ReturnsOneResult()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/employees?FirstNameContains=John");

        response.EnsureSuccessStatusCode();

        var employees = await response.Content.ReadFromJsonAsync<IEnumerable<GetEmployeeResponse>>() ?? [];
        Assert.Single(employees);
    }

    [Fact]
    public async Task GetEmployeeById_ReturnsOkResult()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/employees/1");

        response.EnsureSuccessStatusCode();

        var employee = await response.Content.ReadFromJsonAsync<GetEmployeeResponse>() ?? null;
        Assert.NotNull(employee);
    }


    [Fact]
    public async Task GetEmployeeById_ReturnsNotFoundResult()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/employees/999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateEmployee_ReturnsCreatedResult()
    {
        var client = _factory.CreateClient();
        var employee = new Employee { FirstName = "Test", LastName = "Test", SocialSecurityNumber = "123-45-6789" };
        var response = await client.PostAsJsonAsync("/employees", employee);

        response.EnsureSuccessStatusCode();
    }
    [Fact]
    public async Task CreateEmployee_ReturnsBadRequestResult()
    {
        // Arrange
        var client = _factory.CreateClient();
        var invalidEmployee = new CreateEmployeeRequest(); // Empty object to trigger validation errors

        // Act
        var response = await client.PostAsJsonAsync("/employees", invalidEmployee);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(problemDetails);
        Assert.Contains("FirstName", problemDetails.Errors.Keys);
        Assert.Contains("LastName", problemDetails.Errors.Keys);
        Assert.Contains("'First Name' must not be empty.", problemDetails.Errors["FirstName"]);
        Assert.Contains("'Last Name' must not be empty.", problemDetails.Errors["LastName"]);
    }

    [Fact]
    public async Task UpdateEmployee_ReturnsOkResult()
    {
        var client = _factory.CreateClient();
        var response = await client.PutAsJsonAsync("/employees/1", new Employee
        {
            FirstName = "John",
            LastName = "Doe",
            SocialSecurityNumber = "123-45-6789",
            Address1 = "123 Main Smoot"
        });

        response.EnsureSuccessStatusCode();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var employee = await db.Employees.FindAsync(1) ?? null;
        Assert.NotNull(employee);
        Assert.Equal("123 Main Smoot", employee.Address1);
    }

    [Fact]
    public async Task UpdateEmployee_ReturnsNotFoundResult()
    {
        var client = _factory.CreateClient();
        var response = await client.PutAsJsonAsync("/employees/999", new Employee { FirstName = "John", LastName = "Doe", SocialSecurityNumber = "123-45-6789" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(problemDetails);

        Assert.Contains("", problemDetails.Errors.Keys);
        Assert.Contains("Employee does not exist.", problemDetails.Errors[""]);

    }

    // [Fact]
    // public void UpdateEmployee_LogsAndReturnsOkResult()
    // {
    //     // Arrange
    //     var loggerMock = new Mock<ILogger<EmployeesController>>().SetupAllProperties();
    //     var repositoryMock = new Mock<IRepository<Employee>>();
    //     var controllerMock = new EmployeesController(loggerMock.Object, repositoryMock.Object);

    //     var employeeId = 1;
    //     var updateRequest = new UpdateEmployeeRequest { Address1 = "123 Main St", City = "Anytown", State = "NY", ZipCode = "12345" };

    //     repositoryMock.Setup(r => r.GetById(employeeId)).Returns(new Employee { Id = employeeId, FirstName = "John", LastName = "Doe", SocialSecurityNumber = "123-45-6789" });

    //     // Act
    //     var result = controllerMock.Update(employeeId, updateRequest);

    //     // Assert
    //     Assert.IsType<OkObjectResult>(result);

    //     loggerMock.VerifyLog(logger => logger.LogInformation("Updating employee with ID: {EmployeeId}", employeeId));
    //     loggerMock.VerifyLog(logger => logger.LogInformation("Employee with ID: {EmployeeId} successfully updated", employeeId));
    // }

    [Fact]
    public async Task UpdateEmployee_ReturnsBadRequestWhenAddress()
    {
        // Arrange
        var client = _factory.CreateClient();
        var invalidEmployee = new UpdateEmployeeRequest(); // Empty object to trigger validation errors

        // Act
        var response = await client.PutAsJsonAsync("/employees/1", invalidEmployee);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(problemDetails);
        Assert.Contains("Address1", problemDetails.Errors.Keys);
    }

    [Fact]
    public async Task DeleteEmployee_ReturnsNoContentResult()
    {
        var client = _factory.CreateClient();

        var newEmployee = new Employee { FirstName = "Meow", LastName = "Garita", SocialSecurityNumber = "123-45-6789" };
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Employees.Add(newEmployee);
            await db.SaveChangesAsync();
        }

        var response = await client.DeleteAsync($"/employees/{newEmployee.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task DeleteEmployee_ReturnsNotFoundResult()
    {
        var client = _factory.CreateClient();
        var response = await client.DeleteAsync("/employees/99999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetBenefitsForEmployee_ReturnsOkResult()
    {
        // Act
        var client = _factory.CreateClient();
        var response = await client.GetAsync($"/employees/1/benefits");

        // Assert
        response.EnsureSuccessStatusCode();

        var benefits = await response.Content.ReadFromJsonAsync<IEnumerable<GetEmployeeResponseEmployeeBenefit>>();
        Assert.NotNull(benefits);
        Assert.Equal(2, benefits.Count());
    }

}

