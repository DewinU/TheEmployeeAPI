using TheEmployeeAPI.Abstractions;
using TheEmployeeAPI.Employees;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IRepository<Employee>, EmployeeRepository>();
builder.Services.AddProblemDetails();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// var employeeRoute = app.MapGroup("/employees");

// employeeRoute.MapGet(string.Empty, ([FromServices] IRepository<Employee> repository) =>
// {
//     return Results.Ok(repository.GetAll().Select(employee => new GetEmployeeResponse
//     {
//         FirstName = employee.FirstName,
//         LastName = employee.LastName,
//         Address1 = employee.Address1,
//         Address2 = employee.Address2,
//         City = employee.City,
//         State = employee.State,
//         ZipCode = employee.ZipCode,
//         PhoneNumber = employee.PhoneNumber,
//         Email = employee.Email
//     }));
// });

// employeeRoute.MapGet("{id:int}", ([FromRoute] int id, [FromServices] IRepository<Employee> repository) =>
// {
//     var employee = repository.GetById(id);
//     if (employee == null)
//     {
//         return Results.NotFound();
//     }

//     return Results.Ok(new GetEmployeeResponse
//     {
//         FirstName = employee.FirstName,
//         LastName = employee.LastName,
//         Address1 = employee.Address1,
//         Address2 = employee.Address2,
//         City = employee.City,
//         State = employee.State,
//         ZipCode = employee.ZipCode,
//         PhoneNumber = employee.PhoneNumber,
//         Email = employee.Email
//     });
// });

// employeeRoute.MapPost(string.Empty, async ([FromBody] CreateEmployeeRequest employeeRequest, [FromServices] IRepository<Employee> repository, [FromServices] IValidator<CreateEmployeeRequest> validator) =>
// {
//     // var validationProblems = new List<ValidationResult>();
//     // var isValid = Validator.TryValidateObject(employeeRequest, new ValidationContext(employeeRequest), validationProblems, true);
//     // if (!isValid)
//     // {
//     //     return Results.BadRequest(validationProblems.ToValidationProblemDetails());
//     // }

//     var validationResults = await validator.ValidateAsync(employeeRequest);
//     if (!validationResults.IsValid)
//     {
//         return Results.ValidationProblem(validationResults.ToDictionary());
//     }


//     var newEmployee = new Employee
//     {
//         FirstName = employeeRequest.FirstName!,
//         LastName = employeeRequest.LastName!,
//         SocialSecurityNumber = employeeRequest.SocialSecurityNumber!,
//         Address1 = employeeRequest.Address1,
//         Address2 = employeeRequest.Address2,
//         City = employeeRequest.City,
//         State = employeeRequest.State,
//         ZipCode = employeeRequest.ZipCode,
//         PhoneNumber = employeeRequest.PhoneNumber,
//         Email = employeeRequest.Email
//     };
//     repository.Create(newEmployee);
//     return Results.Created($"/employees/{newEmployee.Id}", employeeRequest);
// });

// employeeRoute.MapPut("{id:int}", ([FromRoute] int id, [FromBody] UpdateEmployeeRequest employeeRequest, [FromServices] IRepository<Employee> repository) =>
// {
//     var existingEmployee = repository.GetById(id);
//     if (existingEmployee == null)
//     {
//         return Results.NotFound();
//     }

//     existingEmployee.Address1 = employeeRequest.Address1;
//     existingEmployee.Address2 = employeeRequest.Address2;
//     existingEmployee.City = employeeRequest.City;
//     existingEmployee.State = employeeRequest.State;
//     existingEmployee.ZipCode = employeeRequest.ZipCode;
//     existingEmployee.PhoneNumber = employeeRequest.PhoneNumber;
//     existingEmployee.Email = employeeRequest.Email;

//     repository.Update(existingEmployee);
//     return Results.Ok(existingEmployee);
// });

app.MapControllers();

app.Run();

public partial class Program { }