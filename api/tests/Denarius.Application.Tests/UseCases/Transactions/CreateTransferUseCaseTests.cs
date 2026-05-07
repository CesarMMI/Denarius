using Denarius.Application.Exceptions.Accounts;
using Denarius.Application.Inputs.Transactions;
using Denarius.Application.UseCases.Transactions;
using Denarius.Domain.Entities;
using Denarius.Domain.Enums;
using Denarius.Domain.Exceptions.Transactions;
using Denarius.Domain.Interfaces.Repositories;
using NSubstitute;

namespace Denarius.Application.Tests.UseCases.Transactions;

public class CreateTransferUseCaseTests
{
    private readonly IAccountRepository _accountRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly CreateTransferUseCase _sut;

    public CreateTransferUseCaseTests()
    {
        _accountRepository = Substitute.For<IAccountRepository>();
        _transactionRepository = Substitute.For<ITransactionRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _sut = new CreateTransferUseCase(_accountRepository, _transactionRepository, _unitOfWork);
    }

    private static Account ActiveAccount(Guid userId, string currency = "BRL") =>
        new(userId, "Test", currency, "#000000");

    // -------------------------------------------------------------------------
    // Execute — happy path
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Execute_WithValidInput_ReturnsLinkedTransactionPair()
    {
        var userId = Guid.NewGuid();
        var source = ActiveAccount(userId);
        var destination = ActiveAccount(userId);
        _accountRepository.GetByIdAsync(source.Id, userId).Returns(source);
        _accountRepository.GetByIdAsync(destination.Id, userId).Returns(destination);

        var result = await _sut.Execute(new CreateTransferInput(userId, source.Id, destination.Id, 200m, "Transferência", DateTime.UtcNow));

        Assert.Equal(result.Incoming.Id, result.Outgoing.TransferPeerId);
        Assert.Equal(result.Outgoing.Id, result.Incoming.TransferPeerId);
    }

    [Fact]
    public async Task Execute_WithValidInput_IncomingIsMarkedAsIncoming()
    {
        var userId = Guid.NewGuid();
        var source = ActiveAccount(userId);
        var destination = ActiveAccount(userId);
        _accountRepository.GetByIdAsync(source.Id, userId).Returns(source);
        _accountRepository.GetByIdAsync(destination.Id, userId).Returns(destination);

        var result = await _sut.Execute(new CreateTransferInput(userId, source.Id, destination.Id, 200m, "Transferência", DateTime.UtcNow));

        Assert.True(result.Incoming.IsIncomingTransfer);
        Assert.False(result.Outgoing.IsIncomingTransfer);
    }

    [Fact]
    public async Task Execute_WithValidInput_DebitsSourceAccount()
    {
        var userId = Guid.NewGuid();
        var source = ActiveAccount(userId);
        var destination = ActiveAccount(userId);
        _accountRepository.GetByIdAsync(source.Id, userId).Returns(source);
        _accountRepository.GetByIdAsync(destination.Id, userId).Returns(destination);

        await _sut.Execute(new CreateTransferInput(userId, source.Id, destination.Id, 200m, "Transferência", DateTime.UtcNow));

        Assert.Equal(-200m, source.Balance);
    }

    [Fact]
    public async Task Execute_WithValidInput_CreditsDestinationAccount()
    {
        var userId = Guid.NewGuid();
        var source = ActiveAccount(userId);
        var destination = ActiveAccount(userId);
        _accountRepository.GetByIdAsync(source.Id, userId).Returns(source);
        _accountRepository.GetByIdAsync(destination.Id, userId).Returns(destination);

        await _sut.Execute(new CreateTransferInput(userId, source.Id, destination.Id, 200m, "Transferência", DateTime.UtcNow));

        Assert.Equal(200m, destination.Balance);
    }

    [Fact]
    public async Task Execute_WithValidInput_CallsAddRangeAndUpdateBothAccountsAndCommit()
    {
        var userId = Guid.NewGuid();
        var source = ActiveAccount(userId);
        var destination = ActiveAccount(userId);
        _accountRepository.GetByIdAsync(source.Id, userId).Returns(source);
        _accountRepository.GetByIdAsync(destination.Id, userId).Returns(destination);

        await _sut.Execute(new CreateTransferInput(userId, source.Id, destination.Id, 200m, "Transferência", DateTime.UtcNow));

        await _transactionRepository.Received(1).AddRangeAsync(Arg.Any<IEnumerable<Transaction>>());
        await _accountRepository.Received(1).UpdateAsync(source);
        await _accountRepository.Received(1).UpdateAsync(destination);
        await _unitOfWork.Received(1).CommitAsync();
    }

    [Fact]
    public async Task Execute_WithValidInput_OutgoingBelongsToSourceAccount()
    {
        var userId = Guid.NewGuid();
        var source = ActiveAccount(userId);
        var destination = ActiveAccount(userId);
        _accountRepository.GetByIdAsync(source.Id, userId).Returns(source);
        _accountRepository.GetByIdAsync(destination.Id, userId).Returns(destination);

        var result = await _sut.Execute(new CreateTransferInput(userId, source.Id, destination.Id, 200m, "Transferência", DateTime.UtcNow));

        Assert.Equal(source.Id, result.Outgoing.AccountId);
        Assert.Equal(destination.Id, result.Incoming.AccountId);
    }

    // -------------------------------------------------------------------------
    // Execute — errors
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Execute_WithNonExistentSourceAccount_ThrowsAccountNotFoundException()
    {
        var userId = Guid.NewGuid();
        var sourceId = Guid.NewGuid();
        _accountRepository.GetByIdAsync(sourceId, userId).Returns((Account?)null);

        await Assert.ThrowsAsync<AccountNotFoundException>(() =>
            _sut.Execute(new CreateTransferInput(userId, sourceId, Guid.NewGuid(), 200m, "Test", DateTime.UtcNow)));
    }

    [Fact]
    public async Task Execute_WithInactiveSourceAccount_ThrowsInactiveAccountException()
    {
        var userId = Guid.NewGuid();
        var source = ActiveAccount(userId);
        source.Deactivate();
        _accountRepository.GetByIdAsync(source.Id, userId).Returns(source);

        await Assert.ThrowsAsync<InactiveAccountException>(() =>
            _sut.Execute(new CreateTransferInput(userId, source.Id, Guid.NewGuid(), 200m, "Test", DateTime.UtcNow)));
    }

    [Fact]
    public async Task Execute_WithNonExistentDestinationAccount_ThrowsAccountNotFoundException()
    {
        var userId = Guid.NewGuid();
        var source = ActiveAccount(userId);
        var destinationId = Guid.NewGuid();
        _accountRepository.GetByIdAsync(source.Id, userId).Returns(source);
        _accountRepository.GetByIdAsync(destinationId, userId).Returns((Account?)null);

        await Assert.ThrowsAsync<AccountNotFoundException>(() =>
            _sut.Execute(new CreateTransferInput(userId, source.Id, destinationId, 200m, "Test", DateTime.UtcNow)));
    }

    [Fact]
    public async Task Execute_WithInactiveDestinationAccount_ThrowsInactiveAccountException()
    {
        var userId = Guid.NewGuid();
        var source = ActiveAccount(userId);
        var destination = ActiveAccount(userId);
        destination.Deactivate();
        _accountRepository.GetByIdAsync(source.Id, userId).Returns(source);
        _accountRepository.GetByIdAsync(destination.Id, userId).Returns(destination);

        await Assert.ThrowsAsync<InactiveAccountException>(() =>
            _sut.Execute(new CreateTransferInput(userId, source.Id, destination.Id, 200m, "Test", DateTime.UtcNow)));
    }

    [Fact]
    public async Task Execute_WithSameSourceAndDestination_ThrowsInvalidTransferException()
    {
        var userId = Guid.NewGuid();
        var account = ActiveAccount(userId);
        _accountRepository.GetByIdAsync(account.Id, userId).Returns(account);

        await Assert.ThrowsAsync<InvalidTransferException>(() =>
            _sut.Execute(new CreateTransferInput(userId, account.Id, account.Id, 200m, "Test", DateTime.UtcNow)));
    }

    [Fact]
    public async Task Execute_WithDifferentCurrencies_ThrowsInvalidTransferException()
    {
        var userId = Guid.NewGuid();
        var source = ActiveAccount(userId, "BRL");
        var destination = ActiveAccount(userId, "USD");
        _accountRepository.GetByIdAsync(source.Id, userId).Returns(source);
        _accountRepository.GetByIdAsync(destination.Id, userId).Returns(destination);

        await Assert.ThrowsAsync<InvalidTransferException>(() =>
            _sut.Execute(new CreateTransferInput(userId, source.Id, destination.Id, 200m, "Test", DateTime.UtcNow)));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Execute_WithInvalidAmount_ThrowsInvalidAmountException(decimal amount)
    {
        var userId = Guid.NewGuid();
        var source = ActiveAccount(userId);
        var destination = ActiveAccount(userId);
        _accountRepository.GetByIdAsync(source.Id, userId).Returns(source);
        _accountRepository.GetByIdAsync(destination.Id, userId).Returns(destination);

        await Assert.ThrowsAsync<InvalidAmountException>(() =>
            _sut.Execute(new CreateTransferInput(userId, source.Id, destination.Id, amount, "Test", DateTime.UtcNow)));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task Execute_WithInvalidDescription_ThrowsInvalidDescriptionException(string? description)
    {
        var userId = Guid.NewGuid();
        var source = ActiveAccount(userId);
        var destination = ActiveAccount(userId);
        _accountRepository.GetByIdAsync(source.Id, userId).Returns(source);
        _accountRepository.GetByIdAsync(destination.Id, userId).Returns(destination);

        await Assert.ThrowsAsync<InvalidDescriptionException>(() =>
            _sut.Execute(new CreateTransferInput(userId, source.Id, destination.Id, 200m, description!, DateTime.UtcNow)));
    }
}
