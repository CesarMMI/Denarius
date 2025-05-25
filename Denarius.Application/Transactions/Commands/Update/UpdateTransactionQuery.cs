using Denarius.Application.Shared.Command;
using Denarius.Application.Shared.Exceptions;
using Denarius.Application.Shared.Validators;

namespace Denarius.Application.Transactions.Commands.Update;

public class UpdateTransactionQuery : Query
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public string Description { get; set; } = string.Empty;
    public int AccountId { get; set; }
    public int? CategoryId { get; set; }

    public override void Validate()
    {
        base.Validate();

        if(!Id.IsValidId())
            throw new BadRequestException("Transaction Id is required");

        if (Amount == decimal.Zero)
            throw new BadRequestException("Amount can't be equal to 0");

        ValidateString(Description, "Description", 1, 50);

        if (!AccountId.IsValidId())
            throw new BadRequestException("Account Id is required");

        if (!CategoryId.IsValidId())
            throw new BadRequestException("Invalid category Id");
    }
}
