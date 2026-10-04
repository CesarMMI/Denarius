using Denarius.Application.IO.Reports;

namespace Denarius.Application.UseCases.Reports.GetExpensesByCategory;

public interface IGetExpensesByCategoryUseCase : IUseCase<GetExpensesByCategoryInput, Task<ExpensesByCategoryOutput>>
{
}
