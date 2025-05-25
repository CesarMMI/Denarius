using Denarius.Application.Shared.Command;
using Denarius.Application.Shared.Exceptions;
using Denarius.Application.Shared.Validators;
using Denarius.Domain.Enums;

namespace Denarius.Application.Categories.Commands.Create;

public class CreateCategoryQuery : Query
{
    public string Name { get; set; } = string.Empty;
    public string? Color { get; set; }
    public ECategoryType Type { get; set; }

    public override void Validate()
    {
        base.Validate();

        ValidateString(Name, nameof(Name), 3, 50);

        if (Color is not null && !Color.IsValidColor())
            throw new BadRequestException("Invalid color");

        if (!Type.IsValidEnum())
            throw new BadRequestException("Invalid type");
    }
}
