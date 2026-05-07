namespace Denarius.Application.Outputs.Transactions;

public record CreateTransferOutput(TransactionOutput Outgoing, TransactionOutput Incoming);
