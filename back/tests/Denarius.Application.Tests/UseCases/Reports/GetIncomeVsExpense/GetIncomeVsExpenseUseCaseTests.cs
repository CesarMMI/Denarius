using Denarius.Application.IO.Reports;
using Denarius.Application.UseCases.Reports.GetIncomeVsExpense;
using Denarius.Domain.Repositories;
using NSubstitute;

namespace Denarius.Application.Tests.UseCases.Reports.GetIncomeVsExpense;

public class GetIncomeVsExpenseUseCaseTests
{
    private readonly ITransactionRepository _transactionRepository = Substitute.For<ITransactionRepository>();
    private readonly TimeProvider _timeProvider = Substitute.For<TimeProvider>();
    private readonly IGetIncomeVsExpenseUseCase _useCase;

    public GetIncomeVsExpenseUseCaseTests()
    {
        _useCase = new GetIncomeVsExpenseUseCase(_transactionRepository, _timeProvider);
        _timeProvider.GetUtcNow().Returns(new DateTimeOffset(2026, 10, 3, 15, 0, 0, TimeSpan.Zero));
        _transactionRepository.SumByMonthAsync(Arg.Any<DateOnly>(), Arg.Any<DateOnly>()).Returns([]);
    }

    [Fact]
    public async Task Execute_ByDefault_ReturnsTheTwelveMonthsEndingInTheMonthOldestFirst()
    {
        var output = (await _useCase.Execute(new GetIncomeVsExpenseInput(new YearMonth(2026, 10)))).ToList();

        Assert.Equal(12, output.Count);
        Assert.Equal("2025-11", output[0].Month);
        Assert.Equal("2026-10", output[11].Month);
        Assert.Equal(output.Select(o => o.Month).Order(), output.Select(o => o.Month));
        await _transactionRepository.Received(1).SumByMonthAsync(new DateOnly(2025, 11, 1), new DateOnly(2026, 11, 1));
    }

    [Fact]
    public async Task Execute_MonthsWithoutTransactions_AreFilledWithZeros()
    {
        _transactionRepository.SumByMonthAsync(new DateOnly(2026, 8, 1), new DateOnly(2026, 11, 1))
            .Returns([(2026, 10, 8000m, 2210m), (2026, 8, 8000m, 4000m)]);

        var output = await _useCase.Execute(new GetIncomeVsExpenseInput(new YearMonth(2026, 10), 3));

        Assert.Equal(
            [
                new IncomeVsExpenseOutput(new YearMonth(2026, 8), 8000m, 4000m, 4000m),
                new IncomeVsExpenseOutput(new YearMonth(2026, 9), 0m, 0m, 0m),
                new IncomeVsExpenseOutput(new YearMonth(2026, 10), 8000m, 2210m, 5790m)
            ],
            output);
    }

    [Fact]
    public async Task Execute_HistoryWithoutTransactions_ReturnsZerosForEveryMonth()
    {
        var output = await _useCase.Execute(new GetIncomeVsExpenseInput(new YearMonth(2026, 10)));

        Assert.Equal(12, output.Count());
        Assert.All(output, o => Assert.Equal((0m, 0m, 0m), (o.Income, o.Expense, o.Balance)));
    }

    [Fact]
    public async Task Execute_SeriesCrossingTheYear_StartsInThePreviousYear()
    {
        _transactionRepository.SumByMonthAsync(new DateOnly(2025, 11, 1), new DateOnly(2026, 3, 1))
            .Returns([(2025, 12, 7500m, 2700m), (2026, 1, 7500m, 2800m)]);

        var output = (await _useCase.Execute(new GetIncomeVsExpenseInput(new YearMonth(2026, 2), 4))).ToList();

        Assert.Equal(["2025-11", "2025-12", "2026-01", "2026-02"], output.Select(o => o.Month));
        Assert.Equal(4800m, output[1].Balance);
        Assert.Equal(4700m, output[2].Balance);
    }

    [Fact]
    public async Task Execute_OneMonth_ReturnsOnlyThatMonth()
    {
        var output = await _useCase.Execute(new GetIncomeVsExpenseInput(new YearMonth(2026, 10), 1));

        Assert.Equal("2026-10", Assert.Single(output).Month);
        await _transactionRepository.Received(1).SumByMonthAsync(new DateOnly(2026, 10, 1), new DateOnly(2026, 11, 1));
    }

    [Fact]
    public async Task Execute_OnlyIncomeOrOnlyExpense_BalancesIncomeMinusExpense()
    {
        _transactionRepository.SumByMonthAsync(new DateOnly(2026, 9, 1), new DateOnly(2026, 11, 1))
            .Returns([(2026, 9, 8000m, 0m), (2026, 10, 0m, 1200m)]);

        var output = (await _useCase.Execute(new GetIncomeVsExpenseInput(new YearMonth(2026, 10), 2))).ToList();

        Assert.Equal(8000m, output[0].Balance);
        Assert.Equal(-1200m, output[1].Balance);
    }

    [Fact]
    public async Task Execute_WithoutMonth_EndsInTheCurrentMonthInSaoPaulo()
    {
        _timeProvider.GetUtcNow().Returns(new DateTimeOffset(2026, 10, 1, 2, 0, 0, TimeSpan.Zero));

        var output = await _useCase.Execute(new GetIncomeVsExpenseInput());

        Assert.Equal("2026-09", output.Last().Month);
        await _transactionRepository.Received(1).SumByMonthAsync(new DateOnly(2025, 10, 1), new DateOnly(2026, 10, 1));
    }
}
