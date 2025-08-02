using Denarius.Application.Domain.Results.Accounts;
using Denarius.Domain.Models;

namespace Denarius.Application.Extensions;

internal static class AccountExtensions
{
    public static AccountResult ToResult(this Account account)
    {
        return new AccountResult
        {
            Id = account.Id,
            Name = account.Name,
            Color = account.Color,
            Balance = account.Balance,
            CreatedAt = account.CreatedAt,
            UpdatedAt = account.UpdatedAt
        };
    }
}
