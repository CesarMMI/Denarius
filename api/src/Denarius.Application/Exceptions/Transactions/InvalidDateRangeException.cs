namespace Denarius.Application.Exceptions.Transactions;

public class InvalidDateRangeException : AppException
{
    public InvalidDateRangeException()
        : base("StartDate cannot be later than EndDate.") { }
}
