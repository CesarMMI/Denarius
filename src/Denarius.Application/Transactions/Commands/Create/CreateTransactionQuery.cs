using Denarius.Application.Shared.Command;
using Denarius.Application.Shared.Exceptions;
using Denarius.Application.Shared.Validators;

namespace Denarius.Application.Transactions.Commands.Create;

public class CreateTransactionQuery : Query
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
        if (Description.Length < 1)
            throw new BadRequestException("Description length can't be lower than 1");
        if (Description.Length > 50)
            throw new BadRequestException("Description length can't be greater than 50");

        if (!AccountId.IsValidId())
            throw new BadRequestException("Account Id is required");

        if (!CategoryId.IsValidId())
            throw new BadRequestException("Invalid category Id");
    }
}
