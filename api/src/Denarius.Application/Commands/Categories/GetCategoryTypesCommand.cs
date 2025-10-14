using Denarius.Application.Domain.Commands.Categories;
using Denarius.Application.Domain.Queries.Categories;
using Denarius.Application.Domain.Results.Categories;
using Denarius.Domain.Enums;

namespace Denarius.Application.Commands.Categories;

internal class GetCategoryTypesCommand : Command<GetCategoryTypesQuery, IEnumerable<CategoryTypeResult>>, IGetCategoryTypesCommand
{
    protected override async Task<IEnumerable<CategoryTypeResult>> Handle(GetCategoryTypesQuery query)
    {
        var list = Enum.GetValues<ECategoryType>()
         .Select(e => new CategoryTypeResult { Label = e.ToString(), Value = (int)e })
         .ToList();
        return await Task.FromResult(list);
    }

    protected override void Validate(GetCategoryTypesQuery query)
    {
    }
}
