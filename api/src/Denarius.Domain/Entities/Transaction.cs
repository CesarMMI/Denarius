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
    public Account Account { get; private set; }
    public Tag? Tag { get; private set; }

    public Transaction(Identifier id, Name title, DateOnly date, Money amount, ETransactionType type, Account account,
        Tag? tag) : base(id)
    {
        if (amount.Value <= 0) throw new ZeroOrNegativeTransactionAmountException();
        if (!Enum.IsDefined(type)) throw new InvalidTransactionTypeException();

        Title = title;
        Date = date;
        Amount = amount;
        Type = type;
        Account = account;
        Tag = tag;
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

    public void SetAccount(Account account)
    {
        Account = account;
        TriggerUpdate();
    }

    public void SetTag(Tag? tag)
    {
        Tag = tag;
        TriggerUpdate();
    }
}