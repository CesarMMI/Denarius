using Denarius.Domain.ValueObjects;

namespace Denarius.Domain.Entities;

public abstract class Entity(Identifier id)
{
    public Identifier Id { get; init; } = id;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? DeletedAt { get; private set; } = null;

    protected void TriggerUpdate()
    {
        UpdatedAt = DateTime.UtcNow;
    }
}