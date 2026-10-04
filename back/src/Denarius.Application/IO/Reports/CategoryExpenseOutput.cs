namespace Denarius.Application.IO.Reports;

/// <summary>A category's money out in a month, or "Outras", which adds up the smallest ones and has no id or color.</summary>
public record CategoryExpenseOutput
{
    public Guid? CategoryId { get; init; }
    public string CategoryName { get; init; }
    public string? Color { get; init; }
    public decimal Amount { get; init; }
    /// <summary>The share of the month's total, on a 0–100 scale.</summary>
    public decimal Percentage { get; init; }

    public CategoryExpenseOutput(Guid? categoryId, string categoryName, string? color, decimal amount, decimal percentage)
    {
        CategoryId = categoryId;
        CategoryName = categoryName;
        Color = color;
        Amount = amount;
        Percentage = percentage;
    }
}
