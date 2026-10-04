using Denarius.Application.IO.Reports;
using Denarius.Application.IO.Transactions;
using Denarius.Application.UseCases.Reports.ListMonthlyTransactions;
using Denarius.Domain.Entities;
using Denarius.Domain.Repositories;
using NSubstitute;

namespace Denarius.Application.Tests.UseCases.Reports.ListMonthlyTransactions;

public class ListMonthlyTransactionsUseCaseTests
{
    private static readonly YearMonth September = new(2026, 9);

    private readonly ITransactionRepository _transactionRepository = Substitute.For<ITransactionRepository>();
    private readonly TimeProvider _timeProvider = Substitute.For<TimeProvider>();
    private readonly IListMonthlyTransactionsUseCase _useCase;

    public ListMonthlyTransactionsUseCaseTests()
    {
        _useCase = new ListMonthlyTransactionsUseCase(_transactionRepository, _timeProvider);
        _timeProvider.GetUtcNow().Returns(new DateTimeOffset(2026, 10, 3, 15, 0, 0, TimeSpan.Zero));
        _transactionRepository.GetWithCategoryNameAsync(Arg.Any<DateOnly>(), Arg.Any<DateOnly>()).Returns([]);
    }

    private void Transactions(params (Transaction Transaction, string CategoryName)[] transactions) =>
        _transactionRepository.GetWithCategoryNameAsync(new DateOnly(2026, 9, 1), new DateOnly(2026, 10, 1)).Returns(transactions);

    [Fact]
    public async Task Execute_MapsEachTransactionWithItsCategoryNameTypeAndAmount()
    {
        var salario = new Transaction("Salário", new DateTime(2026, 9, 5, 0, 0, 0, DateTimeKind.Utc), 8000m, Guid.NewGuid());
        var feira = new Transaction(null, new DateTime(2026, 9, 3, 0, 0, 0, DateTimeKind.Utc), -50m, Guid.NewGuid());
        Transactions((feira, "Mercado"), (salario, "Salário"));

        var output = await _useCase.Execute(new ListMonthlyTransactionsInput(September));

        Assert.Equal(
            [
                new MonthlyTransactionOutput(salario, "Salário"),
                new MonthlyTransactionOutput(feira, "Mercado")
            ],
            output);
        var expense = output.Last();
        Assert.Equal(feira.Id, expense.Id);
        Assert.Equal(new DateTime(2026, 9, 3, 0, 0, 0, DateTimeKind.Utc), expense.Date);
        Assert.Null(expense.Description);
        Assert.Equal("Mercado", expense.CategoryName);
        Assert.Equal(TransactionType.Out, expense.Type);
        Assert.Equal(50m, expense.Amount);
        Assert.Equal(TransactionType.In, output.First().Type);
        Assert.Equal(8000m, output.First().Amount);
    }

    [Fact]
    public async Task Execute_OrdersByDateDescendingThenByTheMostRecentlyRecorded()
    {
        var categoryId = Guid.NewGuid();
        var first = new Transaction("Lançada primeiro", new DateTime(2026, 9, 10), -10m, categoryId) { CreatedAt = new DateTime(2026, 9, 10, 9, 0, 0) };
        var last = new Transaction("Lançada por último", new DateTime(2026, 9, 10), -10m, categoryId) { CreatedAt = new DateTime(2026, 9, 10, 18, 0, 0) };
        var older = new Transaction("Antiga", new DateTime(2026, 9, 1), -10m, categoryId) { CreatedAt = new DateTime(2026, 9, 20) };
        var newer = new Transaction("Recente", new DateTime(2026, 9, 28), -10m, categoryId) { CreatedAt = new DateTime(2026, 9, 1) };
        Transactions((older, "Mercado"), (first, "Mercado"), (newer, "Mercado"), (last, "Mercado"));

        var output = await _useCase.Execute(new ListMonthlyTransactionsInput(September));

        Assert.Equal(["Recente", "Lançada por último", "Lançada primeiro", "Antiga"], output.Select(o => o.Description));
    }

    [Fact]
    public async Task Execute_ManyTransactions_ReturnsEveryOne()
    {
        var categoryId = Guid.NewGuid();
        Transactions(Enumerable.Range(1, 30)
            .Select(day => (new Transaction($"Dia {day}", new DateTime(2026, 9, day), -day, categoryId), "Mercado"))
            .ToArray());

        var output = await _useCase.Execute(new ListMonthlyTransactionsInput(September));

        Assert.Equal(30, output.Count());
    }

    [Fact]
    public async Task Execute_MonthWithoutTransactions_ReturnsAnEmptyList()
    {
        var output = await _useCase.Execute(new ListMonthlyTransactionsInput(September));

        Assert.Empty(output);
    }

    [Fact]
    public async Task Execute_December_ReadsUpToTheFirstDayOfTheNextYear()
    {
        await _useCase.Execute(new ListMonthlyTransactionsInput(new YearMonth(2026, 12)));

        await _transactionRepository.Received(1).GetWithCategoryNameAsync(new DateOnly(2026, 12, 1), new DateOnly(2027, 1, 1));
    }

    [Fact]
    public async Task Execute_WithoutMonth_ReadsTheCurrentMonthInSaoPaulo()
    {
        _timeProvider.GetUtcNow().Returns(new DateTimeOffset(2026, 10, 1, 2, 0, 0, TimeSpan.Zero));

        await _useCase.Execute(new ListMonthlyTransactionsInput());

        await _transactionRepository.Received(1).GetWithCategoryNameAsync(new DateOnly(2026, 9, 1), new DateOnly(2026, 10, 1));
    }
}
