using Denarius.Application.IO.Categories;
using Denarius.Application.UseCases.Categories.List;
using Denarius.Domain.Entities;
using Denarius.Domain.Repositories;
using Denarius.Domain.ValueObjects;
using NSubstitute;

namespace Denarius.Application.Tests.UseCases.Categories.List;

public class ListCategoriesUseCaseTests
{
    private readonly ICategoryRepository _categoryRepository = Substitute.For<ICategoryRepository>();
    private readonly ITransactionRepository _transactionRepository = Substitute.For<ITransactionRepository>();
    private readonly IListCategoriesUseCase _useCase;

    public ListCategoriesUseCaseTests()
    {
        _useCase = new ListCategoriesUseCase(_categoryRepository, _transactionRepository);
    }

    [Fact]
    public async Task Execute_NoFilters_ReturnsAllCategoriesOrderedByNameAscending()
    {
        var lazer = new Category("Lazer", new Color("#FF0000"));
        var mercado = new Category("Mercado", new Color("#00FF00"));

        _categoryRepository.GetAllAsync(null).Returns([mercado, lazer]);
        _transactionRepository.GetAllAsync().Returns([]);

        var output = (await _useCase.Execute(new ListCategoriesInput(null, null, null))).ToList();

        Assert.Equal(["Lazer", "Mercado"], output.Select(o => o.Name));
        await _categoryRepository.Received(1).GetAllAsync(null);
    }

    [Fact]
    public async Task Execute_WithNameFilter_PassesFilterToRepository()
    {
        var lazer = new Category("Lazer", new Color("#FF0000"));

        _categoryRepository.GetAllAsync("Laz").Returns([lazer]);
        _transactionRepository.GetAllAsync().Returns([]);

        var output = await _useCase.Execute(new ListCategoriesInput("Laz", null, null));

        Assert.Single(output);
        await _categoryRepository.Received(1).GetAllAsync("Laz");
    }

    [Fact]
    public async Task Execute_CalculatesTransactionCountAndBalancePerCategory()
    {
        var lazer = new Category("Lazer", new Color("#FF0000"));
        var mercado = new Category("Mercado", new Color("#00FF00"));

        var transactions = new List<Transaction>
        {
            new("Salário", new DateTime(2026, 8, 5), 1000m, lazer.Id),
            new("Cinema", new DateTime(2026, 8, 10), -100m, lazer.Id)
        };

        _categoryRepository.GetAllAsync(null).Returns([lazer, mercado]);
        _transactionRepository.GetAllAsync().Returns(transactions);

        var output = (await _useCase.Execute(new ListCategoriesInput(null, null, null))).ToList();

        var lazerOutput = output.Single(o => o.Name == "Lazer");
        var mercadoOutput = output.Single(o => o.Name == "Mercado");

        Assert.Equal(2, lazerOutput.TransactionCount);
        Assert.Equal(900m, lazerOutput.Balance);
        Assert.Equal(0, mercadoOutput.TransactionCount);
        Assert.Equal(0m, mercadoOutput.Balance);
    }

    [Fact]
    public async Task Execute_WithTransactionTrue_ReturnsOnlyCategoriesWithTransactions()
    {
        var lazer = new Category("Lazer", new Color("#FF0000"));
        var mercado = new Category("Mercado", new Color("#00FF00"));

        var transactions = new List<Transaction> { new("Cinema", new DateTime(2026, 8, 10), -100m, lazer.Id) };

        _categoryRepository.GetAllAsync(null).Returns([lazer, mercado]);
        _transactionRepository.GetAllAsync().Returns(transactions);

        var output = await _useCase.Execute(new ListCategoriesInput(null, true, null));

        Assert.Single(output);
        Assert.Equal("Lazer", output.Single().Name);
    }

    [Fact]
    public async Task Execute_WithTransactionFalse_ReturnsOnlyCategoriesWithoutTransactions()
    {
        var lazer = new Category("Lazer", new Color("#FF0000"));
        var mercado = new Category("Mercado", new Color("#00FF00"));

        var transactions = new List<Transaction> { new("Cinema", new DateTime(2026, 8, 10), -100m, lazer.Id) };

        _categoryRepository.GetAllAsync(null).Returns([lazer, mercado]);
        _transactionRepository.GetAllAsync().Returns(transactions);

        var output = await _useCase.Execute(new ListCategoriesInput(null, false, null));

        Assert.Single(output);
        Assert.Equal("Mercado", output.Single().Name);
    }

    [Fact]
    public async Task Execute_WithDateRef_OnlyConsidersTransactionsWithinTheMonth()
    {
        var lazer = new Category("Lazer", new Color("#FF0000"));

        var transactions = new List<Transaction>
        {
            new("Dentro do mês", new DateTime(2026, 8, 1), 100m, lazer.Id),
            new("Fim do mês", new DateTime(2026, 8, 31), 50m, lazer.Id),
            new("Fora do mês", new DateTime(2026, 9, 1), 500m, lazer.Id),
            new("Antes do mês", new DateTime(2026, 7, 31), 300m, lazer.Id)
        };

        _categoryRepository.GetAllAsync(null).Returns([lazer]);
        _transactionRepository.GetAllAsync().Returns(transactions);

        var output = await _useCase.Execute(new ListCategoriesInput(null, null, new DateTime(2026, 8, 28)));

        var lazerOutput = output.Single();
        Assert.Equal(2, lazerOutput.TransactionCount);
        Assert.Equal(150m, lazerOutput.Balance);
    }

    [Fact]
    public async Task Execute_OrderByTransactionCountDescending_OrdersCorrectly()
    {
        var lazer = new Category("Lazer", new Color("#FF0000"));
        var mercado = new Category("Mercado", new Color("#00FF00"));

        var transactions = new List<Transaction>
        {
            new("A", new DateTime(2026, 8, 1), 10m, lazer.Id),
            new("B", new DateTime(2026, 8, 2), 10m, mercado.Id),
            new("C", new DateTime(2026, 8, 3), 10m, mercado.Id)
        };

        _categoryRepository.GetAllAsync(null).Returns([lazer, mercado]);
        _transactionRepository.GetAllAsync().Returns(transactions);

        var output = (await _useCase.Execute(new ListCategoriesInput(null, null, null, CategoryOrderField.TransactionCount, false))).ToList();

        Assert.Equal(["Mercado", "Lazer"], output.Select(o => o.Name));
    }

    [Fact]
    public async Task Execute_OrderByBalanceAscending_OrdersCorrectly()
    {
        var lazer = new Category("Lazer", new Color("#FF0000"));
        var mercado = new Category("Mercado", new Color("#00FF00"));

        var transactions = new List<Transaction>
        {
            new("A", new DateTime(2026, 8, 1), 500m, lazer.Id),
            new("B", new DateTime(2026, 8, 2), 100m, mercado.Id)
        };

        _categoryRepository.GetAllAsync(null).Returns([lazer, mercado]);
        _transactionRepository.GetAllAsync().Returns(transactions);

        var output = (await _useCase.Execute(new ListCategoriesInput(null, null, null, CategoryOrderField.Balance, true))).ToList();

        Assert.Equal(["Mercado", "Lazer"], output.Select(o => o.Name));
    }

    [Fact]
    public async Task Execute_RepositoryReturnsNoMatches_ReturnsEmpty()
    {
        _categoryRepository.GetAllAsync("Inexistente").Returns([]);
        _transactionRepository.GetAllAsync().Returns([]);

        var output = await _useCase.Execute(new ListCategoriesInput("Inexistente", null, null));

        Assert.Empty(output);
    }
}
