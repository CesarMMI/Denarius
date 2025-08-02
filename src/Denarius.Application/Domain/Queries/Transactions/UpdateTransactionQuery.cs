using Denarius.Application.Extensions;
using Denarius.Domain.Exceptions;

namespace Denarius.Application.Domain.Queries.Transactions;

public class UpdateTransactionQuery : QueryId
{
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public string Description { get; set; } = string.Empty;
    public int AccountId { get; set; }
    public int? CategoryId { get; set; }

    public override void Validate()
    {
        base.Validate();

        if (Amount == decimal.Zero)
            throw new BadRequestException("Amount can't be equal to 0");

        if (!Description.IsValidString())
            throw new BadRequestException("Description is required");
        if (Description.Length < 3)
            throw new BadRequestException("Description length can't be lower than 3");
        if (Description.Length > 50)
            throw new BadRequestException("Description length can't be greater than 50");

        if (!AccountId.IsValidId())
            throw new BadRequestException("Account id is required");

        if (!CategoryId.IsValidId())
            throw new BadRequestException("Invalid category id");
    }
}
