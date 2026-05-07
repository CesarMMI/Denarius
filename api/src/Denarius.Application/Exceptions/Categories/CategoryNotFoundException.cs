namespace Denarius.Application.Exceptions.Categories;

public class CategoryNotFoundException : NotFoundException
{
    public CategoryNotFoundException(Guid categoryId)
        : base("Category", categoryId) { }
}
