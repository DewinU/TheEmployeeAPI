using Microsoft.EntityFrameworkCore;
using TheEmployeeAPI;

public static class SeedData
{
    public static void MigrateAndSeed(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<AppDbContext>();
        context.Database.Migrate();

        if (!context.Employees.Any())
        {
            var employees = new List<Employee>
            {
                new() {
                    FirstName = "John",
                    LastName = "Doe",
                    SocialSecurityNumber = "123-45-6789",
                    Address1 = "123 Main St",
                    City = "Anytown",
                    State = "NY",
                    ZipCode = "12345",
                    PhoneNumber = "555-123-4567",
                    Email = "john.doe@example.com"
                },
                new() {
                    FirstName = "Jane",
                    LastName = "Smith",
                    SocialSecurityNumber = "987-65-4321",
                    Address1 = "456 Elm St",
                    Address2 = "Apt 2B",
                    City = "Othertown",
                    State = "CA",
                    ZipCode = "98765",
                    PhoneNumber = "555-987-6543",
                    Email = "jane.smith@example.com"
                }
            };

            context.Employees.AddRange(employees);
            context.SaveChanges();
        }

        if (!context.Benefits.Any())
        {
            var benefits = new List<Benefit>
            {
                new() { Name = "Health", Description = "Medical, dental, and vision coverage", BaseCost = 100.00m },
                new() { Name = "Dental", Description = "Dental coverage", BaseCost = 50.00m },
                new() { Name = "Vision", Description = "Vision coverage", BaseCost = 30.00m }
            };

            context.Benefits.AddRange(benefits);
            context.SaveChanges();

            // Add employee benefits too
            var healthBenefit = context.Benefits.AsNoTracking().Single(b => b.Name == "Health");
            var dentalBenefit = context.Benefits.AsNoTracking().Single(b => b.Name == "Dental");
            var visionBenefit = context.Benefits.AsNoTracking().Single(b => b.Name == "Vision");

            var john = context.Employees.AsNoTracking().Single(e => e.FirstName == "John");
            john.Benefits = new List<EmployeeBenefit>
            {
                new() { BenefitId = healthBenefit.Id, CostToEmployee = 100m },
                new() { BenefitId = dentalBenefit.Id }
            };

            var jane = context.Employees.AsNoTracking().Single(e => e.FirstName == "Jane");
            jane.Benefits = new List<EmployeeBenefit>
            {
                new() { BenefitId = healthBenefit.Id, CostToEmployee = 120m },
                new() { BenefitId = visionBenefit.Id }
            };

            // Detach any existing tracked instances
            var trackedJohn = context.Employees.Local.FirstOrDefault(e => e.Id == john.Id);
            if (trackedJohn != null)
            {
                context.Entry(trackedJohn).State = EntityState.Detached;
            }

            var trackedJane = context.Employees.Local.FirstOrDefault(e => e.Id == jane.Id);
            if (trackedJane != null)
            {
                context.Entry(trackedJane).State = EntityState.Detached;
            }

            // Attach entities to the context
            context.Attach(john);
            context.Attach(jane);
            context.Employees.UpdateRange(john, jane);
            context.SaveChanges();
        }
    }
}