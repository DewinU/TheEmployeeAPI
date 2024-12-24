using TheEmployeeAPI.Employees;
using FluentValidation;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Extensions;
using TheEmployeeAPI;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "TheEmployeeAPI.xml"));
});

builder.Services.AddProblemDetails();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddFluentValidationAutoValidation();

builder.Services.AddControllers(options =>
{
    // options.Filters.Add<FluentValidationFilter>();
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"))
);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    SeedData.MigrateAndSeed(services);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();

public partial class Program { }