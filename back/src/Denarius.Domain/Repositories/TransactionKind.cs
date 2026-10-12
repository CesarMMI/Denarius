namespace Denarius.Domain.Repositories;

/// <summary>Which transactions a list keeps: all of them, only money in (positive values) or only money out (negative values).</summary>
public enum TransactionKind
{
    All,
    In,
    Out
}
