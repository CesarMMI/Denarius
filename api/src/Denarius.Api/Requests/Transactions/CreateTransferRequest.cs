namespace Denarius.Api.Requests.Transactions;

public record CreateTransferRequest(
    Guid SourceAccountId,
    Guid DestinationAccountId,
    decimal Amount,
    string Description,
    DateTime Date);
