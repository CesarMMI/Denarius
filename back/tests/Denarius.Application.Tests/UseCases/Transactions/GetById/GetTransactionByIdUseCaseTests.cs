using Denarius.Application.Exceptions;
using Denarius.Application.UseCases.Transactions.GetById;
using Denarius.Domain.Entities;
using Denarius.Domain.Repositories;
using NSubstitute;

namespace Denarius.Application.Tests.UseCases.Transactions.GetById;

public class GetTransactionByIdUseCaseTests
{
    private readonly ITransactionRepository _transactionRepository = Substitute.For<ITransactionRepository>();
    private readonly IGetTransactionByIdUseCase _useCase;

    public GetTransactionByIdUseCaseTests()
    {
        _useCase = new GetTransactionByIdUseCase(_transactionRepository);
    }

    [Fact]
    public async Task Execute_ExistingTransaction_ReturnsOutput()
    {
        var transaction = new Transaction("Compras", DateTime.UtcNow, 150.75m, Guid.NewGuid());
        _transactionRepository.GetByIdAsync(transaction.Id).Returns(transaction);

        var output = await _useCase.Execute(transaction.Id);

        Assert.Equal(transaction.Id, output.Id);
        Assert.Equal("Compras", output.Description);
        Assert.Equal(150.75m, output.Value);
    }

    [Fact]
    public async Task Execute_TransactionNotFound_ThrowsNotFoundException()
    {
        var id = Guid.NewGuid();
        _transactionRepository.GetByIdAsync(id).Returns((Transaction?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _useCase.Execute(id));
    }
}
