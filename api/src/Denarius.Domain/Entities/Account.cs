using Denarius.Domain.Exceptions;
using Denarius.Domain.Exceptions.Accounts;

namespace Denarius.Domain.Entities;

public class Account
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string Name { get; private set; }
    public string CurrencyCode { get; private set; }
    public decimal Balance { get; private set; }
    public string Color { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public Account(Guid userId, string name, string currencyCode, string color)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidNameException();

        if (!IsValidCurrencyCode(currencyCode))
            throw new InvalidCurrencyCodeException();

        if (string.IsNullOrWhiteSpace(color))
            throw new InvalidColorException();

        Id = Guid.NewGuid();
        UserId = userId;
        Name = name;
        CurrencyCode = currencyCode;
        Balance = 0;
        Color = color;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Update(string name, string color)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidNameException();

        if (string.IsNullOrWhiteSpace(color))
            throw new InvalidColorException();

        Name = name;
        Color = color;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ApplyDelta(decimal delta)
    {
        if (delta == 0)
            throw new InvalidDeltaException();

        Balance += delta;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    private static bool IsValidCurrencyCode(string? code) =>
        code is { Length: 3 } && code.All(c => char.IsLetter(c) && char.IsUpper(c));
}
