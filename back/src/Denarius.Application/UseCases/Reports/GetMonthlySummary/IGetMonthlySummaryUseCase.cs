using Denarius.Application.IO.Reports;

namespace Denarius.Application.UseCases.Reports.GetMonthlySummary;

public interface IGetMonthlySummaryUseCase : IUseCase<GetMonthlySummaryInput, Task<MonthlySummaryOutput>>
{
}
