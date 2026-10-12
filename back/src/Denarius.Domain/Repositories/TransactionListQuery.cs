namespace Denarius.Domain.Repositories;

/// <summary>What <see cref="ITransactionRepository.ListAsync"/> filters and sorts by; every filter is optional and they combine with AND.</summary>
public record TransactionListQuery
{
    /// <summary>Already trimmed. Matches a part of the description, ignoring case and accents; <c>null</c> means no search.</summary>
    public string? Description { get; init; }

    /// <summary>Keeps transactions dated from here on (inclusive). UTC.</summary>
    public DateTime? From { get; init; }

    /// <summary>Keeps transactions dated before this instant (exclusive). UTC.</summary>
    public DateTime? To { get; init; }

    public TransactionKind Type { get; init; } = TransactionKind.All;

    public Guid? CategoryId { get; init; }

    public TransactionSortField SortBy { get; init; } = TransactionSortField.Date;

    public bool Ascending { get; init; }
}
