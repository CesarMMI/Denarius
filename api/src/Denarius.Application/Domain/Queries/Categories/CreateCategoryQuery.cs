using Denarius.Application.Extensions;
using Denarius.Domain.Enums;
using Denarius.Domain.Exceptions;

namespace Denarius.Application.Domain.Queries.Categories;

public class CreateCategoryQuery : Query
{
    public string Name { get; set; } = string.Empty;
    public string? Color { get; set; }
    public ECategoryType Type { get; set; }

    public override void Validate()
    {
        base.Validate();

        if (!Name.IsValidString())
            throw new BadRequestException("Name is required");
        if (Name.Length < 3)
            throw new BadRequestException("Name length can't be lower than 3");
        if (Name.Length > 50)
            throw new BadRequestException("Name length can't be greater than 50");

        if (Color is not null && !Color.IsValidColor())
            throw new BadRequestException("Invalid color");

        if (!Type.IsValidEnum())
            throw new BadRequestException("Invalid type");
    }
}
