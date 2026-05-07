using Denarius.Domain.Enums;
using Denarius.Domain.Exceptions.Transactions;

namespace Denarius.Domain.Entities;

public class Transaction
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid AccountId { get; private set; }
    public Guid? CategoryId { get; private set; }
    public Guid? TransferPeerId { get; private set; }
    public TransactionType Type { get; private set; }
    public decimal Amount { get; private set; }
    public string Description { get; private set; }
    public DateTime Date { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public bool IsTransfer => Type == TransactionType.Transfer;
    public bool IsIncomingTransfer { get; private set; }

    public Transaction(
        Guid userId,
        Guid accountId,
        Guid? categoryId,
        Guid? transferPeerId,
        TransactionType type,
        decimal amount,
        string description,
        DateTime date)
    {
        if (amount <= 0)
            throw new InvalidAmountException();

        if (string.IsNullOrWhiteSpace(description))
            throw new InvalidDescriptionException();

        if (type == TransactionType.Transfer)
        {
            if (categoryId.HasValue)
                throw new InvalidCategoryException("Transfer transactions cannot have a category.");

            if (!transferPeerId.HasValue)
                throw new InvalidTransferException("Transfer transactions must have a TransferPeerId.");
        }
        else
        {
            if (!categoryId.HasValue)
                throw new InvalidCategoryException("Income and Expense transactions must have a category.");
        }

        Id = Guid.NewGuid();
        UserId = userId;
        AccountId = accountId;
        CategoryId = categoryId;
        TransferPeerId = transferPeerId;
        Type = type;
        Amount = amount;
        Description = description;
        Date = date;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsIncoming()
    {
        if (!IsTransfer)
            throw new InvalidOperationException("Only Transfer transactions can be marked as incoming.");
        IsIncomingTransfer = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void LinkTransferPeer(Guid peerId)
    {
        if (!IsTransfer)
            throw new InvalidOperationException("Only Transfer transactions can have a peer.");
        TransferPeerId = peerId;
        UpdatedAt = DateTime.UtcNow;
    }

    public decimal ToDelta() =>
        Type switch
        {
            TransactionType.Income => Amount,
            TransactionType.Expense => -Amount,
            TransactionType.Transfer => -Amount,
            _ => throw new InvalidOperationException($"Unknown transaction type: {Type}")
        };

    public void UpdateAmount(decimal amount)
    {
        if (amount <= 0)
            throw new InvalidAmountException();

        Amount = amount;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new InvalidDescriptionException();

        Description = description;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateCategory(Guid? categoryId)
    {
        if (IsTransfer)
            throw new InvalidCategoryException("Cannot set category on a Transfer transaction.");

        if (!categoryId.HasValue)
            throw new InvalidCategoryException("Income and Expense transactions must have a category.");

        CategoryId = categoryId;
        UpdatedAt = DateTime.UtcNow;
    }
}
