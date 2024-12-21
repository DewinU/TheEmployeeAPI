using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using TheEmployeeAPI.Abstractions;
using TheEmployeeAPI.Employees;

namespace Tests;

public class BasicTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public BasicTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        var repo = _factory.Services.GetRequiredService<IRepository<Employee>>();
        repo.Create(new Employee
        {
            FirstName = "John",
            LastName = "Doe",
            SocialSecurityNumber = "123-45-6789",
            Address1 = "123 Main St",
            City = "Anytown",
            State = "NY",
            ZipCode = "12345",
            Benefits = new List<EmployeeBenefits>
            {
                new() {
                    BenefitType = BenefitType.Health, Cost = 100.00m
                },
                new() {
                    BenefitType = BenefitType.Dental, Cost = 50.00m
                },
            }
        });
    }


    [Fact]
    public async Task GetAllEmployees_ReturnsOkResult()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/employees");

        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task GetEmployeeById_ReturnsOkResult()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/employees/1");

        response.EnsureSuccessStatusCode();
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
        var employee = new Employee { FirstName = "John", LastName = "Doe", SocialSecurityNumber = "123-45-6789" };
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
        var response = await client.PutAsJsonAsync("/employees/1", new Employee { FirstName = "John", LastName = "Doe", SocialSecurityNumber = "123-45-6789", Address1 = "123 Main St", City = "Anytown", State = "NY", ZipCode = "12345" });

        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task UpdateEmployee_ReturnsNotFoundResult()
    {
        var client = _factory.CreateClient();
        var response = await client.PutAsJsonAsync("/employees/999", new Employee { FirstName = "John", LastName = "Doe", SocialSecurityNumber = "123-45-6789" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public void UpdateEmployee_LogsAndReturnsOkResult()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<EmployeesController>>().SetupAllProperties();
        var repositoryMock = new Mock<IRepository<Employee>>();
        var controllerMock = new EmployeesController(loggerMock.Object, repositoryMock.Object);

        var employeeId = 1;
        var updateRequest = new UpdateEmployeeRequest { Address1 = "123 Main St", City = "Anytown", State = "NY", ZipCode = "12345" };

        repositoryMock.Setup(r => r.GetById(employeeId)).Returns(new Employee { Id = employeeId, FirstName = "John", LastName = "Doe", SocialSecurityNumber = "123-45-6789" });

        // Act
        var result = controllerMock.Update(employeeId, updateRequest);

        // Assert
        Assert.IsType<OkObjectResult>(result);

        loggerMock.VerifyLog(logger => logger.LogInformation("Updating employee with ID: {EmployeeId}", employeeId));
        loggerMock.VerifyLog(logger => logger.LogInformation("Employee with ID: {EmployeeId} successfully updated", employeeId));
    }

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

