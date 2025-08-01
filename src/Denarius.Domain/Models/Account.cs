
namespace Denarius.Domain.Models;

public class Account
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;
    public string? Color { get; set; }
    public decimal Balance { get; set; } = 0;

    public int UserId { get; set; }
    public User User { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
