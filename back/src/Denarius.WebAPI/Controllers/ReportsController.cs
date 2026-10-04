using System.ComponentModel.DataAnnotations;
using Denarius.Application.IO.Reports;
using Denarius.Application.UseCases.Reports.GetCumulativeExpenseComparison;
using Denarius.Application.UseCases.Reports.GetExpensesByCategory;
using Denarius.Application.UseCases.Reports.GetIncomeVsExpense;
using Denarius.Application.UseCases.Reports.GetMonthlySummary;
using Denarius.Application.UseCases.Reports.ListMonthlyTransactions;
using Microsoft.AspNetCore.Mvc;

namespace Denarius.WebAPI.Controllers;

/// <summary>Each report covers a <c>yyyy-MM</c> month, the current one in São Paulo when omitted; an invalid month is a 400.</summary>
[ApiController]
[Route("api/[controller]")]
public class ReportsController(
    IGetMonthlySummaryUseCase getMonthlySummaryUseCase,
    IGetExpensesByCategoryUseCase getExpensesByCategoryUseCase,
    IGetIncomeVsExpenseUseCase getIncomeVsExpenseUseCase,
    IGetCumulativeExpenseComparisonUseCase getCumulativeExpenseComparisonUseCase,
    IListMonthlyTransactionsUseCase listMonthlyTransactionsUseCase) : ControllerBase
{
    [HttpGet("summary")]
    public async Task<ActionResult<MonthlySummaryOutput>> Summary([FromQuery] YearMonth? month)
    {
        var output = await getMonthlySummaryUseCase.Execute(new GetMonthlySummaryInput(month));
        return Ok(output);
    }

    [HttpGet("expensesByCategory")]
    public async Task<ActionResult<ExpensesByCategoryOutput>> ExpensesByCategory([FromQuery] YearMonth? month)
    {
        var output = await getExpensesByCategoryUseCase.Execute(new GetExpensesByCategoryInput(month));
        return Ok(output);
    }

    [HttpGet("incomeVsExpense")]
    public async Task<ActionResult<IEnumerable<IncomeVsExpenseOutput>>> IncomeVsExpense(
        [FromQuery] YearMonth? month,
        [FromQuery, Range(1, 24)] int months = 12)
    {
        var output = await getIncomeVsExpenseUseCase.Execute(new GetIncomeVsExpenseInput(month, months));
        return Ok(output);
    }

    [HttpGet("cumulativeExpenses")]
    public async Task<ActionResult<CumulativeExpenseComparisonOutput>> CumulativeExpenses([FromQuery] YearMonth? month)
    {
        var output = await getCumulativeExpenseComparisonUseCase.Execute(new GetCumulativeExpenseComparisonInput(month));
        return Ok(output);
    }

    [HttpGet("transactions")]
    public async Task<ActionResult<IEnumerable<MonthlyTransactionOutput>>> Transactions([FromQuery] YearMonth? month)
    {
        var output = await listMonthlyTransactionsUseCase.Execute(new ListMonthlyTransactionsInput(month));
        return Ok(output);
    }
}
