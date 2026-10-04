using Denarius.Application.IO.Reports;
using Denarius.Application.UseCases.Reports.GetMonthlySummary;
using Denarius.Domain.Repositories;
using NSubstitute;

namespace Denarius.Application.Tests.UseCases.Reports.GetMonthlySummary;

public class GetMonthlySummaryUseCaseTests
{
    private readonly ITransactionRepository _transactionRepository = Substitute.For<ITransactionRepository>();
    private readonly TimeProvider _timeProvider = Substitute.For<TimeProvider>();
    private readonly IGetMonthlySummaryUseCase _useCase;

    public GetMonthlySummaryUseCaseTests()
    {
        _useCase = new GetMonthlySummaryUseCase(_transactionRepository, _timeProvider);
        Today(2026, 10, 3);
        _transactionRepository.SumByMonthAsync(Arg.Any<DateOnly>(), Arg.Any<DateOnly>()).Returns([]);
    }

    /// <summary>Noon in São Paulo on that day.</summary>
    private void Today(int year, int month, int day) =>
        _timeProvider.GetUtcNow().Returns(new DateTimeOffset(year, month, day, 15, 0, 0, TimeSpan.Zero));

    /// <summary>The sums of the report month and the month before, read in a single range.</summary>
    private void Totals(YearMonth month, params (int Year, int Month, decimal Income, decimal Expense)[] totals) =>
        _transactionRepository.SumByMonthAsync(month.AddMonths(-1).FirstDay, month.AddMonths(1).FirstDay).Returns(totals);

    [Fact]
    public async Task Execute_MonthWithoutTransactions_ReturnsZerosAndNoRates()
    {
        var output = await _useCase.Execute(new GetMonthlySummaryInput(new YearMonth(2026, 3)));

        Assert.Equal("2026-03", output.Month);
        Assert.Equal(0m, output.TotalIncome);
        Assert.Equal(0m, output.TotalExpense);
        Assert.Equal(0m, output.Balance);
        Assert.Null(output.SavingsRate);
        Assert.Equal(0m, output.ProjectedExpense);
        Assert.Equal(0m, output.ProjectedBalance);
        Assert.Equal(new PreviousMonthSummaryOutput(0m, 0m, 0m, null, null, null), output.PreviousMonth);
    }

    [Fact]
    public async Task Execute_IncomeAndExpense_ReturnsTotalsBalanceAndSavingsRate()
    {
        var september = new YearMonth(2026, 9);
        Totals(september, (2026, 9, 5000m, 3000m));

        var output = await _useCase.Execute(new GetMonthlySummaryInput(september));

        Assert.Equal(5000m, output.TotalIncome);
        Assert.Equal(3000m, output.TotalExpense);
        Assert.Equal(2000m, output.Balance);
        Assert.Equal(40m, output.SavingsRate);
    }

    [Fact]
    public async Task Execute_OnlyIncome_SavesEverything()
    {
        var september = new YearMonth(2026, 9);
        Totals(september, (2026, 9, 8000m, 0m));

        var output = await _useCase.Execute(new GetMonthlySummaryInput(september));

        Assert.Equal(0m, output.TotalExpense);
        Assert.Equal(8000m, output.Balance);
        Assert.Equal(100m, output.SavingsRate);
    }

    [Fact]
    public async Task Execute_OnlyExpenses_HasNoSavingsRateAndANegativeBalance()
    {
        var september = new YearMonth(2026, 9);
        Totals(september, (2026, 9, 0m, 1200m));

        var output = await _useCase.Execute(new GetMonthlySummaryInput(september));

        Assert.Equal(0m, output.TotalIncome);
        Assert.Equal(-1200m, output.Balance);
        Assert.Null(output.SavingsRate);
    }

    [Theory]
    [InlineData(3000, 1000, 66.67)]
    [InlineData(1000, 1500, -50)]
    [InlineData(8000, 2210, 72.38)]
    public async Task Execute_SavingsRate_IsAPercentageRoundedToTwoDecimals(decimal income, decimal expense, decimal expected)
    {
        var september = new YearMonth(2026, 9);
        Totals(september, (2026, 9, income, expense));

        var output = await _useCase.Execute(new GetMonthlySummaryInput(september));

        Assert.Equal(expected, output.SavingsRate);
    }

    [Fact]
    public async Task Execute_PastMonth_ProjectsTheActualValues()
    {
        var september = new YearMonth(2026, 9);
        Totals(september, (2026, 9, 8000m, 5200m));

        var output = await _useCase.Execute(new GetMonthlySummaryInput(september));

        Assert.Equal(5200m, output.ProjectedExpense);
        Assert.Equal(2800m, output.ProjectedBalance);
        await _transactionRepository.Received(1).SumByMonthAsync(Arg.Any<DateOnly>(), Arg.Any<DateOnly>());
    }

    [Fact]
    public async Task Execute_CurrentMonth_ProjectsTheExpenseToDateOverTheDaysElapsed()
    {
        Today(2026, 9, 10);
        var september = new YearMonth(2026, 9);
        Totals(september, (2026, 9, 5000m, 1500m));
        _transactionRepository.SumByMonthAsync(new DateOnly(2026, 9, 1), new DateOnly(2026, 9, 11)).Returns([(2026, 9, 5000m, 900m)]);

        var output = await _useCase.Execute(new GetMonthlySummaryInput(september));

        Assert.Equal(1500m, output.TotalExpense);
        Assert.Equal(2700m, output.ProjectedExpense);
        Assert.Equal(2300m, output.ProjectedBalance);
    }

    [Fact]
    public async Task Execute_CurrentMonth_RoundsTheProjectionToCents()
    {
        var october = new YearMonth(2026, 10);
        Totals(october, (2026, 10, 8000m, 100m));
        _transactionRepository.SumByMonthAsync(new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 4)).Returns([(2026, 10, 0m, 100m)]);

        var output = await _useCase.Execute(new GetMonthlySummaryInput(october));

        Assert.Equal(1033.33m, output.ProjectedExpense);
        Assert.Equal(6966.67m, output.ProjectedBalance);
    }

    [Fact]
    public async Task Execute_FirstDayOfTheCurrentMonth_ProjectsTheDayExpenseOverTheWholeMonth()
    {
        Today(2026, 10, 1);
        var october = new YearMonth(2026, 10);
        Totals(october, (2026, 10, 0m, 50m));
        _transactionRepository.SumByMonthAsync(new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 2)).Returns([(2026, 10, 0m, 50m)]);

        var output = await _useCase.Execute(new GetMonthlySummaryInput(october));

        Assert.Equal(1550m, output.ProjectedExpense);
        Assert.Equal(-1550m, output.ProjectedBalance);
    }

    [Fact]
    public async Task Execute_FutureMonth_ProjectsZero()
    {
        var november = new YearMonth(2026, 11);
        Totals(november, (2026, 11, 0m, 500m));

        var output = await _useCase.Execute(new GetMonthlySummaryInput(november));

        Assert.Equal(500m, output.TotalExpense);
        Assert.Equal(0m, output.ProjectedExpense);
        Assert.Equal(0m, output.ProjectedBalance);
        await _transactionRepository.Received(1).SumByMonthAsync(Arg.Any<DateOnly>(), Arg.Any<DateOnly>());
    }

    [Fact]
    public async Task Execute_ComparesWithThePreviousMonth()
    {
        var september = new YearMonth(2026, 9);
        Totals(september, (2026, 8, 8000m, 4000m), (2026, 9, 8000m, 5200m));

        var output = await _useCase.Execute(new GetMonthlySummaryInput(september));

        Assert.Equal(new PreviousMonthSummaryOutput(8000m, 4000m, 4000m, 0m, 30m, -30m), output.PreviousMonth);
    }

    [Fact]
    public async Task Execute_PreviousValueOfZero_HasNoChange()
    {
        var september = new YearMonth(2026, 9);
        Totals(september, (2026, 8, 0m, 400m), (2026, 9, 1000m, 600m));

        var output = await _useCase.Execute(new GetMonthlySummaryInput(september));

        Assert.Null(output.PreviousMonth.TotalIncomeChange);
        Assert.Equal(50m, output.PreviousMonth.TotalExpenseChange);
        Assert.Equal(200m, output.PreviousMonth.BalanceChange);
    }

    [Theory]
    [InlineData(600, 550, 150)]
    [InlineData(100, 400, -200)]
    public async Task Execute_NegativePreviousBalance_MeasuresTheChangeAgainstItsSize(decimal income, decimal expense, decimal expected)
    {
        var september = new YearMonth(2026, 9);
        Totals(september, (2026, 8, 400m, 500m), (2026, 9, income, expense));

        var output = await _useCase.Execute(new GetMonthlySummaryInput(september));

        Assert.Equal(-100m, output.PreviousMonth.Balance);
        Assert.Equal(expected, output.PreviousMonth.BalanceChange);
    }

    [Fact]
    public async Task Execute_January_ComparesWithDecemberOfThePreviousYear()
    {
        _transactionRepository.SumByMonthAsync(new DateOnly(2025, 12, 1), new DateOnly(2026, 2, 1))
            .Returns([(2025, 12, 7500m, 2700m), (2026, 1, 7500m, 2800m)]);

        var output = await _useCase.Execute(new GetMonthlySummaryInput(new YearMonth(2026, 1)));

        Assert.Equal(2800m, output.TotalExpense);
        Assert.Equal(new PreviousMonthSummaryOutput(7500m, 2700m, 4800m, 0m, 3.7m, -2.08m), output.PreviousMonth);
    }

    [Fact]
    public async Task Execute_WithoutMonth_SummarizesTheCurrentMonthInSaoPaulo()
    {
        _timeProvider.GetUtcNow().Returns(new DateTimeOffset(2026, 10, 1, 2, 0, 0, TimeSpan.Zero));
        var september = new YearMonth(2026, 9);
        Totals(september, (2026, 9, 8000m, 5200m));
        _transactionRepository.SumByMonthAsync(new DateOnly(2026, 9, 1), new DateOnly(2026, 10, 1)).Returns([(2026, 9, 8000m, 5200m)]);

        var output = await _useCase.Execute(new GetMonthlySummaryInput());

        Assert.Equal("2026-09", output.Month);
        Assert.Equal(5200m, output.ProjectedExpense);
        await _transactionRepository.Received(1).SumByMonthAsync(new DateOnly(2026, 8, 1), new DateOnly(2026, 10, 1));
        await _transactionRepository.Received(1).SumByMonthAsync(new DateOnly(2026, 9, 1), new DateOnly(2026, 10, 1));
    }
}
