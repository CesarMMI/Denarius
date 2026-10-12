using Denarius.Application;
using Denarius.Application.IO.Transactions;
using Denarius.Application.UseCases.Transactions.List;
using Denarius.Domain.Entities;
using Denarius.Domain.Repositories;
using Denarius.Domain.ValueObjects;
using Denarius.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Denarius.Infrastructure.Tests.Repositories;

public class TransactionRepositoryTests(PostgreSqlFixture fixture) : IClassFixture<PostgreSqlFixture>, IAsyncLifetime
{
    public async Task InitializeAsync()
    {
        await using var context = fixture.CreateContext();
        await context.Database.ExecuteSqlRawAsync("""TRUNCATE TABLE "Transactions", "Categories" CASCADE""");
    }

    public Task DisposeAsync() => Task.CompletedTask;

    private static DateTime Utc(int year, int month, int day, int hour = 0, int minute = 0, int second = 0, int millisecond = 0) =>
        new(year, month, day, hour, minute, second, millisecond, DateTimeKind.Utc);

    private async Task<Category> AddCategoryAsync(string name = "Mercado")
    {
        await using var context = fixture.CreateContext();
        var category = new Category(name, new Color("#FF0000"));
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        return category;
    }

    private int _created;

    /// <summary>Adds the transactions with creation times that grow in the order given, across calls.</summary>
    private async Task AddAsync(Category category, params (string? Description, DateTime Date, decimal Value)[] items)
    {
        await using var context = fixture.CreateContext();

        foreach (var (description, date, value) in items)
            context.Transactions.Add(new Transaction(description, date, value, category.Id) { CreatedAt = Utc(2026, 1, 1).AddMinutes(_created++) });

        await context.SaveChangesAsync();
    }

    private async Task<List<string?>> ListDescriptionsAsync(TransactionListQuery query)
    {
        await using var context = fixture.CreateContext();
        var transactions = await new TransactionRepository(context).ListAsync(query);

        return transactions.Select(t => t.Description).ToList();
    }

    [Theory]
    [InlineData("farmacia sao", "Farmácia São João")]
    [InlineData("FARMÁCIA", "Farmácia São João")]
    [InlineData("são joão", "Farmácia São João")]
    [InlineData("farma", "Farmácia São João")]
    [InlineData("padariá", "Padaria")]
    [InlineData("acai", "Açaí")]
    [InlineData("AÇAÍ", "Açaí")]
    [InlineData("açaí", "Açaí")]
    public async Task ListAsync_WithDescription_IgnoresCaseAndAccents(string search, string expected)
    {
        var category = await AddCategoryAsync();
        await AddAsync(category,
            ("Farmácia São João", Utc(2026, 3, 1), -10m),
            ("Padaria", Utc(2026, 3, 2), -10m),
            ("Mercado", Utc(2026, 3, 3), -10m),
            ("Açaí", Utc(2026, 3, 4), -10m));

        var descriptions = await ListDescriptionsAsync(new TransactionListQuery { Description = search });

        Assert.Equal([expected], descriptions);
    }

    [Fact]
    public async Task ListAsync_WithDescription_NeverReturnsTransactionsWithoutDescription()
    {
        var category = await AddCategoryAsync();
        await AddAsync(category, (null, Utc(2026, 3, 1), -10m), ("Mercado", Utc(2026, 3, 2), -10m));

        var descriptions = await ListDescriptionsAsync(new TransactionListQuery { Description = "e" });

        Assert.Equal(["Mercado"], descriptions);
    }

    [Fact]
    public async Task ListAsync_WithDescriptionMatchingNothing_ReturnsEmpty()
    {
        var category = await AddCategoryAsync();
        await AddAsync(category, ("Mercado", Utc(2026, 3, 1), -10m));

        Assert.Empty(await ListDescriptionsAsync(new TransactionListQuery { Description = "Lazer" }));
    }

    [Theory]
    [InlineData("%", "Desconto 50%")]
    [InlineData("_", "a_b")]
    [InlineData("\\", "caminho\\arquivo")]
    [InlineData("a_b", "a_b")]
    public async Task ListAsync_WithWildcardCharacters_TreatsThemAsText(string search, string expected)
    {
        var category = await AddCategoryAsync();
        await AddAsync(category,
            ("Desconto 50%", Utc(2026, 3, 1), -10m),
            ("a_b", Utc(2026, 3, 2), -10m),
            ("caminho\\arquivo", Utc(2026, 3, 3), -10m),
            ("Padaria", Utc(2026, 3, 4), -10m));

        var descriptions = await ListDescriptionsAsync(new TransactionListQuery { Description = search });

        Assert.Equal([expected], descriptions);
    }

    [Fact]
    public async Task ListAsync_WithMonth_KeepsFromTheStartOfTheMonthUpToTheStartOfTheNext()
    {
        var category = await AddCategoryAsync();
        await AddAsync(category,
            ("fim de fevereiro", Utc(2026, 2, 28, 23, 59, 59, 999), -10m),
            ("início de março", Utc(2026, 3, 1), -10m),
            ("meio de março", Utc(2026, 3, 15, 12), -10m),
            ("fim de março", Utc(2026, 3, 31, 23, 59, 59, 999), -10m),
            ("início de abril", Utc(2026, 4, 1), -10m));

        var march = await ListDescriptionsAsync(new TransactionListQuery { From = Utc(2026, 3, 1), To = Utc(2026, 4, 1) });
        var february = await ListDescriptionsAsync(new TransactionListQuery { From = Utc(2026, 2, 1), To = Utc(2026, 3, 1) });

        Assert.Equal(["fim de março", "meio de março", "início de março"], march);
        Assert.Equal(["fim de fevereiro"], february);
    }

    [Theory]
    [InlineData(TransactionKind.All, new[] { "Salário", "Cinema" })]
    [InlineData(TransactionKind.In, new[] { "Salário" })]
    [InlineData(TransactionKind.Out, new[] { "Cinema" })]
    public async Task ListAsync_WithType_KeepsOnlyTransactionsOfThatType(TransactionKind type, string[] expected)
    {
        var category = await AddCategoryAsync();
        await AddAsync(category, ("Salário", Utc(2026, 8, 5), 1000m), ("Cinema", Utc(2026, 8, 1), -100m));

        var descriptions = await ListDescriptionsAsync(new TransactionListQuery { Type = type });

        Assert.Equal(expected, descriptions);
    }

    [Fact]
    public async Task ListAsync_WithCategory_KeepsOnlyTransactionsOfThatCategory()
    {
        var leisure = await AddCategoryAsync("Lazer");
        var market = await AddCategoryAsync("Mercado");
        await AddAsync(leisure, ("Cinema", Utc(2026, 8, 1), -100m));
        await AddAsync(market, ("Feira", Utc(2026, 8, 2), -80m));
        await AddAsync(leisure, ("Show", Utc(2026, 8, 3), -200m));
        await using var context = fixture.CreateContext();
        var repository = new TransactionRepository(context);

        var transactions = (await repository.ListAsync(new TransactionListQuery { CategoryId = leisure.Id })).ToList();

        Assert.Equal(["Show", "Cinema"], transactions.Select(t => t.Description));
        Assert.All(transactions, t => Assert.Equal(leisure.Id, t.CategoryId));
        Assert.Empty(await repository.ListAsync(new TransactionListQuery { CategoryId = Guid.NewGuid() }));
    }

    [Fact]
    public async Task ListAsync_WithCombinedFilters_KeepsOnlyTransactionsMatchingAllOfThem()
    {
        var market = await AddCategoryAsync("Mercado");
        var leisure = await AddCategoryAsync("Lazer");
        await AddAsync(market,
            ("Farmácia Centro", Utc(2026, 3, 5), -50m),
            ("Farmácia Norte", Utc(2026, 4, 5), -50m),
            ("Farmácia Sul", Utc(2026, 3, 6), 50m),
            ("Mercado", Utc(2026, 3, 8), -50m));
        await AddAsync(leisure, ("Farmácia Leste", Utc(2026, 3, 7), -50m));

        var descriptions = await ListDescriptionsAsync(new TransactionListQuery
        {
            Description = "farmacia",
            From = Utc(2026, 3, 1),
            To = Utc(2026, 4, 1),
            Type = TransactionKind.Out,
            CategoryId = market.Id
        });

        Assert.Equal(["Farmácia Centro"], descriptions);
    }

    [Fact]
    public async Task ListAsync_WithoutFilters_ReturnsEverythingFromTheMostRecentDateToTheOldest()
    {
        var category = await AddCategoryAsync();
        await AddAsync(category,
            ("A", Utc(2026, 3, 10), -10m),
            ("B", Utc(2026, 3, 10), -10m),
            ("C", Utc(2026, 3, 5), -10m),
            ("D", Utc(2026, 3, 20), -10m));

        var descriptions = await ListDescriptionsAsync(new TransactionListQuery());

        Assert.Equal(["D", "B", "A", "C"], descriptions);
    }

    [Fact]
    public async Task ListAsync_OrderByDateAscending_OrdersTiesByCreationInTheSameDirection()
    {
        var category = await AddCategoryAsync();
        await AddAsync(category,
            ("A", Utc(2026, 3, 10), -10m),
            ("B", Utc(2026, 3, 10), -10m),
            ("C", Utc(2026, 3, 5), -10m),
            ("D", Utc(2026, 3, 20), -10m));

        var descriptions = await ListDescriptionsAsync(new TransactionListQuery { SortBy = TransactionSortField.Date, Ascending = true });

        Assert.Equal(["C", "A", "B", "D"], descriptions);
    }

    [Theory]
    [InlineData(true, new[] { "Aluguel", "Farmácia", "Salário" })]
    [InlineData(false, new[] { "Salário", "Farmácia", "Aluguel" })]
    public async Task ListAsync_OrderByValue_OrdersBySignedValue(bool ascending, string[] expected)
    {
        var category = await AddCategoryAsync();
        await AddAsync(category,
            ("Farmácia", Utc(2026, 8, 1), -20m),
            ("Salário", Utc(2026, 8, 2), 3000m),
            ("Aluguel", Utc(2026, 8, 3), -1500m));

        var descriptions = await ListDescriptionsAsync(new TransactionListQuery { SortBy = TransactionSortField.Value, Ascending = ascending });

        Assert.Equal(expected, descriptions);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ListAsync_OrderByValueWithTies_OrdersTheTiesByMostRecentDateThenCreation(bool ascending)
    {
        var category = await AddCategoryAsync();
        await AddAsync(category,
            ("P", Utc(2026, 3, 1), 10m),
            ("S", Utc(2026, 3, 2), 10m),
            ("Q", Utc(2026, 3, 3), 10m),
            ("R", Utc(2026, 3, 3), 10m));

        var descriptions = await ListDescriptionsAsync(new TransactionListQuery { SortBy = TransactionSortField.Value, Ascending = ascending });

        Assert.Equal(["R", "Q", "S", "P"], descriptions);
    }

    [Fact]
    public async Task ListAsync_WithMonth_ReturnsOnlyTransactionsWithinTheMonth()
    {
        var category = await AddCategoryAsync();
        await AddAsync(category,
            ("Início do mês", Utc(2026, 8, 1), 100m),
            ("Fim do mês", Utc(2026, 8, 31, 23, 59, 59), 50m),
            ("Mês seguinte", Utc(2026, 9, 1), 500m),
            ("Mês anterior", Utc(2026, 7, 31, 23, 59, 59), 300m));

        var descriptions = await ListDescriptionsAsync(new TransactionListQuery { From = Utc(2026, 8, 1), To = Utc(2026, 9, 1) });

        Assert.Equal(["Fim do mês", "Início do mês"], descriptions);
    }

    [Fact]
    public async Task ListAsync_WithCombinedFilters_ReturnsOnlyTheTransactionMatchingAllOfThem()
    {
        var leisure = await AddCategoryAsync("Lazer");
        var market = await AddCategoryAsync("Mercado");
        await AddAsync(leisure,
            ("Cinema", Utc(2026, 8, 10), -100m),
            ("Cinema reembolso", Utc(2026, 8, 11), 100m),
            ("Cinema", Utc(2026, 9, 10), -100m));
        await AddAsync(market, ("Cinema", Utc(2026, 8, 12), -100m));
        await AddAsync(leisure, ("Show", Utc(2026, 8, 13), -200m));
        await using var context = fixture.CreateContext();

        var transactions = await new TransactionRepository(context).ListAsync(new TransactionListQuery
        {
            Description = "cinema",
            From = Utc(2026, 8, 1),
            To = Utc(2026, 9, 1),
            Type = TransactionKind.Out,
            CategoryId = leisure.Id
        });

        var single = Assert.Single(transactions);
        Assert.Equal(Utc(2026, 8, 10), single.Date);
        Assert.Equal(leisure.Id, single.CategoryId);
    }

    [Theory]
    [InlineData(true, new[] { "Aluguel", "Farmácia", "Mercado" })]
    [InlineData(false, new[] { "Mercado", "Farmácia", "Aluguel" })]
    public async Task ListAsync_OrderByDescription_OrdersAlphabetically(bool ascending, string[] expected)
    {
        var category = await AddCategoryAsync();
        await AddAsync(category,
            ("Farmácia", Utc(2026, 8, 1), -20m),
            ("Mercado", Utc(2026, 8, 2), -80m),
            ("Aluguel", Utc(2026, 8, 3), -1500m));

        var descriptions = await ListDescriptionsAsync(new TransactionListQuery { SortBy = TransactionSortField.Description, Ascending = ascending });

        Assert.Equal(expected, descriptions);
    }

    [Theory]
    [InlineData(true, new[] { "Cinema", "Feira", "Salário" })]
    [InlineData(false, new[] { "Salário", "Feira", "Cinema" })]
    public async Task ListAsync_OrderByCategoryName_OrdersByTheCategoryName(bool ascending, string[] expected)
    {
        var leisure = await AddCategoryAsync("Lazer");
        var market = await AddCategoryAsync("Mercado");
        var work = await AddCategoryAsync("Trabalho");
        await AddAsync(work, ("Salário", Utc(2026, 8, 1), 3000m));
        await AddAsync(leisure, ("Cinema", Utc(2026, 8, 2), -100m));
        await AddAsync(market, ("Feira", Utc(2026, 8, 3), -80m));

        var descriptions = await ListDescriptionsAsync(new TransactionListQuery { SortBy = TransactionSortField.CategoryName, Ascending = ascending });

        Assert.Equal(expected, descriptions);
    }

    [Theory]
    [InlineData(false, new[] { "Lançada por último", "Lançada primeiro" })]
    [InlineData(true, new[] { "Lançada primeiro", "Lançada por último" })]
    public async Task ListAsync_OrderByDateWithTies_OrdersTheTiesByCreationInTheSameDirection(bool ascending, string[] expected)
    {
        var category = await AddCategoryAsync();
        await AddAsync(category,
            ("Lançada primeiro", Utc(2026, 8, 1), 10m),
            ("Lançada por último", Utc(2026, 8, 1), 10m));

        var descriptions = await ListDescriptionsAsync(new TransactionListQuery { SortBy = TransactionSortField.Date, Ascending = ascending });

        Assert.Equal(expected, descriptions);
    }

    [Fact]
    public async Task ListAsync_OrderByAnotherFieldWithTies_OrdersTheTiesByMostRecentDateThenCreation()
    {
        var category = await AddCategoryAsync();
        await AddAsync(category,
            ("Mercado", Utc(2026, 8, 2), -30m),
            ("Mercado", Utc(2026, 8, 1), -10m),
            ("Mercado", Utc(2026, 8, 1), -20m));
        await using var context = fixture.CreateContext();

        var transactions = await new TransactionRepository(context).ListAsync(new TransactionListQuery { SortBy = TransactionSortField.Description, Ascending = true });

        Assert.Equal([-30m, -20m, -10m], transactions.Select(t => t.Value));
    }

    [Fact]
    public async Task GetAllAsync_ReturnsTheTransactionsWithoutTrackingThem()
    {
        var category = await AddCategoryAsync();
        await AddAsync(category, ("Mercado", Utc(2026, 3, 1), -10m), ("Padaria", Utc(2026, 3, 2), -10m));
        await using var context = fixture.CreateContext();

        var transactions = await new TransactionRepository(context).GetAllAsync();

        Assert.Equal(2, transactions.Count());
        Assert.Empty(context.ChangeTracker.Entries());
    }

    [Fact]
    public async Task ListAsync_RunsTheFiltersAndTheOrderingInTheDatabase()
    {
        var category = await AddCategoryAsync();
        await AddAsync(category, ("Farmácia", Utc(2026, 3, 1), -10m), ("Mercado", Utc(2026, 4, 1), -10m));
        var commands = new List<string>();
        await using var context = fixture.CreateContext(commands.Add);

        var transactions = await new TransactionRepository(context).ListAsync(new TransactionListQuery
        {
            Description = "farmacia",
            From = Utc(2026, 3, 1),
            To = Utc(2026, 4, 1),
            Type = TransactionKind.Out,
            CategoryId = category.Id,
            SortBy = TransactionSortField.Value
        });

        Assert.Single(transactions);
        var command = Assert.Single(commands);
        Assert.Contains("WHERE", command);
        Assert.Contains("ORDER BY", command);
        Assert.Contains("unaccent", command, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("farmacia", command, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(true, new[] { "agua", "Agua", "água", "Água", "Banana" })]
    [InlineData(false, new[] { "Banana", "Água", "água", "Agua", "agua" })]
    public async Task ListAsync_OrderByDescription_FollowsThePortugueseAlphabeticalOrder(bool ascending, string[] expected)
    {
        var category = await AddCategoryAsync();
        await AddAsync(category,
            ("Banana", Utc(2026, 3, 1), -10m),
            ("Água", Utc(2026, 3, 2), -10m),
            ("agua", Utc(2026, 3, 3), -10m),
            ("água", Utc(2026, 3, 4), -10m),
            ("Agua", Utc(2026, 3, 5), -10m));

        var descriptions = await ListDescriptionsAsync(new TransactionListQuery { SortBy = TransactionSortField.Description, Ascending = ascending });

        Assert.Equal(expected, descriptions);
    }

    [Fact]
    public async Task ListAsync_OrderByDescription_PutsSpacesAndHyphensBeforeLettersAndDigitsBeforeLetters()
    {
        var category = await AddCategoryAsync();
        await AddAsync(category,
            ("Aguardente", Utc(2026, 3, 1), -10m),
            ("Água-viva", Utc(2026, 3, 2), -10m),
            ("Agua 2", Utc(2026, 3, 3), -10m),
            ("Água de coco", Utc(2026, 3, 4), -10m));

        var descriptions = await ListDescriptionsAsync(new TransactionListQuery { SortBy = TransactionSortField.Description, Ascending = true });

        Assert.Equal(["Agua 2", "Água de coco", "Água-viva", "Aguardente"], descriptions);
    }

    [Theory]
    [InlineData(true, new[] { null, null, "agua", "Banana" })]
    [InlineData(false, new[] { "Banana", "agua", null, null })]
    public async Task ListAsync_OrderByDescription_PutsTransactionsWithoutDescriptionFirstWhenAscendingAndLastWhenDescending(bool ascending, string?[] expected)
    {
        var category = await AddCategoryAsync();
        await AddAsync(category,
            (null, Utc(2026, 3, 1), -10m),
            ("Banana", Utc(2026, 3, 2), -10m),
            ("agua", Utc(2026, 3, 3), -10m),
            (null, Utc(2026, 3, 4), -10m));

        var descriptions = await ListDescriptionsAsync(new TransactionListQuery { SortBy = TransactionSortField.Description, Ascending = ascending });

        Assert.Equal(expected, descriptions);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ListAsync_OrderByDescriptionWithEqualDescriptions_OrdersTheTiesByMostRecentDateThenCreation(bool ascending)
    {
        var category = await AddCategoryAsync();
        await AddAsync(category,
            ("Mercado", Utc(2026, 3, 1), 1m),
            ("Mercado", Utc(2026, 3, 2), 2m),
            ("Mercado", Utc(2026, 3, 2), 3m));
        await using var context = fixture.CreateContext();

        var transactions = await new TransactionRepository(context).ListAsync(new TransactionListQuery { SortBy = TransactionSortField.Description, Ascending = ascending });

        Assert.Equal([3m, 2m, 1m], transactions.Select(t => t.Value));
    }

    [Theory]
    [InlineData(true, new[] { "da Abacaxi", "da agua", "da Maçã" })]
    [InlineData(false, new[] { "da Maçã", "da agua", "da Abacaxi" })]
    public async Task ListAsync_OrderByCategoryName_UsesTheCurrentNameOfTheCategory(bool ascending, string[] expected)
    {
        var renamed = await AddCategoryAsync("Zebra");
        var apple = await AddCategoryAsync("Maçã");
        var water = await AddCategoryAsync("Água");
        await AddAsync(renamed, ("da Abacaxi", Utc(2026, 3, 1), -10m));
        await AddAsync(apple, ("da Maçã", Utc(2026, 3, 2), -10m));
        await AddAsync(water, ("da agua", Utc(2026, 3, 3), -10m));
        await using (var context = fixture.CreateContext())
            await context.Database.ExecuteSqlInterpolatedAsync($"""UPDATE "Categories" SET "Name" = 'Abacaxi' WHERE "Id" = {renamed.Id}""");

        var descriptions = await ListDescriptionsAsync(new TransactionListQuery { SortBy = TransactionSortField.CategoryName, Ascending = ascending });

        Assert.Equal(expected, descriptions);
    }

    [Fact]
    public async Task ListAsync_OrderByCategoryNameWithTheSameCategory_OrdersTheTiesByMostRecentDateThenCreation()
    {
        var category = await AddCategoryAsync();
        await AddAsync(category,
            ("A", Utc(2026, 3, 1), -10m),
            ("B", Utc(2026, 3, 2), -10m),
            ("C", Utc(2026, 3, 2), -10m));

        var descriptions = await ListDescriptionsAsync(new TransactionListQuery { SortBy = TransactionSortField.CategoryName, Ascending = true });

        Assert.Equal(["C", "B", "A"], descriptions);
    }

    [Fact]
    public async Task ListAsync_JoinsTheCategoriesOnlyWhenOrderingByCategoryName()
    {
        var category = await AddCategoryAsync();
        await AddAsync(category, ("Mercado", Utc(2026, 3, 1), -10m));

        async Task<string> CommandAsync(TransactionSortField sortBy)
        {
            var commands = new List<string>();
            await using var context = fixture.CreateContext(commands.Add);
            await new TransactionRepository(context).ListAsync(new TransactionListQuery { SortBy = sortBy });

            return Assert.Single(commands);
        }

        var byCategoryName = await CommandAsync(TransactionSortField.CategoryName);
        var byDescription = await CommandAsync(TransactionSortField.Description);
        var byDate = await CommandAsync(TransactionSortField.Date);

        Assert.Contains("JOIN \"Categories\"", byCategoryName);
        Assert.Contains("COLLATE pt_br_icu", byCategoryName);
        Assert.DoesNotContain("Categories", byDescription);
        Assert.Contains("COLLATE pt_br_icu", byDescription);
        Assert.DoesNotContain("Categories", byDate);
        Assert.DoesNotContain("COLLATE", byDate);
    }

    [Theory]
    [InlineData(DateTimeKind.Unspecified)]
    [InlineData(DateTimeKind.Utc)]
    [InlineData(DateTimeKind.Local)]
    public async Task UseCase_WithDateRefOfAnyKind_ListsTheSameMonth(DateTimeKind kind)
    {
        var category = await AddCategoryAsync();
        await AddAsync(category,
            ("fim de fevereiro", Utc(2026, 2, 28, 23, 59, 59, 999), -10m),
            ("início de março", Utc(2026, 3, 1), -10m),
            ("fim de março", Utc(2026, 3, 31, 23, 59, 59, 999), -10m),
            ("início de abril", Utc(2026, 4, 1), -10m));
        await using var context = fixture.CreateContext();
        var services = new ServiceCollection().AddApplication();
        services.AddScoped<ITransactionRepository>(_ => new TransactionRepository(context));
        await using var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();
        var useCase = scope.ServiceProvider.GetRequiredService<IListTransactionsUseCase>();
        var dateRef = DateTime.SpecifyKind(new DateTime(2026, 3, 15, 12, 0, 0), kind);

        var output = await useCase.Execute(new ListTransactionsInput(dateRef: dateRef));

        Assert.Equal(["fim de março", "início de março"], output.Select(t => t.Description));
    }
}
