using Microsoft.EntityFrameworkCore;

namespace PGPartialSearch.ApiService.Data;

public static class SearchQuery
{
    public static IQueryable<Person> Apply(IQueryable<Person> queryable, string query) =>
        queryable.Where(p =>
            EF.Functions.TrigramsAreSimilar(p.FirstName, query) ||
            EF.Functions.TrigramsAreSimilar(p.LastName, query));
}
