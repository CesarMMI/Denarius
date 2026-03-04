using Denarius.Domain.ValueObjects;

namespace Denarius.Domain.Entities;

public sealed class Account : Entity
{
    public Name Name { get; private set; }
    public Color Color { get; private set; }
    public Money InitialBalance { get; init; }

    public static Account New(Name name, Color color, Money initialBalance)
    {
        return new Account()
        {
            Id = Guid.NewGuid(),
            Name = name,
            Color = color,
            InitialBalance = initialBalance
        };
    }

    public void Rename(Name name)
    {
        Name = name;
        TriggerUpdate();
    }

    public void ChangeColor(Color color)
    {
        Color = color;
        TriggerUpdate();
    }
}