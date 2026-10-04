namespace Denarius.Application.UseCases.Reports;

internal static class TimeProviderExtensions
{
    private static readonly TimeZoneInfo SaoPaulo = TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");

    /// <summary>
    /// Today in São Paulo, wherever the server runs. Transaction dates are calendar days, so the time zone only decides
    /// which day it is now: at 22:00 of September 30 in São Paulo, UTC is already on October 1.
    /// </summary>
    public static DateOnly GetTodayInSaoPaulo(this TimeProvider timeProvider) =>
        DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(timeProvider.GetUtcNow(), SaoPaulo).DateTime);
}
