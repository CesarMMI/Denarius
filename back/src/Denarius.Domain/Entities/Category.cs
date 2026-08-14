using Denarius.Domain.Exceptions;
using Denarius.Domain.ValueObjects;

namespace Denarius.Domain.Entities;

public class Category : Entity
{
    public const int NameMaxLength = 100;

    public string Name { get; private set; }
    public Color Color { get; private set; }

    public Category(string name, Color color) : base(Guid.NewGuid(), DateTime.UtcNow, DateTime.UtcNow)
    {
        Name = ValidateName(name);
        Color = color;
    }

    private Category(Guid id, DateTime createdAt, DateTime updatedAt, string name, Color color) : base(id, createdAt, updatedAt)
    {
        Name = name;
        Color = color;
    }

    public void Update(string name, Color color)
    {
        Name = ValidateName(name);
        Color = color;
        MarkAsUpdated();
    }

    private static string ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("O nome da categoria não pode ser vazio.");

        name = name.Trim();

        if (name.Length > NameMaxLength)
            throw new DomainException($"O nome da categoria não pode ter mais que {NameMaxLength} caracteres.");

        return name;
    }
}
