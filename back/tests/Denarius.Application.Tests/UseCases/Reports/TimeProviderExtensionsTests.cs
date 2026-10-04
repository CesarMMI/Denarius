using Denarius.Application.UseCases.Reports;
using NSubstitute;

namespace Denarius.Application.Tests.UseCases.Reports;

public class TimeProviderExtensionsTests
{
    private readonly TimeProvider _timeProvider = Substitute.For<TimeProvider>();

    [Theory]
    [InlineData(2026, 10, 1, 2, 2026, 9, 30)]
    [InlineData(2026, 10, 1, 3, 2026, 10, 1)]
    [InlineData(2026, 1, 1, 2, 2025, 12, 31)]
    [InlineData(2026, 9, 15, 12, 2026, 9, 15)]
    public void GetTodayInSaoPaulo_ReturnsTheDayInSaoPauloNotInUtc(int year, int month, int day, int utcHour, int expectedYear, int expectedMonth, int expectedDay)
    {
        _timeProvider.GetUtcNow().Returns(new DateTimeOffset(year, month, day, utcHour, 0, 0, TimeSpan.Zero));

        Assert.Equal(new DateOnly(expectedYear, expectedMonth, expectedDay), _timeProvider.GetTodayInSaoPaulo());
    }
}
