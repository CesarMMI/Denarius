using Denarius.Application.IO.Transactions;
using Denarius.Application.UseCases.Transactions.List;
using Denarius.Domain.Entities;
using Denarius.Domain.Repositories;
using NSubstitute;

namespace Denarius.Application.Tests.UseCases.Transactions.List;

public class ListTransactionsUseCaseTests
{
    private readonly ITransactionRepository _transactionRepository = Substitute.For<ITransactionRepository>();
    private readonly IListTransactionsUseCase _useCase;
    private TransactionListQuery? _query;

    public ListTransactionsUseCaseTests()
    {
        _transactionRepository.ListAsync(Arg.Do<TransactionListQuery>(query => _query = query)).Returns([]);
        _useCase = new ListTransactionsUseCase(_transactionRepository);
    }

    [Fact]
    public async Task Execute_TransactionsExist_ReturnsThemMappedToOutputInTheOrderTheRepositoryGivesThem()
    {
        var transactions = new List<Transaction>
        {
            new("Compras", DateTime.UtcNow, 150.75m, Guid.NewGuid()),
            new("Farmácia", DateTime.UtcNow, 42.5m, Guid.NewGuid())
        };

        _transactionRepository.ListAsync(Arg.Any<TransactionListQuery>()).Returns(transactions);

        var output = await _useCase.Execute(new ListTransactionsInput());

        Assert.Equal(["Compras", "Farmácia"], output.Select(o => o.Description));
        Assert.Equal([150.75m, 42.5m], output.Select(o => o.Value));
        await _transactionRepository.Received(1).ListAsync(Arg.Any<TransactionListQuery>());
    }

    [Fact]
    public async Task Execute_NoTransactions_ReturnsEmpty()
    {
        var output = await _useCase.Execute(new ListTransactionsInput());

        Assert.Empty(output);
    }

    [Fact]
    public async Task Execute_NoFilters_AsksForEverythingOrderedByDateDescending()
    {
        await _useCase.Execute(new ListTransactionsInput());

        Assert.Equal(new TransactionListQuery
        {
            Description = null,
            From = null,
            To = null,
            Type = TransactionKind.All,
            CategoryId = null,
            SortBy = TransactionSortField.Date,
            Ascending = false
        }, _query);
    }

    [Fact]
    public async Task Execute_WithDescription_PassesItTrimmed()
    {
        await _useCase.Execute(new ListTransactionsInput(description: " MERCADO "));

        Assert.Equal("MERCADO", _query?.Description);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Execute_WithBlankDescription_DoesNotSearch(string description)
    {
        await _useCase.Execute(new ListTransactionsInput(description: description));

        Assert.Null(_query?.Description);
    }

    [Theory]
    [InlineData(DateTimeKind.Unspecified)]
    [InlineData(DateTimeKind.Utc)]
    [InlineData(DateTimeKind.Local)]
    public async Task Execute_WithDateRef_AsksForTheWholeMonthInUtc(DateTimeKind kind)
    {
        var dateRef = DateTime.SpecifyKind(new DateTime(2026, 8, 15, 13, 45, 0), kind);

        await _useCase.Execute(new ListTransactionsInput(dateRef: dateRef));

        Assert.Equal(new DateTime(2026, 8, 1, 0, 0, 0, DateTimeKind.Utc), _query?.From);
        Assert.Equal(new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc), _query?.To);
        Assert.Equal(DateTimeKind.Utc, _query?.From?.Kind);
        Assert.Equal(DateTimeKind.Utc, _query?.To?.Kind);
    }

    [Fact]
    public async Task Execute_WithDateRefInDecember_AsksForTheMonthUpToJanuaryOfTheNextYear()
    {
        await _useCase.Execute(new ListTransactionsInput(dateRef: new DateTime(2026, 12, 31, 23, 59, 59)));

        Assert.Equal(new DateTime(2026, 12, 1, 0, 0, 0, DateTimeKind.Utc), _query?.From);
        Assert.Equal(new DateTime(2027, 1, 1, 0, 0, 0, DateTimeKind.Utc), _query?.To);
    }

    [Theory]
    [InlineData(TransactionType.All, TransactionKind.All)]
    [InlineData(TransactionType.In, TransactionKind.In)]
    [InlineData(TransactionType.Out, TransactionKind.Out)]
    public async Task Execute_WithType_AsksForThatType(TransactionType type, TransactionKind expected)
    {
        await _useCase.Execute(new ListTransactionsInput(type: type));

        Assert.Equal(expected, _query?.Type);
    }

    [Fact]
    public async Task Execute_WithCategoryId_AsksForThatCategory()
    {
        var categoryId = Guid.NewGuid();

        await _useCase.Execute(new ListTransactionsInput(categoryId: categoryId));

        Assert.Equal(categoryId, _query?.CategoryId);
    }

    [Theory]
    [InlineData(TransactionOrderField.Date, TransactionSortField.Date, true)]
    [InlineData(TransactionOrderField.Date, TransactionSortField.Date, false)]
    [InlineData(TransactionOrderField.Description, TransactionSortField.Description, true)]
    [InlineData(TransactionOrderField.Description, TransactionSortField.Description, false)]
    [InlineData(TransactionOrderField.Value, TransactionSortField.Value, true)]
    [InlineData(TransactionOrderField.Value, TransactionSortField.Value, false)]
    [InlineData(TransactionOrderField.CategoryName, TransactionSortField.CategoryName, true)]
    [InlineData(TransactionOrderField.CategoryName, TransactionSortField.CategoryName, false)]
    public async Task Execute_WithOrderBy_AsksForThatFieldAndDirection(TransactionOrderField orderBy, TransactionSortField expected, bool ascending)
    {
        await _useCase.Execute(new ListTransactionsInput(orderBy: orderBy, ascending: ascending));

        Assert.Equal(expected, _query?.SortBy);
        Assert.Equal(ascending, _query?.Ascending);
    }
}
