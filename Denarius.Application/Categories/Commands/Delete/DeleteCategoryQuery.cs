using Denarius.Application.Shared.Command;
using Denarius.Application.Shared.Exceptions;
using Denarius.Application.Shared.Validators;

namespace Denarius.Application.Categories.Commands.Delete;

public class DeleteCategoryQuery : Query
{
    public int Id { get; set; }

    public override void Validate()
    {
        base.Validate();

        if (!Id.IsValidInt())
            throw new BadRequestException("Category Id is required");
    }
}
