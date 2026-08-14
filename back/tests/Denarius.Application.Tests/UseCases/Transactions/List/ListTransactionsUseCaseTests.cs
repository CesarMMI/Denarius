using Denarius.Application.UseCases.Transactions.List;
using Denarius.Domain.Entities;
using Denarius.Domain.Repositories;
using NSubstitute;

namespace Denarius.Application.Tests.UseCases.Transactions.List;

public class ListTransactionsUseCaseTests
{
    private readonly ITransactionRepository _transactionRepository = Substitute.For<ITransactionRepository>();
    private readonly IListTransactionsUseCase _useCase;

    public ListTransactionsUseCaseTests()
    {
        _useCase = new ListTransactionsUseCase(_transactionRepository);
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

        var output = await _useCase.Execute(null);

        Assert.Equal(2, output.Count());
        Assert.Contains(output, o => o.Description == "Compras" && o.Value == 150.75m);
        Assert.Contains(output, o => o.Description == "Farmácia" && o.Value == 42.5m);
        await _transactionRepository.Received(1).GetAllAsync();
    }

    [Fact]
    public async Task Execute_NoTransactions_ReturnsEmpty()
    {
        _transactionRepository.GetAllAsync().Returns([]);

        var output = await _useCase.Execute(null);

        Assert.Empty(output);
    }
}
