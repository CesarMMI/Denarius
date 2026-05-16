namespace Denarius.Api.Requests.Transactions;

public record UpdateTransactionRequest(decimal Amount, string Description, Guid? CategoryId);
