using Denarius.Domain.Enums;
using Denarius.Domain.Exceptions.Transaction;
using Denarius.Domain.ValueObjects;

namespace Denarius.Domain.Entities;

public sealed class Transaction : Entity
{
    public Name Title { get; private set; }
    public DateOnly Date { get; private set; }
    public Money Amount { get; private set; }
    public ETransactionType Type { get; private set; }
    public Guid AccountId { get; private set; }
    public Guid? TagId { get; private set; }

    public static Transaction New(Name title, DateOnly date, Money amount, ETransactionType type, Guid accountId, Guid? tagId)
    {
        if (amount.Value <= 0) throw new ZeroOrNegativeTransactionAmountException();
        if (!Enum.IsDefined(type)) throw new InvalidTransactionTypeException();

        return new Transaction()
        {
            Id = Guid.NewGuid(),
            Title = title,
            Date = date,
            Amount = amount,
            Type = type,
            AccountId = accountId,
            TagId = tagId
        };
    }

    public void Retitle(Name title)
    {
        Title = title;
        TriggerUpdate();
    }

    public void ChangeDate(DateOnly date)
    {
        Date = date;
        TriggerUpdate();
    }

    public void ChangeAmount(Money amount)
    {
        Amount = amount;
        TriggerUpdate();
    }

    public void ChangeType(ETransactionType type)
    {
        Type = type;
        TriggerUpdate();
    }

    public void SetAccount(Guid accountId)
    {
        AccountId = accountId;
        TriggerUpdate();
    }

    public void SetTag(Guid? tagId)
    {
        TagId = tagId;
        TriggerUpdate();
    }
}