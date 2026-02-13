using Microsoft.EntityFrameworkCore;
using PGPartialSearch.ApiService.Data;

namespace PGPartialSearch.ApiService.Tests;

public class SearchQueryTests
{
    [Fact]
    public void Apply_UsesTrigramSimilarityPredicate()
    {
        using var context = CreateContext();

        var sql = SearchQuery.Apply(context.People, "Beatty").ToQueryString();

        Assert.Contains("\"FirstName\" %", sql);
        Assert.Contains("\"LastName\" %", sql);
        Assert.DoesNotContain("ILIKE", sql);
    }

    [Fact]
    public void Apply_WithBeattieQuery_GeneratesSimilaritySql()
    {
        using var context = CreateContext();

        var sql = SearchQuery.Apply(context.People, "Beattie").ToQueryString();

        Assert.Contains("@query='Beattie'", sql);
        Assert.Contains("% @query", sql);
    }

    private static SearchDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<SearchDbContext>()
            .UseNpgsql("Host=localhost;Database=searchdb")
            .Options;

        return new SearchDbContext(options);
    }
}
