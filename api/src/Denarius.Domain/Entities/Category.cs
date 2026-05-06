using Denarius.Domain.Enums;
using Denarius.Domain.Exceptions;

namespace Denarius.Domain.Entities;

public class Category
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string Name { get; private set; }
    public string Color { get; private set; }
    public CategoryType Type { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public Category(Guid userId, string name, string color, CategoryType type)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidNameException();

        if (string.IsNullOrWhiteSpace(color))
            throw new InvalidColorException();

        Id = Guid.NewGuid();
        UserId = userId;
        Name = name;
        Color = color;
        Type = type;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool AcceptsTransactionType(TransactionType transactionType) =>
        transactionType switch
        {
            TransactionType.Income => Type == CategoryType.Income,
            TransactionType.Expense => Type == CategoryType.Expense,
            _ => false
        };
}
