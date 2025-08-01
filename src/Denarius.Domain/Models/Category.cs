using Denarius.Domain.Enums;

namespace Denarius.Domain.Models;

public class Category
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;
    public string? Color { get; set; }
    public ECategoryType Type { get; set; }

    public int UserId { get; set; }
    public User User { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
