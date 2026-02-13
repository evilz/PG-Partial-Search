using PGPartialSearch.ApiService.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();

// Add OpenAPI
builder.Services.AddOpenApi();

// Add PostgreSQL with EF Core
builder.AddNpgsqlDbContext<SearchDbContext>("searchdb");

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Enable static files
app.UseDefaultFiles();
app.UseStaticFiles();

// Seed the database
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<SearchDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    await DbSeeder.SeedAsync(context, logger);
}

// Search API endpoint
app.MapGet("/api/search", async (
    string query, 
    int page, 
    int pageSize,
    SearchDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(query))
    {
        return Results.BadRequest(new { error = "Query parameter is required" });
    }

    page = Math.Max(1, page);
    pageSize = Math.Clamp(pageSize, 1, 100);

    var filteredQuery = SearchQuery.Apply(db.People, query);

    var totalCount = await filteredQuery.CountAsync();

    var items = await filteredQuery
        .OrderBy(p => p.LastName)
        .ThenBy(p => p.FirstName)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .Select(p => new
        {
            p.Id,
            p.FirstName,
            p.LastName,
            p.Email,
            p.Phone,
            p.City
        })
        .ToListAsync();

    var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

    return Results.Ok(new
    {
        items,
        page,
        pageSize,
        totalCount,
        totalPages
    });
})
.WithName("SearchPeople")
.WithDescription("Search for people by first or last name with partial matching");

app.MapDefaultEndpoints();

app.Run();

