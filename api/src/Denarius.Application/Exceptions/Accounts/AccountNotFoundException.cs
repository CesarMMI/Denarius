namespace Denarius.Application.Exceptions.Accounts;

public class AccountNotFoundException : NotFoundException
{
    public AccountNotFoundException(Guid accountId)
        : base("Account", accountId) { }
}
