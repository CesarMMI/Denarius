using Denarius.Application.IO.Reports;

namespace Denarius.Application.Tests.IO.Reports;

public class YearMonthTests
{
    [Fact]
    public void TryParse_YearDashMonth_ReturnsTheMonth()
    {
        var parsed = YearMonth.TryParse("2026-09", null, out var month);

        Assert.True(parsed);
        Assert.Equal(new YearMonth(2026, 9), month);
        Assert.Equal(2026, month.Year);
        Assert.Equal(9, month.Month);
    }

    [Theory]
    [InlineData("2026-9")]
    [InlineData("2026-13")]
    [InlineData("2026-00")]
    [InlineData("09-2026")]
    [InlineData("26-09")]
    [InlineData("2026-09-01")]
    [InlineData(" 2026-09")]
    [InlineData("abc")]
    [InlineData("")]
    [InlineData(null)]
    public void TryParse_AnythingButYearDashMonth_Fails(string? value)
    {
        Assert.False(YearMonth.TryParse(value, null, out _));
    }

    [Theory]
    [InlineData(2026, 9, 1, 2026, 10)]
    [InlineData(2026, 12, 1, 2027, 1)]
    [InlineData(2026, 1, -1, 2025, 12)]
    [InlineData(2026, 2, -3, 2025, 11)]
    [InlineData(2026, 9, 0, 2026, 9)]
    public void AddMonths_MovesAcrossYears(int year, int month, int months, int expectedYear, int expectedMonth)
    {
        Assert.Equal(new YearMonth(expectedYear, expectedMonth), new YearMonth(year, month).AddMonths(months));
    }

    [Theory]
    [InlineData(2026, 9, 30)]
    [InlineData(2026, 10, 31)]
    [InlineData(2026, 2, 28)]
    [InlineData(2028, 2, 29)]
    public void DayCount_ReturnsTheDaysOfTheMonth(int year, int month, int expected)
    {
        Assert.Equal(expected, new YearMonth(year, month).DayCount);
    }

    [Fact]
    public void FirstDay_ReturnsTheFirstDayOfTheMonth()
    {
        Assert.Equal(new DateOnly(2026, 9, 1), new YearMonth(2026, 9).FirstDay);
    }

    [Fact]
    public void FromDate_ReturnsTheMonthOfTheDay()
    {
        Assert.Equal(new YearMonth(2026, 9), YearMonth.FromDate(new DateOnly(2026, 9, 30)));
    }

    [Fact]
    public void CompareTo_OrdersChronologicallyAcrossYears()
    {
        var december = new YearMonth(2025, 12);
        var january = new YearMonth(2026, 1);

        Assert.True(december.CompareTo(january) < 0);
        Assert.True(january.CompareTo(december) > 0);
        Assert.Equal(0, january.CompareTo(new YearMonth(2026, 1)));
        Assert.True(december < january);
        Assert.True(january > december);
    }

    [Theory]
    [InlineData(2026, 9, "2026-09")]
    [InlineData(987, 1, "0987-01")]
    public void ToString_WritesYearDashMonth(int year, int month, string expected)
    {
        Assert.Equal(expected, new YearMonth(year, month).ToString());
    }

    [Theory]
    [InlineData(2026, 0)]
    [InlineData(2026, 13)]
    public void Constructor_MonthOutOfRange_Throws(int year, int month)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new YearMonth(year, month));
    }
}
