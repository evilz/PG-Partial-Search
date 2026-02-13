namespace PGPartialSearch.ApiService.Data;

public class Person
{
    public int Id { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public required string Phone { get; set; }
    public required string City { get; set; }
}


public sealed class PersonSearchResult
{
    public Person Person { get; init; } = default!;
    public double Score { get; init; }
}