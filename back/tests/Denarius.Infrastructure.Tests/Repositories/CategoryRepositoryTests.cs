using Denarius.Domain.Entities;
using Denarius.Domain.ValueObjects;
using Denarius.Infrastructure.Repositories;

namespace Denarius.Infrastructure.Tests.Repositories;

public class CategoryRepositoryTests(CategoryRepositoryTests.SeededFixture fixture) : IClassFixture<CategoryRepositoryTests.SeededFixture>
{
    public class SeededFixture : PostgreSqlFixture
    {
        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();

            await using var context = CreateContext();
            context.Categories.AddRange(
                new Category("Mercado", new Color("#FF0000")),
                new Category("Educação", new Color("#00FF00")));
            await context.SaveChangesAsync();
        }
    }

    [Theory]
    [InlineData("Mer", "Mercado")]
    [InlineData("mer", "Mercado")]
    [InlineData("MERCADO", "Mercado")]
    [InlineData("Educa", "Educação")]
    [InlineData("educação", "Educação")]
    [InlineData("EDUCAÇÃO", "Educação")]
    [InlineData("educacao", "Educação")]
    [InlineData("EDUCACAO", "Educação")]
    public async Task GetAllAsync_WithName_IgnoresCaseAndAccents(string name, string expected)
    {
        await using var context = fixture.CreateContext();
        var repository = new CategoryRepository(context);

        var categories = await repository.GetAllAsync(name);

        Assert.Equal([expected], categories.Select(c => c.Name));
    }

    [Fact]
    public async Task GetAllAsync_WithNameMatchingNothing_ReturnsEmpty()
    {
        await using var context = fixture.CreateContext();
        var repository = new CategoryRepository(context);

        var categories = await repository.GetAllAsync("Lazer");

        Assert.Empty(categories);
    }

    [Fact]
    public async Task GetAllAsync_WithLikeWildcards_TreatsThemAsText()
    {
        await using var context = fixture.CreateContext();
        var repository = new CategoryRepository(context);

        Assert.Empty(await repository.GetAllAsync("%"));
        Assert.Empty(await repository.GetAllAsync("_"));
    }
}
