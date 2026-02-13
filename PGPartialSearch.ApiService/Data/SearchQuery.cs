using Microsoft.EntityFrameworkCore;

namespace PGPartialSearch.ApiService.Data;

public static class SearchQuery
{
    public static IQueryable<PersonSearchResult> Apply(
        IQueryable<Person> queryable,
        string query)
    {
        return queryable
            .Where(p =>
                EF.Functions.TrigramsAreSimilar(p.FirstName, query) ||
                EF.Functions.TrigramsAreSimilar(p.LastName, query))
            .Select(p => new PersonSearchResult
            {
                Person = p,

                Score = Math.Max(
                    EF.Functions.TrigramsSimilarity(p.FirstName, query),
                    EF.Functions.TrigramsSimilarity(p.LastName, query)
                )
            })
            .OrderByDescending(x => x.Score);
    }
}
