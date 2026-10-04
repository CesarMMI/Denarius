using Denarius.Application.IO.Reports;
using Denarius.Application.UseCases.Reports.GetCumulativeExpenseComparison;
using Denarius.Domain.Repositories;
using NSubstitute;

namespace Denarius.Application.Tests.UseCases.Reports.GetCumulativeExpenseComparison;

public class GetCumulativeExpenseComparisonUseCaseTests
{
    private readonly ITransactionRepository _transactionRepository = Substitute.For<ITransactionRepository>();
    private readonly TimeProvider _timeProvider = Substitute.For<TimeProvider>();
    private readonly IGetCumulativeExpenseComparisonUseCase _useCase;

    public GetCumulativeExpenseComparisonUseCaseTests()
    {
        _useCase = new GetCumulativeExpenseComparisonUseCase(_transactionRepository, _timeProvider);
        _timeProvider.GetUtcNow().Returns(new DateTimeOffset(2026, 10, 3, 15, 0, 0, TimeSpan.Zero));
        _transactionRepository.SumExpensesByDayAsync(Arg.Any<DateOnly>(), Arg.Any<DateOnly>()).Returns([]);
    }

    /// <summary>The daily expenses of the report month and the month before, read in a single range.</summary>
    private void Expenses(YearMonth month, params (DateOnly Date, decimal Expense)[] expenses) =>
        _transactionRepository.SumExpensesByDayAsync(month.AddMonths(-1).FirstDay, month.AddMonths(1).FirstDay).Returns(expenses);

    [Fact]
    public async Task Execute_PastMonth_AccumulatesEveryDayOfTheMonth()
    {
        var september = new YearMonth(2026, 9);
        Expenses(september, (new DateOnly(2026, 9, 3), 100m), (new DateOnly(2026, 9, 10), 250m));

        var output = await _useCase.Execute(new GetCumulativeExpenseComparisonInput(september));

        var series = output.CurrentMonth.ToList();
        Assert.Equal(Enumerable.Range(1, 30), series.Select(d => d.Day));
        Assert.Equal(new AccumulatedExpenseOutput(2, 0m), series[1]);
        Assert.Equal(new AccumulatedExpenseOutput(3, 100m), series[2]);
        Assert.Equal(new AccumulatedExpenseOutput(9, 100m), series[8]);
        Assert.Equal(new AccumulatedExpenseOutput(10, 350m), series[9]);
        Assert.Equal(new AccumulatedExpenseOutput(30, 350m), series[29]);
        Assert.Equal(30, output.DaysInCurrentMonth);
        Assert.Equal(31, output.DaysInPreviousMonth);
    }

    [Fact]
    public async Task Execute_MonthWithoutExpenses_ReturnsZeroForEveryDay()
    {
        var output = await _useCase.Execute(new GetCumulativeExpenseComparisonInput(new YearMonth(2026, 6)));

        Assert.Equal(30, output.CurrentMonth.Count());
        Assert.Equal(31, output.PreviousMonth.Count());
        Assert.All(output.CurrentMonth.Concat(output.PreviousMonth), d => Assert.Equal(0m, d.Accumulated));
    }

    [Fact]
    public async Task Execute_CurrentMonth_StopsAtTodayAndKeepsThePreviousMonthWhole()
    {
        var october = new YearMonth(2026, 10);
        Expenses(october,
            (new DateOnly(2026, 9, 1), 100m),
            (new DateOnly(2026, 9, 28), 70m),
            (new DateOnly(2026, 10, 1), 150m),
            (new DateOnly(2026, 10, 2), 60m),
            (new DateOnly(2026, 10, 10), 2000m));

        var output = await _useCase.Execute(new GetCumulativeExpenseComparisonInput(october));

        Assert.Equal(
            [new AccumulatedExpenseOutput(1, 150m), new AccumulatedExpenseOutput(2, 210m), new AccumulatedExpenseOutput(3, 210m)],
            output.CurrentMonth);
        Assert.Equal(30, output.PreviousMonth.Count());
        Assert.Equal(new AccumulatedExpenseOutput(30, 170m), output.PreviousMonth.Last());
        Assert.Equal(31, output.DaysInCurrentMonth);
        Assert.Equal(30, output.DaysInPreviousMonth);
    }

    [Fact]
    public async Task Execute_MonthAfterTheCurrentOne_HasNoDaysAndComparesWithTheCurrentMonthUpToToday()
    {
        var output = await _useCase.Execute(new GetCumulativeExpenseComparisonInput(new YearMonth(2026, 11)));

        Assert.Empty(output.CurrentMonth);
        Assert.Equal([1, 2, 3], output.PreviousMonth.Select(d => d.Day));
        Assert.Equal(30, output.DaysInCurrentMonth);
        Assert.Equal(31, output.DaysInPreviousMonth);
    }

    [Fact]
    public async Task Execute_FutureMonths_HaveNoDays()
    {
        var output = await _useCase.Execute(new GetCumulativeExpenseComparisonInput(new YearMonth(2028, 2)));

        Assert.Empty(output.CurrentMonth);
        Assert.Empty(output.PreviousMonth);
        Assert.Equal(29, output.DaysInCurrentMonth);
        Assert.Equal(31, output.DaysInPreviousMonth);
    }

    [Fact]
    public async Task Execute_January_ComparesWithDecemberOfThePreviousYear()
    {
        var january = new YearMonth(2026, 1);
        Expenses(january, (new DateOnly(2025, 12, 20), 800m), (new DateOnly(2026, 1, 15), 900m));

        var output = await _useCase.Execute(new GetCumulativeExpenseComparisonInput(january));

        Assert.Equal(31, output.CurrentMonth.Count());
        Assert.Equal(31, output.PreviousMonth.Count());
        Assert.Equal(900m, output.CurrentMonth.Last().Accumulated);
        Assert.Equal(800m, output.PreviousMonth.Last().Accumulated);
        Assert.Equal(0m, output.PreviousMonth.Single(d => d.Day == 19).Accumulated);
        await _transactionRepository.Received(1).SumExpensesByDayAsync(new DateOnly(2025, 12, 1), new DateOnly(2026, 2, 1));
    }

    [Fact]
    public async Task Execute_February_HasItsOwnNumberOfDays()
    {
        var output = await _useCase.Execute(new GetCumulativeExpenseComparisonInput(new YearMonth(2026, 2)));

        Assert.Equal(28, output.CurrentMonth.Count());
        Assert.Equal(28, output.DaysInCurrentMonth);
        Assert.Equal(31, output.DaysInPreviousMonth);
    }

    [Fact]
    public async Task Execute_WithoutMonth_ComparesTheCurrentMonthInSaoPaulo()
    {
        _timeProvider.GetUtcNow().Returns(new DateTimeOffset(2026, 10, 1, 2, 0, 0, TimeSpan.Zero));

        var output = await _useCase.Execute(new GetCumulativeExpenseComparisonInput());

        Assert.Equal(30, output.CurrentMonth.Count());
        Assert.Equal(30, output.DaysInCurrentMonth);
        Assert.Equal(31, output.DaysInPreviousMonth);
        await _transactionRepository.Received(1).SumExpensesByDayAsync(new DateOnly(2026, 8, 1), new DateOnly(2026, 10, 1));
    }
}
