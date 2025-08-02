namespace Denarius.Application.Domain.Results.Accounts;

public readonly struct AccountResult
{
    public int Id { get; init; }
    public string Name { get; init; }
    public string? Color { get; init; }
    public decimal Balance { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}