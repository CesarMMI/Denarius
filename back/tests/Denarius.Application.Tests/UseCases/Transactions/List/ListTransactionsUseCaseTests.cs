using Denarius.Application.IO.Transactions;
using Denarius.Application.UseCases.Transactions.List;
using Denarius.Domain.Entities;
using Denarius.Domain.Repositories;
using Denarius.Domain.ValueObjects;
using NSubstitute;

namespace Denarius.Application.Tests.UseCases.Transactions.List;

public class ListTransactionsUseCaseTests
{
    private readonly ITransactionRepository _transactionRepository = Substitute.For<ITransactionRepository>();
    private readonly ICategoryRepository _categoryRepository = Substitute.For<ICategoryRepository>();
    private readonly IListTransactionsUseCase _useCase;

    public ListTransactionsUseCaseTests()
    {
        _useCase = new ListTransactionsUseCase(_transactionRepository, _categoryRepository);
    }

    [Fact]
    public async Task Execute_TransactionsExist_ReturnsAllTransactionsMappedToOutput()
    {
        var transactions = new List<Transaction>
        {
            new("Compras", DateTime.UtcNow, 150.75m, Guid.NewGuid()),
            new("Farmácia", DateTime.UtcNow, 42.5m, Guid.NewGuid())
        };

        _transactionRepository.GetAllAsync().Returns(transactions);

        var output = await _useCase.Execute(new ListTransactionsInput());

        Assert.Equal(2, output.Count());
        Assert.Contains(output, o => o.Description == "Compras" && o.Value == 150.75m);
        Assert.Contains(output, o => o.Description == "Farmácia" && o.Value == 42.5m);
        await _transactionRepository.Received(1).GetAllAsync();
    }

    [Fact]
    public async Task Execute_NoTransactions_ReturnsEmpty()
    {
        _transactionRepository.GetAllAsync().Returns([]);

        var output = await _useCase.Execute(new ListTransactionsInput());

        Assert.Empty(output);
    }

    [Fact]
    public async Task Execute_NoFilters_ReturnsAllTransactionsOrderedByDateDescending()
    {
        var categoryId = Guid.NewGuid();
        var transactions = new List<Transaction>
        {
            new("Meio", new DateTime(2026, 8, 15), 10m, categoryId),
            new("Antiga", new DateTime(2026, 7, 1), 10m, categoryId),
            new("Recente", new DateTime(2026, 9, 20), 10m, categoryId)
        };

        _transactionRepository.GetAllAsync().Returns(transactions);

        var output = await _useCase.Execute(new ListTransactionsInput());

        Assert.Equal(["Recente", "Meio", "Antiga"], output.Select(o => o.Description));
    }

    [Fact]
    public async Task Execute_WithDescription_ReturnsOnlyMatchingTransactionsIgnoringCase()
    {
        var categoryId = Guid.NewGuid();
        var transactions = new List<Transaction>
        {
            new("Mercado do bairro", new DateTime(2026, 8, 1), -50m, categoryId),
            new("Supermercado", new DateTime(2026, 8, 2), -80m, categoryId),
            new("Farmácia", new DateTime(2026, 8, 3), -20m, categoryId),
            new(null, new DateTime(2026, 8, 4), -10m, categoryId)
        };

        _transactionRepository.GetAllAsync().Returns(transactions);

        var output = await _useCase.Execute(new ListTransactionsInput(description: " MERCADO "));

        Assert.Equal(["Supermercado", "Mercado do bairro"], output.Select(o => o.Description));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Execute_WithBlankDescription_DoesNotFilter(string description)
    {
        var categoryId = Guid.NewGuid();
        var transactions = new List<Transaction>
        {
            new("Mercado", new DateTime(2026, 8, 1), -50m, categoryId),
            new(null, new DateTime(2026, 8, 2), -10m, categoryId)
        };

        _transactionRepository.GetAllAsync().Returns(transactions);

        var output = await _useCase.Execute(new ListTransactionsInput(description: description));

        Assert.Equal(2, output.Count());
    }

    [Fact]
    public async Task Execute_WithDateRef_ReturnsOnlyTransactionsWithinTheMonth()
    {
        var categoryId = Guid.NewGuid();
        var transactions = new List<Transaction>
        {
            new("Início do mês", new DateTime(2026, 8, 1), 100m, categoryId),
            new("Fim do mês", new DateTime(2026, 8, 31, 23, 59, 59), 50m, categoryId),
            new("Mês seguinte", new DateTime(2026, 9, 1), 500m, categoryId),
            new("Mês anterior", new DateTime(2026, 7, 31, 23, 59, 59), 300m, categoryId)
        };

        _transactionRepository.GetAllAsync().Returns(transactions);

        var output = await _useCase.Execute(new ListTransactionsInput(dateRef: new DateTime(2026, 8, 28)));

        Assert.Equal(["Fim do mês", "Início do mês"], output.Select(o => o.Description));
    }

    [Theory]
    [InlineData(TransactionType.All, new[] { "Salário", "Cinema" })]
    [InlineData(TransactionType.In, new[] { "Salário" })]
    [InlineData(TransactionType.Out, new[] { "Cinema" })]
    public async Task Execute_WithType_ReturnsOnlyTransactionsOfThatType(TransactionType type, string[] expected)
    {
        var categoryId = Guid.NewGuid();
        var transactions = new List<Transaction>
        {
            new("Salário", new DateTime(2026, 8, 5), 1000m, categoryId),
            new("Cinema", new DateTime(2026, 8, 1), -100m, categoryId)
        };

        _transactionRepository.GetAllAsync().Returns(transactions);

        var output = await _useCase.Execute(new ListTransactionsInput(type: type));

        Assert.Equal(expected, output.Select(o => o.Description));
    }

    [Fact]
    public async Task Execute_WithCategoryId_ReturnsOnlyTransactionsOfThatCategory()
    {
        var lazerId = Guid.NewGuid();
        var mercadoId = Guid.NewGuid();
        var transactions = new List<Transaction>
        {
            new("Cinema", new DateTime(2026, 8, 1), -100m, lazerId),
            new("Feira", new DateTime(2026, 8, 2), -80m, mercadoId),
            new("Show", new DateTime(2026, 8, 3), -200m, lazerId)
        };

        _transactionRepository.GetAllAsync().Returns(transactions);

        var output = await _useCase.Execute(new ListTransactionsInput(categoryId: lazerId));

        Assert.Equal(["Show", "Cinema"], output.Select(o => o.Description));
        Assert.All(output, o => Assert.Equal(lazerId, o.CategoryId));
    }

    [Fact]
    public async Task Execute_WithCombinedFilters_ReturnsOnlyTransactionsMatchingAllOfThem()
    {
        var lazerId = Guid.NewGuid();
        var mercadoId = Guid.NewGuid();
        var transactions = new List<Transaction>
        {
            new("Cinema", new DateTime(2026, 8, 10), -100m, lazerId),
            new("Cinema reembolso", new DateTime(2026, 8, 11), 100m, lazerId),
            new("Cinema", new DateTime(2026, 9, 10), -100m, lazerId),
            new("Cinema", new DateTime(2026, 8, 12), -100m, mercadoId),
            new("Show", new DateTime(2026, 8, 13), -200m, lazerId)
        };

        _transactionRepository.GetAllAsync().Returns(transactions);

        var output = await _useCase.Execute(new ListTransactionsInput("cinema", new DateTime(2026, 8, 1), TransactionType.Out, lazerId));

        var single = Assert.Single(output);
        Assert.Equal(new DateTime(2026, 8, 10), single.Date);
        Assert.Equal(lazerId, single.CategoryId);
    }

    [Fact]
    public async Task Execute_OrderByDateAscending_OrdersFromOldestToNewest()
    {
        var categoryId = Guid.NewGuid();
        var transactions = new List<Transaction>
        {
            new("Meio", new DateTime(2026, 8, 15), 10m, categoryId),
            new("Recente", new DateTime(2026, 9, 20), 10m, categoryId),
            new("Antiga", new DateTime(2026, 7, 1), 10m, categoryId)
        };

        _transactionRepository.GetAllAsync().Returns(transactions);

        var output = await _useCase.Execute(new ListTransactionsInput(orderBy: TransactionOrderField.Date, ascending: true));

        Assert.Equal(["Antiga", "Meio", "Recente"], output.Select(o => o.Description));
    }

    [Theory]
    [InlineData(true, new[] { "Aluguel", "Farmácia", "Mercado" })]
    [InlineData(false, new[] { "Mercado", "Farmácia", "Aluguel" })]
    public async Task Execute_OrderByDescription_OrdersAlphabetically(bool ascending, string[] expected)
    {
        var categoryId = Guid.NewGuid();
        var transactions = new List<Transaction>
        {
            new("Farmácia", new DateTime(2026, 8, 1), -20m, categoryId),
            new("Mercado", new DateTime(2026, 8, 2), -80m, categoryId),
            new("Aluguel", new DateTime(2026, 8, 3), -1500m, categoryId)
        };

        _transactionRepository.GetAllAsync().Returns(transactions);

        var output = await _useCase.Execute(new ListTransactionsInput(orderBy: TransactionOrderField.Description, ascending: ascending));

        Assert.Equal(expected, output.Select(o => o.Description));
    }

    [Theory]
    [InlineData(true, new[] { "Aluguel", "Farmácia", "Salário" })]
    [InlineData(false, new[] { "Salário", "Farmácia", "Aluguel" })]
    public async Task Execute_OrderByValue_OrdersBySignedValue(bool ascending, string[] expected)
    {
        var categoryId = Guid.NewGuid();
        var transactions = new List<Transaction>
        {
            new("Farmácia", new DateTime(2026, 8, 1), -20m, categoryId),
            new("Salário", new DateTime(2026, 8, 2), 3000m, categoryId),
            new("Aluguel", new DateTime(2026, 8, 3), -1500m, categoryId)
        };

        _transactionRepository.GetAllAsync().Returns(transactions);

        var output = await _useCase.Execute(new ListTransactionsInput(orderBy: TransactionOrderField.Value, ascending: ascending));

        Assert.Equal(expected, output.Select(o => o.Description));
    }

    [Theory]
    [InlineData(true, new[] { "Cinema", "Feira", "Salário" })]
    [InlineData(false, new[] { "Salário", "Feira", "Cinema" })]
    public async Task Execute_OrderByCategoryName_OrdersByTheCategoryName(bool ascending, string[] expected)
    {
        var lazer = new Category("Lazer", new Color("#FF0000"));
        var mercado = new Category("Mercado", new Color("#00FF00"));
        var trabalho = new Category("Trabalho", new Color("#0000FF"));
        var transactions = new List<Transaction>
        {
            new("Salário", new DateTime(2026, 8, 1), 3000m, trabalho.Id),
            new("Cinema", new DateTime(2026, 8, 2), -100m, lazer.Id),
            new("Feira", new DateTime(2026, 8, 3), -80m, mercado.Id)
        };

        _transactionRepository.GetAllAsync().Returns(transactions);
        _categoryRepository.GetAllAsync(null).Returns([trabalho, lazer, mercado]);

        var output = await _useCase.Execute(new ListTransactionsInput(orderBy: TransactionOrderField.CategoryName, ascending: ascending));

        Assert.Equal(expected, output.Select(o => o.Description));
        await _categoryRepository.Received(1).GetAllAsync(null);
    }

    [Theory]
    [InlineData(TransactionOrderField.Date)]
    [InlineData(TransactionOrderField.Description)]
    [InlineData(TransactionOrderField.Value)]
    public async Task Execute_NotOrderingByCategoryName_DoesNotLoadCategories(TransactionOrderField orderBy)
    {
        _transactionRepository.GetAllAsync().Returns([new("Cinema", new DateTime(2026, 8, 1), -100m, Guid.NewGuid())]);

        await _useCase.Execute(new ListTransactionsInput(orderBy: orderBy));

        await _categoryRepository.DidNotReceive().GetAllAsync(Arg.Any<string?>());
    }
}
