namespace Denarius.Application.Inputs.Accounts;

public record UpdateAccountInput(Guid UserId, Guid AccountId, string Name, string Color);
