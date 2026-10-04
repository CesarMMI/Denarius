using System.Globalization;

namespace Denarius.Application.IO.Reports;

/// <summary>
/// A calendar month, written <c>yyyy-MM</c> (<c>2026-09</c>). MVC binds it from the query string through
/// <see cref="TryParse"/>, so a malformed month is rejected before any use case runs.
/// </summary>
public readonly record struct YearMonth : IComparable<YearMonth>
{
    private const string Format = "yyyy-MM";

    public DateOnly FirstDay { get; }
    public int Year => FirstDay.Year;
    public int Month => FirstDay.Month;
    public int DayCount => DateTime.DaysInMonth(Year, Month);

    public YearMonth(int year, int month)
    {
        FirstDay = new DateOnly(year, month, 1);
    }

    public YearMonth AddMonths(int months) => FromDate(FirstDay.AddMonths(months));

    public static YearMonth FromDate(DateOnly date) => new(date.Year, date.Month);

    public static bool TryParse(string? value, IFormatProvider? provider, out YearMonth result)
    {
        var parsed = DateOnly.TryParseExact(value, Format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date);
        result = parsed ? FromDate(date) : default;
        return parsed;
    }

    public int CompareTo(YearMonth other) => FirstDay.CompareTo(other.FirstDay);

    public static bool operator <(YearMonth left, YearMonth right) => left.CompareTo(right) < 0;

    public static bool operator >(YearMonth left, YearMonth right) => left.CompareTo(right) > 0;

    public override string ToString() => FirstDay.ToString(Format, CultureInfo.InvariantCulture);
}
