using Denarius.Application.IO.Reports;
using Denarius.Application.UseCases.Reports.GetExpensesByCategory;
using Denarius.Domain.Entities;
using Denarius.Domain.Repositories;
using Denarius.Domain.ValueObjects;
using NSubstitute;

namespace Denarius.Application.Tests.UseCases.Reports.GetExpensesByCategory;

public class GetExpensesByCategoryUseCaseTests
{
    private static readonly YearMonth September = new(2026, 9);

    private readonly ITransactionRepository _transactionRepository = Substitute.For<ITransactionRepository>();
    private readonly TimeProvider _timeProvider = Substitute.For<TimeProvider>();
    private readonly IGetExpensesByCategoryUseCase _useCase;

    public GetExpensesByCategoryUseCaseTests()
    {
        _useCase = new GetExpensesByCategoryUseCase(_transactionRepository, _timeProvider);
        _timeProvider.GetUtcNow().Returns(new DateTimeOffset(2026, 10, 3, 15, 0, 0, TimeSpan.Zero));
        _transactionRepository.SumExpensesByCategoryAsync(Arg.Any<DateOnly>(), Arg.Any<DateOnly>()).Returns([]);
    }

    private static Category Category(string name, string color = "#43A047") => new(name, new Color(color));

    private void Expenses(params (Category Category, decimal Expense)[] expenses) =>
        _transactionRepository.SumExpensesByCategoryAsync(new DateOnly(2026, 9, 1), new DateOnly(2026, 10, 1)).Returns(expenses);

    [Fact]
    public async Task Execute_MonthWithoutExpenses_ReturnsAZeroTotalAndNoItems()
    {
        var output = await _useCase.Execute(new GetExpensesByCategoryInput(September));

        Assert.Equal(0m, output.Total);
        Assert.Empty(output.Items);
    }

    [Fact]
    public async Task Execute_ListsTheCategoriesLargestFirstWithTheirShareOfTheTotal()
    {
        var mercado = Category("Mercado", "#43A047");
        var lazer = Category("Lazer", "#FDD835");
        var transporte = Category("Transporte", "#00ACC1");
        Expenses((lazer, 300m), (transporte, 100m), (mercado, 600m));

        var output = await _useCase.Execute(new GetExpensesByCategoryInput(September));

        Assert.Equal(1000m, output.Total);
        Assert.Equal(
            [
                new CategoryExpenseOutput(mercado.Id, "Mercado", "#43A047", 600m, 60m),
                new CategoryExpenseOutput(lazer.Id, "Lazer", "#FDD835", 300m, 30m),
                new CategoryExpenseOutput(transporte.Id, "Transporte", "#00ACC1", 100m, 10m)
            ],
            output.Items);
    }

    [Fact]
    public async Task Execute_SameAmount_OrdersByName()
    {
        Expenses((Category("Transporte"), 100m), (Category("Lazer"), 100m), (Category("Mercado"), 200m));

        var output = await _useCase.Execute(new GetExpensesByCategoryInput(September));

        Assert.Equal(["Mercado", "Lazer", "Transporte"], output.Items.Select(i => i.CategoryName));
    }

    [Fact]
    public async Task Execute_Percentages_AreRoundedToTwoDecimals()
    {
        Expenses((Category("Lazer"), 100m), (Category("Mercado"), 100m), (Category("Transporte"), 100m));

        var output = await _useCase.Execute(new GetExpensesByCategoryInput(September));

        Assert.All(output.Items, item => Assert.Equal(33.33m, item.Percentage));
    }

    [Fact]
    public async Task Execute_EightCategories_ListsEveryOne()
    {
        Expenses(Enumerable.Range(1, 8).Select(i => (Category($"Categoria {i}"), i * 10m)).ToArray());

        var output = await _useCase.Execute(new GetExpensesByCategoryInput(September));

        Assert.Equal(8, output.Items.Count());
        Assert.DoesNotContain(output.Items, i => i.CategoryName == "Outras");
        Assert.Equal(360m, output.Total);
    }

    [Fact]
    public async Task Execute_MoreThanEightCategories_AddsTheSmallestUpIntoOutras()
    {
        var categories = Enumerable.Range(1, 10).Select(i => (Category: Category($"Categoria {i:00}"), Expense: i * 100m)).ToArray();
        Expenses(categories);

        var output = await _useCase.Execute(new GetExpensesByCategoryInput(September));

        var items = output.Items.ToList();
        Assert.Equal(5500m, output.Total);
        Assert.Equal(8, items.Count);
        Assert.Equal(["Categoria 10", "Categoria 09", "Categoria 08", "Categoria 07", "Categoria 06", "Categoria 05", "Categoria 04"], items.Take(7).Select(i => i.CategoryName));
        Assert.Equal(new CategoryExpenseOutput(null, "Outras", null, 600m, 10.91m), items[7]);
        Assert.Equal(output.Total, items.Sum(i => i.Amount));
    }

    [Fact]
    public async Task Execute_OutrasAddingUpToMoreThanSomeCategories_StaysLast()
    {
        var shown = Enumerable.Range(1, 7).Select(i => (Category($"Categoria {i}"), 100m));
        var tail = Enumerable.Range(1, 3).Select(i => (Category($"Pequena {i}"), 90m));
        Expenses(shown.Concat(tail).ToArray());

        var output = await _useCase.Execute(new GetExpensesByCategoryInput(September));

        var outras = output.Items.Last();
        Assert.Equal("Outras", outras.CategoryName);
        Assert.Equal(270m, outras.Amount);
        Assert.Equal(27.84m, outras.Percentage);
    }

    [Fact]
    public async Task Execute_December_ReadsUpToTheFirstDayOfTheNextYear()
    {
        await _useCase.Execute(new GetExpensesByCategoryInput(new YearMonth(2026, 12)));

        await _transactionRepository.Received(1).SumExpensesByCategoryAsync(new DateOnly(2026, 12, 1), new DateOnly(2027, 1, 1));
    }

    [Fact]
    public async Task Execute_WithoutMonth_ReadsTheCurrentMonthInSaoPaulo()
    {
        _timeProvider.GetUtcNow().Returns(new DateTimeOffset(2026, 10, 1, 2, 0, 0, TimeSpan.Zero));

        await _useCase.Execute(new GetExpensesByCategoryInput());

        await _transactionRepository.Received(1).SumExpensesByCategoryAsync(new DateOnly(2026, 9, 1), new DateOnly(2026, 10, 1));
    }
}
