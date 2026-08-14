using Denarius.Domain.Exceptions;

namespace Denarius.Domain.Entities;

public class Transaction : Entity
{
    public const int DescriptionMaxLength = 255;

    public string? Description { get; private set; }
    public DateTime Date { get; private set; }
    public decimal Value { get; private set; }
    public Guid CategoryId { get; private set; }

    public Transaction(string? description, DateTime date, decimal value, Guid categoryId) : base(Guid.NewGuid(), DateTime.UtcNow, DateTime.UtcNow)
    {
        Description = ValidateDescription(description);
        Date = ValidateDate(date);
        Value = ValidateValue(value);
        CategoryId = ValidateCategoryId(categoryId);
    }

    private Transaction(Guid id, DateTime createdAt, DateTime updatedAt, string? description, DateTime date, decimal value, Guid categoryId) : base(id, createdAt, updatedAt)
    {
        Description = description;
        Date = date;
        Value = value;
        CategoryId = categoryId;
    }

    public void Update(string? description, DateTime date, decimal value, Guid categoryId)
    {
        Description = ValidateDescription(description);
        Date = ValidateDate(date);
        Value = ValidateValue(value);
        CategoryId = ValidateCategoryId(categoryId);
        MarkAsUpdated();
    }

    private static string? ValidateDescription(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
            return null;

        description = description.Trim();

        if (description.Length > DescriptionMaxLength)
            throw new DomainException($"A descrição da transação não pode ter mais que {DescriptionMaxLength} caracteres.");

        return description;
    }

    private static DateTime ValidateDate(DateTime date)
    {
        if (date == default)
            throw new DomainException("A data da transação é obrigatória.");

        return date;
    }

    private static decimal ValidateValue(decimal value)
    {
        if (value == 0)
            throw new DomainException("O valor da transação não pode ser zero.");

        return value;
    }

    private static Guid ValidateCategoryId(Guid categoryId)
    {
        if (categoryId == Guid.Empty)
            throw new DomainException("A categoria da transação é obrigatória.");

        return categoryId;
    }
}
