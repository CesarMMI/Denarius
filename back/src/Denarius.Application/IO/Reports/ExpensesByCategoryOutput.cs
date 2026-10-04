namespace Denarius.Application.IO.Reports;

public record ExpensesByCategoryOutput
{
    /// <summary>The month's money out, as a positive amount.</summary>
    public decimal Total { get; init; }
    /// <summary>Largest amount first; past eight categories, the seven largest and then "Outras".</summary>
    public IEnumerable<CategoryExpenseOutput> Items { get; init; }

    public ExpensesByCategoryOutput(decimal total, IEnumerable<CategoryExpenseOutput> items)
    {
        Total = total;
        Items = items;
    }
}
