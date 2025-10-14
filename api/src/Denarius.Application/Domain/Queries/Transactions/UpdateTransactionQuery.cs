namespace Denarius.Application.Domain.Queries.Transactions;

public class UpdateTransactionQuery : QueryId
{
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public string Description { get; set; } = string.Empty;
    public int? CategoryId { get; set; }
}
