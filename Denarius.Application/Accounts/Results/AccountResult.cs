using Denarius.Domain.Models;

namespace Denarius.Application.Accounts.Results;

public class AccountResult
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Color { get; set; }
    public decimal Balance { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public static class AccountResultExtensions
{
    public static AccountResult ToAccountResult(this Account account)
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