namespace Denarius.Application.Exceptions.Accounts;

public class InactiveAccountException : AppException
{
    public InactiveAccountException(Guid accountId)
        : base($"Account '{accountId}' is inactive.") { }
}
