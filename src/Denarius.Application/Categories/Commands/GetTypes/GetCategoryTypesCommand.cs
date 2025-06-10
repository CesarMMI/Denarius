using Denarius.Domain.Enums;

namespace Denarius.Application.Categories.Commands.GetTypes;

public class GetCategoryTypesCommand : IGetCategoryTypesCommand
{
    public async Task<IList<GetCategoryTypesResult>> Execute(GetCategoryTypesQuery query)
    {
        var list = Enum.GetValues<ECategoryType>()
         .Select(e => new GetCategoryTypesResult { Label = e.ToString(), Value = (int)e })
         .ToList();
        return await Task.FromResult(list);
    }
}
