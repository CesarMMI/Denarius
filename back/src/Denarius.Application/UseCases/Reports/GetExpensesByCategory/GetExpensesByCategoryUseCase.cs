using Denarius.Application.IO.Reports;
using Denarius.Domain.Repositories;

namespace Denarius.Application.UseCases.Reports.GetExpensesByCategory;

internal class GetExpensesByCategoryUseCase(ITransactionRepository transactionRepository, TimeProvider timeProvider) : IGetExpensesByCategoryUseCase
{
    /// <summary>Past eight categories, the smallest are added up into "Outras", so the list never has more than eight items.</summary>
    private const int MaxItems = 8;

    public async Task<ExpensesByCategoryOutput> Execute(GetExpensesByCategoryInput input)
    {
        var month = input.Month ?? YearMonth.FromDate(timeProvider.GetTodayInSaoPaulo());

        var expenses = (await transactionRepository.SumExpensesByCategoryAsync(month.FirstDay, month.AddMonths(1).FirstDay))
            .OrderByDescending(e => e.Expense)
            .ThenBy(e => e.Category.Name)
            .ToList();
        var total = expenses.Sum(e => e.Expense);

        var shown = expenses.Count > MaxItems ? expenses.Take(MaxItems - 1).ToList() : expenses;
        var items = shown
            .Select(e => new CategoryExpenseOutput(e.Category.Id, e.Category.Name, e.Category.Color.HexCode, e.Expense, Percentage(e.Expense, total)))
            .ToList();

        // The tail stays last, even when it adds up to more than some of the categories shown.
        if (shown.Count < expenses.Count)
        {
            var others = expenses.Skip(shown.Count).Sum(e => e.Expense);
            items.Add(new CategoryExpenseOutput(null, "Outras", null, others, Percentage(others, total)));
        }

        return new ExpensesByCategoryOutput(total, items);
    }

    private static decimal Percentage(decimal part, decimal whole) => Math.Round(part * 100 / whole, 2, MidpointRounding.AwayFromZero);
}
