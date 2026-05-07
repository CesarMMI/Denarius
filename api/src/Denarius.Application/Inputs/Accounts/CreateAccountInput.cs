namespace Denarius.Application.Inputs.Accounts;

public record CreateAccountInput(Guid UserId, string Name, string CurrencyCode, string Color);
