using Denarius.Domain.ValueObjects;

namespace Denarius.Domain.Entities;

public sealed class Tag : Entity
{
    public Name Name { get; private set; }
    public Color Color { get; private set; }

    public static Tag New(Name name, Color color)
    {
        return new Tag()
        {
            Id = Guid.NewGuid(),
            Name = name,
            Color = color
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
