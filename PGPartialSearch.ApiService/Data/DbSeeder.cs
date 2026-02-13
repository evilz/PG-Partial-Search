using Bogus;
using Microsoft.EntityFrameworkCore;

namespace PGPartialSearch.ApiService.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(SearchDbContext context, ILogger logger)
    {
        // Ensure database is created
        await context.Database.EnsureCreatedAsync();

        // Enable pg_trgm extension for trigram search
        await context.Database.ExecuteSqlRawAsync("CREATE EXTENSION IF NOT EXISTS pg_trgm;");

        // Check if data already exists
        if (await context.People.AnyAsync())
        {
            logger.LogInformation("Database already seeded with {Count} people", await context.People.CountAsync());
            return;
        }

        logger.LogInformation("Seeding database with random people data...");

        var faker = new Faker<Person>()
            .RuleFor(p => p.FirstName, f => f.Name.FirstName())
            .RuleFor(p => p.LastName, f => f.Name.LastName())
            .RuleFor(p => p.Email, (f, p) => f.Internet.Email(p.FirstName, p.LastName))
            .RuleFor(p => p.Phone, f => f.Phone.PhoneNumber())
            .RuleFor(p => p.City, f => f.Address.City());

        // Generate 10,000 random people
        var people = faker.Generate(10000);
        
        await context.People.AddRangeAsync(people);
        await context.SaveChangesAsync();

        logger.LogInformation("Database seeded successfully with {Count} people", people.Count);
    }
}
