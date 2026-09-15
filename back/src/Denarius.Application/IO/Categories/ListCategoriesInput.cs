namespace Denarius.Application.IO.Categories;

public record ListCategoriesInput
{
    public string? Name { get; init; }
    public bool? WithTransaction { get; init; }
    public DateTime? DateRef { get; init; }
    public CategoryOrderField OrderBy { get; init; }
    public bool Ascending { get; init; }

    public ListCategoriesInput(string? name, bool? withTransaction, DateTime? dateRef, CategoryOrderField orderBy = CategoryOrderField.Name, bool ascending = true)
    {
        Name = name;
        WithTransaction = withTransaction;
        DateRef = dateRef;
        OrderBy = orderBy;
        Ascending = ascending;
    }
}
