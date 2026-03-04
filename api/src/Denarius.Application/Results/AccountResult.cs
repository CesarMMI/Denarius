using Denarius.Domain.Entities;

namespace Denarius.Application.Results;

public sealed record AccountResult
{
    public Guid Id { get; init; }
    public string Name { get; init; }
    public string Color { get; init; }
    public decimal Balance { get; init; }
    public string CurrencyCode { get; init; }

    public AccountResult(Account account)
    {
        Id = account.Id;
        Name = account.Name.Value;
        Color = account.Color.Value;
        Balance = account.InitialBalance.Value;
        CurrencyCode = account.InitialBalance.Code;
    }
}
