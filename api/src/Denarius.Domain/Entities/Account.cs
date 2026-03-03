using Denarius.Domain.ValueObjects;

namespace Denarius.Domain.Entities;

public sealed class Account(Identifier id, Name name, Color color, Money initialBalance) : Entity(id)
{
    public Name Name { get; private set; } = name;
    public Color Color { get; private set; } = color;
    public Money InitialBalance { get; private set; } = initialBalance;

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