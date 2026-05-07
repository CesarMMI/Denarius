using Denarius.Application.Exceptions.Transactions;
using Denarius.Application.Inputs.Transactions;
using Denarius.Application.UseCases.Transactions;
using Denarius.Domain.Entities;
using Denarius.Domain.Enums;
using Denarius.Domain.Interfaces.Repositories;
using NSubstitute;

namespace Denarius.Application.Tests.UseCases.Transactions;

public class ListTransactionsUseCaseTests
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly ListTransactionsUseCase _sut;

    public ListTransactionsUseCaseTests()
    {
        _transactionRepository = Substitute.For<ITransactionRepository>();
        _sut = new ListTransactionsUseCase(_transactionRepository);
    }

    // -------------------------------------------------------------------------
    // Execute — happy path
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Execute_WithNoFilters_ReturnsAllTransactions()
    {
        var userId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var accountId = Guid.NewGuid();
        var transactions = new List<Transaction>
        {
            new(userId, accountId, categoryId, null, TransactionType.Income, 500m, "Salário", DateTime.UtcNow),
            new(userId, accountId, categoryId, null, TransactionType.Expense, 100m, "Almoço", DateTime.UtcNow)
        };
        _transactionRepository.ListByUserAsync(userId, null, null, null, null, null).Returns(transactions);

        var result = await _sut.Execute(new ListTransactionsInput(userId, null, null, null, null, null));

        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task Execute_WithNoTransactions_ReturnsEmptyList()
    {
        var userId = Guid.NewGuid();
        _transactionRepository.ListByUserAsync(userId, null, null, null, null, null)
            .Returns(Enumerable.Empty<Transaction>());

        var result = await _sut.Execute(new ListTransactionsInput(userId, null, null, null, null, null));

        Assert.Empty(result);
    }

    [Fact]
    public async Task Execute_PassesAllFiltersToRepository()
    {
        var userId = Guid.NewGuid();
        var accountId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var startDate = new DateTime(2024, 1, 1);
        var endDate = new DateTime(2024, 12, 31);
        _transactionRepository.ListByUserAsync(userId, accountId, categoryId, TransactionType.Income, startDate, endDate)
            .Returns(Enumerable.Empty<Transaction>());

        await _sut.Execute(new ListTransactionsInput(userId, accountId, categoryId, TransactionType.Income, startDate, endDate));

        await _transactionRepository.Received(1).ListByUserAsync(userId, accountId, categoryId, TransactionType.Income, startDate, endDate);
    }

    [Fact]
    public async Task Execute_WithOnlyStartDate_DoesNotThrow()
    {
        var userId = Guid.NewGuid();
        _transactionRepository
            .ListByUserAsync(Arg.Any<Guid>(), Arg.Any<Guid?>(), Arg.Any<Guid?>(), Arg.Any<TransactionType?>(), Arg.Any<DateTime?>(), Arg.Any<DateTime?>())
            .Returns(Enumerable.Empty<Transaction>());

        var exception = await Record.ExceptionAsync(() =>
            _sut.Execute(new ListTransactionsInput(userId, null, null, null, DateTime.UtcNow, null)));

        Assert.Null(exception);
    }

    [Fact]
    public async Task Execute_WithOnlyEndDate_DoesNotThrow()
    {
        var userId = Guid.NewGuid();
        _transactionRepository
            .ListByUserAsync(Arg.Any<Guid>(), Arg.Any<Guid?>(), Arg.Any<Guid?>(), Arg.Any<TransactionType?>(), Arg.Any<DateTime?>(), Arg.Any<DateTime?>())
            .Returns(Enumerable.Empty<Transaction>());

        var exception = await Record.ExceptionAsync(() =>
            _sut.Execute(new ListTransactionsInput(userId, null, null, null, null, DateTime.UtcNow)));

        Assert.Null(exception);
    }

    [Fact]
    public async Task Execute_WithStartDateEqualToEndDate_DoesNotThrow()
    {
        var userId = Guid.NewGuid();
        _transactionRepository
            .ListByUserAsync(Arg.Any<Guid>(), Arg.Any<Guid?>(), Arg.Any<Guid?>(), Arg.Any<TransactionType?>(), Arg.Any<DateTime?>(), Arg.Any<DateTime?>())
            .Returns(Enumerable.Empty<Transaction>());

        var exception = await Record.ExceptionAsync(() =>
            _sut.Execute(new ListTransactionsInput(userId, null, null, null, new DateTime(2024, 6, 15), new DateTime(2024, 6, 15))));

        Assert.Null(exception);
    }

    // -------------------------------------------------------------------------
    // Execute — errors
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Execute_WithStartDateAfterEndDate_ThrowsInvalidDateRangeException()
    {
        var userId = Guid.NewGuid();
        var startDate = new DateTime(2024, 12, 31);
        var endDate = new DateTime(2024, 1, 1);

        await Assert.ThrowsAsync<InvalidDateRangeException>(() =>
            _sut.Execute(new ListTransactionsInput(userId, null, null, null, startDate, endDate)));
    }

    [Fact]
    public async Task Execute_WithStartDateAfterEndDate_DoesNotCallRepository()
    {
        var userId = Guid.NewGuid();

        await Assert.ThrowsAsync<InvalidDateRangeException>(() =>
            _sut.Execute(new ListTransactionsInput(userId, null, null, null,
                new DateTime(2024, 12, 1), new DateTime(2024, 1, 1))));

        await _transactionRepository.DidNotReceive()
            .ListByUserAsync(Arg.Any<Guid>(), Arg.Any<Guid?>(), Arg.Any<Guid?>(),
                Arg.Any<TransactionType?>(), Arg.Any<DateTime?>(), Arg.Any<DateTime?>());
    }
}
