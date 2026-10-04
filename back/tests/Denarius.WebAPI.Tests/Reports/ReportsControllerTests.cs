using System.Net;
using System.Text.Json;
using Denarius.Application.IO.Reports;
using Denarius.Application.UseCases.Reports.GetCumulativeExpenseComparison;
using Denarius.Application.UseCases.Reports.GetExpensesByCategory;
using Denarius.Application.UseCases.Reports.GetIncomeVsExpense;
using Denarius.Application.UseCases.Reports.GetMonthlySummary;
using Denarius.Application.UseCases.Reports.ListMonthlyTransactions;
using Denarius.Domain.Entities;
using Denarius.WebAPI.Controllers;
using Denarius.WebAPI.Middleware;
using Denarius.WebAPI.Routing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Denarius.WebAPI.Tests.Reports;

public class ReportsControllerTests
{
    private static readonly MonthlySummaryOutput Summary = new(
        new YearMonth(2026, 9), 8000m, 5200m, 2800m, null, 5200m, 2800m,
        new PreviousMonthSummaryOutput(8000m, 4000m, 4000m, 0m, 30m, -30m));

    private static readonly ExpensesByCategoryOutput ExpensesByCategory = new(1000m,
    [
        new CategoryExpenseOutput(Guid.NewGuid(), "Mercado", "#43A047", 750m, 75m),
        new CategoryExpenseOutput(null, "Outras", null, 250m, 25m)
    ]);

    private static readonly IEnumerable<IncomeVsExpenseOutput> IncomeVsExpense =
    [
        new(new YearMonth(2026, 8), 8000m, 4000m, 4000m),
        new(new YearMonth(2026, 9), 0m, 0m, 0m)
    ];

    private static readonly CumulativeExpenseComparisonOutput CumulativeExpenses = new(
        [new AccumulatedExpenseOutput(1, 150m), new AccumulatedExpenseOutput(2, 210m)],
        [new AccumulatedExpenseOutput(1, 100m), new AccumulatedExpenseOutput(2, 100m), new AccumulatedExpenseOutput(3, 800m)],
        31,
        30);

    private static readonly IEnumerable<MonthlyTransactionOutput> Transactions =
    [
        new(new Transaction("Salário", new DateTime(2026, 9, 5, 0, 0, 0, DateTimeKind.Utc), 8000m, Guid.NewGuid()), "Salário"),
        new(new Transaction(null, new DateTime(2026, 9, 3, 0, 0, 0, DateTimeKind.Utc), -50m, Guid.NewGuid()), "Mercado")
    ];

    private static async Task<IHost> CreateHostAsync(
        Func<GetMonthlySummaryInput, MonthlySummaryOutput>? summary = null,
        Func<GetExpensesByCategoryInput, ExpensesByCategoryOutput>? expensesByCategory = null,
        Func<GetIncomeVsExpenseInput, IEnumerable<IncomeVsExpenseOutput>>? incomeVsExpense = null,
        Func<GetCumulativeExpenseComparisonInput, CumulativeExpenseComparisonOutput>? cumulativeExpenses = null,
        Func<ListMonthlyTransactionsInput, IEnumerable<MonthlyTransactionOutput>>? transactions = null)
    {
        var host = await new HostBuilder()
            .ConfigureWebHost(webHost =>
            {
                webHost.UseTestServer();
                webHost.ConfigureServices(services =>
                {
                    services.AddSingleton<IGetMonthlySummaryUseCase>(new FakeGetMonthlySummaryUseCase(
                        summary ?? (_ => throw new InvalidOperationException("Summary not configured for this test."))));
                    services.AddSingleton<IGetExpensesByCategoryUseCase>(new FakeGetExpensesByCategoryUseCase(
                        expensesByCategory ?? (_ => throw new InvalidOperationException("Expenses by category not configured for this test."))));
                    services.AddSingleton<IGetIncomeVsExpenseUseCase>(new FakeGetIncomeVsExpenseUseCase(
                        incomeVsExpense ?? (_ => throw new InvalidOperationException("Income vs. expense not configured for this test."))));
                    services.AddSingleton<IGetCumulativeExpenseComparisonUseCase>(new FakeGetCumulativeExpenseComparisonUseCase(
                        cumulativeExpenses ?? (_ => throw new InvalidOperationException("Cumulative expenses not configured for this test."))));
                    services.AddSingleton<IListMonthlyTransactionsUseCase>(new FakeListMonthlyTransactionsUseCase(
                        transactions ?? (_ => throw new InvalidOperationException("Transactions not configured for this test."))));
                    services.AddExceptionHandler<GlobalExceptionHandler>();
                    services.AddProblemDetails();
                    services.AddControllers(options => options.UseCamelCaseRoutes()).AddApplicationPart(typeof(ReportsController).Assembly);
                });
                webHost.Configure(app =>
                {
                    app.UseExceptionHandler();
                    app.UseRouting();
                    app.UseEndpoints(endpoints => endpoints.MapControllers());
                });
            })
            .StartAsync();

        return host;
    }

    private static async Task<JsonElement> ReadJsonAsync(HttpResponseMessage response)
    {
        var body = await response.Content.ReadAsStringAsync();
        return JsonDocument.Parse(body).RootElement;
    }

    private static async Task<ValidationProblemDetails> ReadValidationProblemAsync(HttpResponseMessage response)
    {
        var body = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<ValidationProblemDetails>(body)!;
    }

    [Fact]
    public async Task Summary_WithMonth_Returns200WithTheSummaryOfThatMonth()
    {
        GetMonthlySummaryInput? received = null;
        using var host = await CreateHostAsync(summary: input =>
        {
            received = input;
            return Summary;
        });
        var client = host.GetTestClient();

        var response = await client.GetAsync("/api/reports/summary?month=2026-09");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(new YearMonth(2026, 9), received!.Month);
        var body = await ReadJsonAsync(response);
        Assert.Equal("2026-09", body.GetProperty("month").GetString());
        Assert.Equal(8000m, body.GetProperty("totalIncome").GetDecimal());
        Assert.Equal(5200m, body.GetProperty("totalExpense").GetDecimal());
        Assert.Equal(2800m, body.GetProperty("balance").GetDecimal());
        Assert.Equal(JsonValueKind.Null, body.GetProperty("savingsRate").ValueKind);
        Assert.Equal(5200m, body.GetProperty("projectedExpense").GetDecimal());
        Assert.Equal(2800m, body.GetProperty("projectedBalance").GetDecimal());
        var previousMonth = body.GetProperty("previousMonth");
        Assert.Equal(4000m, previousMonth.GetProperty("totalExpense").GetDecimal());
        Assert.Equal(30m, previousMonth.GetProperty("totalExpenseChange").GetDecimal());
        Assert.Equal(-30m, previousMonth.GetProperty("balanceChange").GetDecimal());
    }

    [Theory]
    [InlineData("/api/reports/summary")]
    [InlineData("/api/reports/summary?month=")]
    public async Task Summary_WithoutMonth_LeavesTheMonthToTheUseCase(string url)
    {
        GetMonthlySummaryInput? received = null;
        using var host = await CreateHostAsync(summary: input =>
        {
            received = input;
            return Summary;
        });
        var client = host.GetTestClient();

        var response = await client.GetAsync(url);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(received);
        Assert.Null(received!.Month);
    }

    [Theory]
    [InlineData("2026-13")]
    [InlineData("2026-9")]
    [InlineData("09-2026")]
    [InlineData("2026-09-01")]
    [InlineData("abc")]
    public async Task Summary_InvalidMonth_Returns400WithoutCallingUseCase(string month)
    {
        var called = false;
        using var host = await CreateHostAsync(summary: _ =>
        {
            called = true;
            return Summary;
        });
        var client = host.GetTestClient();

        var response = await client.GetAsync($"/api/reports/summary?month={month}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("month", (await ReadValidationProblemAsync(response)).Errors.Keys);
        Assert.False(called);
    }

    [Fact]
    public async Task ExpensesByCategory_WithMonth_Returns200WithTheTotalAndTheItems()
    {
        GetExpensesByCategoryInput? received = null;
        using var host = await CreateHostAsync(expensesByCategory: input =>
        {
            received = input;
            return ExpensesByCategory;
        });
        var client = host.GetTestClient();

        var response = await client.GetAsync("/api/reports/expensesByCategory?month=2026-09");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(new YearMonth(2026, 9), received!.Month);
        var body = await ReadJsonAsync(response);
        Assert.Equal(1000m, body.GetProperty("total").GetDecimal());
        var items = body.GetProperty("items").EnumerateArray().ToList();
        Assert.Equal(2, items.Count);
        Assert.Equal("Mercado", items[0].GetProperty("categoryName").GetString());
        Assert.Equal("#43A047", items[0].GetProperty("color").GetString());
        Assert.Equal(75m, items[0].GetProperty("percentage").GetDecimal());
        Assert.Equal("Outras", items[1].GetProperty("categoryName").GetString());
        Assert.Equal(JsonValueKind.Null, items[1].GetProperty("categoryId").ValueKind);
        Assert.Equal(JsonValueKind.Null, items[1].GetProperty("color").ValueKind);
        Assert.Equal(250m, items[1].GetProperty("amount").GetDecimal());
    }

    [Fact]
    public async Task ExpensesByCategory_WithoutMonth_LeavesTheMonthToTheUseCase()
    {
        GetExpensesByCategoryInput? received = null;
        using var host = await CreateHostAsync(expensesByCategory: input =>
        {
            received = input;
            return ExpensesByCategory;
        });
        var client = host.GetTestClient();

        var response = await client.GetAsync("/api/reports/expensesByCategory");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Null(received!.Month);
    }

    [Fact]
    public async Task ExpensesByCategory_InvalidMonth_Returns400WithoutCallingUseCase()
    {
        var called = false;
        using var host = await CreateHostAsync(expensesByCategory: _ =>
        {
            called = true;
            return ExpensesByCategory;
        });
        var client = host.GetTestClient();

        var response = await client.GetAsync("/api/reports/expensesByCategory?month=2026-13");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.False(called);
    }

    [Fact]
    public async Task IncomeVsExpense_WithMonthAndMonths_Returns200WithTheSeries()
    {
        GetIncomeVsExpenseInput? received = null;
        using var host = await CreateHostAsync(incomeVsExpense: input =>
        {
            received = input;
            return IncomeVsExpense;
        });
        var client = host.GetTestClient();

        var response = await client.GetAsync("/api/reports/incomeVsExpense?month=2026-09&months=2");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(new YearMonth(2026, 9), received!.Month);
        Assert.Equal(2, received.Months);
        var body = (await ReadJsonAsync(response)).EnumerateArray().ToList();
        Assert.Equal(["2026-08", "2026-09"], body.Select(o => o.GetProperty("month").GetString()));
        Assert.Equal(8000m, body[0].GetProperty("income").GetDecimal());
        Assert.Equal(4000m, body[0].GetProperty("expense").GetDecimal());
        Assert.Equal(4000m, body[0].GetProperty("balance").GetDecimal());
    }

    [Fact]
    public async Task IncomeVsExpense_WithoutParameters_AsksForTwelveMonthsOfTheCurrentMonth()
    {
        GetIncomeVsExpenseInput? received = null;
        using var host = await CreateHostAsync(incomeVsExpense: input =>
        {
            received = input;
            return IncomeVsExpense;
        });
        var client = host.GetTestClient();

        await client.GetAsync("/api/reports/incomeVsExpense");

        Assert.Null(received!.Month);
        Assert.Equal(12, received.Months);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(24)]
    public async Task IncomeVsExpense_MonthsWithinRange_IsPassedToTheUseCase(int months)
    {
        GetIncomeVsExpenseInput? received = null;
        using var host = await CreateHostAsync(incomeVsExpense: input =>
        {
            received = input;
            return IncomeVsExpense;
        });
        var client = host.GetTestClient();

        var response = await client.GetAsync($"/api/reports/incomeVsExpense?months={months}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(months, received!.Months);
    }

    [Theory]
    [InlineData("months=0", "months")]
    [InlineData("months=25", "months")]
    [InlineData("months=abc", "months")]
    [InlineData("month=2026-13", "month")]
    public async Task IncomeVsExpense_InvalidParameter_Returns400WithoutCallingUseCase(string query, string parameter)
    {
        var called = false;
        using var host = await CreateHostAsync(incomeVsExpense: _ =>
        {
            called = true;
            return IncomeVsExpense;
        });
        var client = host.GetTestClient();

        var response = await client.GetAsync($"/api/reports/incomeVsExpense?{query}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains(parameter, (await ReadValidationProblemAsync(response)).Errors.Keys);
        Assert.False(called);
    }

    [Fact]
    public async Task CumulativeExpenses_WithMonth_Returns200WithBothSeriesAndTheirDays()
    {
        GetCumulativeExpenseComparisonInput? received = null;
        using var host = await CreateHostAsync(cumulativeExpenses: input =>
        {
            received = input;
            return CumulativeExpenses;
        });
        var client = host.GetTestClient();

        var response = await client.GetAsync("/api/reports/cumulativeExpenses?month=2026-10");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(new YearMonth(2026, 10), received!.Month);
        var body = await ReadJsonAsync(response);
        var currentMonth = body.GetProperty("currentMonth").EnumerateArray().ToList();
        Assert.Equal([1, 2], currentMonth.Select(d => d.GetProperty("day").GetInt32()));
        Assert.Equal([150m, 210m], currentMonth.Select(d => d.GetProperty("accumulated").GetDecimal()));
        Assert.Equal(3, body.GetProperty("previousMonth").GetArrayLength());
        Assert.Equal(31, body.GetProperty("daysInCurrentMonth").GetInt32());
        Assert.Equal(30, body.GetProperty("daysInPreviousMonth").GetInt32());
    }

    [Fact]
    public async Task CumulativeExpenses_WithoutMonth_LeavesTheMonthToTheUseCase()
    {
        GetCumulativeExpenseComparisonInput? received = null;
        using var host = await CreateHostAsync(cumulativeExpenses: input =>
        {
            received = input;
            return CumulativeExpenses;
        });
        var client = host.GetTestClient();

        var response = await client.GetAsync("/api/reports/cumulativeExpenses");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Null(received!.Month);
    }

    [Fact]
    public async Task CumulativeExpenses_InvalidMonth_Returns400WithoutCallingUseCase()
    {
        var called = false;
        using var host = await CreateHostAsync(cumulativeExpenses: _ =>
        {
            called = true;
            return CumulativeExpenses;
        });
        var client = host.GetTestClient();

        var response = await client.GetAsync("/api/reports/cumulativeExpenses?month=10-2026");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.False(called);
    }

    [Fact]
    public async Task Transactions_WithMonth_Returns200WithEveryTransactionAndItsTypeAsText()
    {
        ListMonthlyTransactionsInput? received = null;
        using var host = await CreateHostAsync(transactions: input =>
        {
            received = input;
            return Transactions;
        });
        var client = host.GetTestClient();

        var response = await client.GetAsync("/api/reports/transactions?month=2026-09");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(new YearMonth(2026, 9), received!.Month);
        var body = (await ReadJsonAsync(response)).EnumerateArray().ToList();
        Assert.Equal(2, body.Count);
        Assert.Equal("Salário", body[0].GetProperty("description").GetString());
        Assert.Equal("Salário", body[0].GetProperty("categoryName").GetString());
        Assert.Equal("in", body[0].GetProperty("type").GetString());
        Assert.Equal(8000m, body[0].GetProperty("amount").GetDecimal());
        Assert.Equal(new DateTime(2026, 9, 5, 0, 0, 0, DateTimeKind.Utc), body[0].GetProperty("date").GetDateTime());
        Assert.Equal(JsonValueKind.Null, body[1].GetProperty("description").ValueKind);
        Assert.Equal("out", body[1].GetProperty("type").GetString());
        Assert.Equal(50m, body[1].GetProperty("amount").GetDecimal());
    }

    [Fact]
    public async Task Transactions_WithoutMonth_LeavesTheMonthToTheUseCase()
    {
        ListMonthlyTransactionsInput? received = null;
        using var host = await CreateHostAsync(transactions: input =>
        {
            received = input;
            return Transactions;
        });
        var client = host.GetTestClient();

        var response = await client.GetAsync("/api/reports/transactions");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Null(received!.Month);
    }

    [Fact]
    public async Task Transactions_InvalidMonth_Returns400WithoutCallingUseCase()
    {
        var called = false;
        using var host = await CreateHostAsync(transactions: _ =>
        {
            called = true;
            return Transactions;
        });
        var client = host.GetTestClient();

        var response = await client.GetAsync("/api/reports/transactions?month=2026-00");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.False(called);
    }

    private sealed class FakeGetMonthlySummaryUseCase(Func<GetMonthlySummaryInput, MonthlySummaryOutput> handler) : IGetMonthlySummaryUseCase
    {
        public Task<MonthlySummaryOutput> Execute(GetMonthlySummaryInput input) => Task.FromResult(handler(input));
    }

    private sealed class FakeGetExpensesByCategoryUseCase(Func<GetExpensesByCategoryInput, ExpensesByCategoryOutput> handler) : IGetExpensesByCategoryUseCase
    {
        public Task<ExpensesByCategoryOutput> Execute(GetExpensesByCategoryInput input) => Task.FromResult(handler(input));
    }

    private sealed class FakeGetIncomeVsExpenseUseCase(Func<GetIncomeVsExpenseInput, IEnumerable<IncomeVsExpenseOutput>> handler) : IGetIncomeVsExpenseUseCase
    {
        public Task<IEnumerable<IncomeVsExpenseOutput>> Execute(GetIncomeVsExpenseInput input) => Task.FromResult(handler(input));
    }

    private sealed class FakeGetCumulativeExpenseComparisonUseCase(Func<GetCumulativeExpenseComparisonInput, CumulativeExpenseComparisonOutput> handler) : IGetCumulativeExpenseComparisonUseCase
    {
        public Task<CumulativeExpenseComparisonOutput> Execute(GetCumulativeExpenseComparisonInput input) => Task.FromResult(handler(input));
    }

    private sealed class FakeListMonthlyTransactionsUseCase(Func<ListMonthlyTransactionsInput, IEnumerable<MonthlyTransactionOutput>> handler) : IListMonthlyTransactionsUseCase
    {
        public Task<IEnumerable<MonthlyTransactionOutput>> Execute(ListMonthlyTransactionsInput input) => Task.FromResult(handler(input));
    }
}
