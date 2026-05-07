namespace Denarius.Application.Inputs.Transactions;

public record CreateTransferInput(
    Guid UserId,
    Guid SourceAccountId,
    Guid DestinationAccountId,
    decimal Amount,
    string Description,
    DateTime Date);
