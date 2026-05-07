using Denarius.Domain.Entities;

namespace Denarius.Application.Outputs.Accounts;

public record AccountOutput(
    Guid Id,
    string Name,
    string CurrencyCode,
    decimal Balance,
    string Color,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt)
{
    public static AccountOutput FromEntity(Account account) => new(
        account.Id,
        account.Name,
        account.CurrencyCode,
        account.Balance,
        account.Color,
        account.IsActive,
        account.CreatedAt,
        account.UpdatedAt);
}
