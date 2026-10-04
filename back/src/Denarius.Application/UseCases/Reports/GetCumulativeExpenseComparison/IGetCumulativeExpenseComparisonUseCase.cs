using Denarius.Application.IO.Reports;

namespace Denarius.Application.UseCases.Reports.GetCumulativeExpenseComparison;

public interface IGetCumulativeExpenseComparisonUseCase : IUseCase<GetCumulativeExpenseComparisonInput, Task<CumulativeExpenseComparisonOutput>>
{
}
