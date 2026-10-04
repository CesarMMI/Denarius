using Denarius.Application.IO.Reports;

namespace Denarius.Application.UseCases.Reports.GetIncomeVsExpense;

public interface IGetIncomeVsExpenseUseCase : IUseCase<GetIncomeVsExpenseInput, Task<IEnumerable<IncomeVsExpenseOutput>>>
{
}
