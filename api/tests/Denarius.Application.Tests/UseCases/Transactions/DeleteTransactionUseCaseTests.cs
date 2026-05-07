using Denarius.Application.Exceptions.Transactions;
using Denarius.Application.Inputs.Transactions;
using Denarius.Application.UseCases.Transactions;
using Denarius.Domain.Entities;
using Denarius.Domain.Enums;
using Denarius.Domain.Interfaces.Repositories;
using NSubstitute;

namespace Denarius.Application.Tests.UseCases.Transactions;

public class DeleteTransactionUseCaseTests
{
    private readonly IAccountRepository _accountRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly DeleteTransactionUseCase _sut;

    public DeleteTransactionUseCaseTests()
    {
        _accountRepository = Substitute.For<IAccountRepository>();
        _transactionRepository = Substitute.For<ITransactionRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _sut = new DeleteTransactionUseCase(_accountRepository, _transactionRepository, _unitOfWork);
    }

    private static Account ActiveAccount(Guid userId) => new(userId, "Test", "BRL", "#000000");

    private static (Transaction Outgoing, Transaction Incoming) TransferPair(
        Guid userId, Guid sourceAccountId, Guid destinationAccountId, decimal amount = 200m)
    {
        var incoming = new Transaction(userId, destinationAccountId, null, Guid.NewGuid(), TransactionType.Transfer, amount, "Transfer", DateTime.UtcNow);
        incoming.MarkAsIncoming();
        var outgoing = new Transaction(userId, sourceAccountId, null, incoming.Id, TransactionType.Transfer, amount, "Transfer", DateTime.UtcNow);
        incoming.LinkTransferPeer(outgoing.Id);
        return (outgoing, incoming);
    }

    // -------------------------------------------------------------------------
    // Execute — happy path (Income / Expense)
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Execute_Income_RevertsAccountBalance()
    {
        var userId = Guid.NewGuid();
        var account = ActiveAccount(userId);
        account.ApplyDelta(500m); // income was applied → balance = 500
        var categoryId = Guid.NewGuid();
        var transaction = new Transaction(userId, account.Id, categoryId, null, TransactionType.Income, 500m, "Salário", DateTime.UtcNow);
        _transactionRepository.GetByIdAsync(transaction.Id, userId).Returns(transaction);
        _accountRepository.GetByIdAsync(account.Id, userId).Returns(account);

        await _sut.Execute(new DeleteTransactionInput(userId, transaction.Id));

        // RevertDelta for Income = -ToDelta() = -500 → balance = 500 - 500 = 0
        Assert.Equal(0m, account.Balance);
    }

    [Fact]
    public async Task Execute_Expense_RevertsAccountBalance()
    {
        var userId = Guid.NewGuid();
        var account = ActiveAccount(userId);
        account.ApplyDelta(-300m); // expense was applied → balance = -300
        var categoryId = Guid.NewGuid();
        var transaction = new Transaction(userId, account.Id, categoryId, null, TransactionType.Expense, 300m, "Almoço", DateTime.UtcNow);
        _transactionRepository.GetByIdAsync(transaction.Id, userId).Returns(transaction);
        _accountRepository.GetByIdAsync(account.Id, userId).Returns(account);

        await _sut.Execute(new DeleteTransactionInput(userId, transaction.Id));

        // RevertDelta for Expense = -(-300) = +300 → balance = -300 + 300 = 0
        Assert.Equal(0m, account.Balance);
    }

    [Fact]
    public async Task Execute_NonTransfer_CallsDeleteAsyncAndUpdateAccountAndCommit()
    {
        var userId = Guid.NewGuid();
        var account = ActiveAccount(userId);
        account.ApplyDelta(500m);
        var categoryId = Guid.NewGuid();
        var transaction = new Transaction(userId, account.Id, categoryId, null, TransactionType.Income, 500m, "Test", DateTime.UtcNow);
        _transactionRepository.GetByIdAsync(transaction.Id, userId).Returns(transaction);
        _accountRepository.GetByIdAsync(account.Id, userId).Returns(account);

        await _sut.Execute(new DeleteTransactionInput(userId, transaction.Id));

        await _transactionRepository.Received(1).DeleteAsync(transaction);
        await _accountRepository.Received(1).UpdateAsync(account);
        await _unitOfWork.Received(1).CommitAsync();
    }

    [Fact]
    public async Task Execute_NonTransfer_DoesNotCallDeleteRangeAsync()
    {
        var userId = Guid.NewGuid();
        var account = ActiveAccount(userId);
        account.ApplyDelta(500m);
        var categoryId = Guid.NewGuid();
        var transaction = new Transaction(userId, account.Id, categoryId, null, TransactionType.Income, 500m, "Test", DateTime.UtcNow);
        _transactionRepository.GetByIdAsync(transaction.Id, userId).Returns(transaction);
        _accountRepository.GetByIdAsync(account.Id, userId).Returns(account);

        await _sut.Execute(new DeleteTransactionInput(userId, transaction.Id));

        await _transactionRepository.DidNotReceive().DeleteRangeAsync(Arg.Any<IEnumerable<Transaction>>());
    }

    // -------------------------------------------------------------------------
    // Execute — happy path (Transfer)
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Execute_Transfer_RevertsSourceAccountBalance()
    {
        var userId = Guid.NewGuid();
        var sourceAccount = ActiveAccount(userId);
        sourceAccount.ApplyDelta(-200m); // outgoing applied -200 → balance = -200
        var destinationAccount = ActiveAccount(userId);
        destinationAccount.ApplyDelta(200m);

        var (outgoing, incoming) = TransferPair(userId, sourceAccount.Id, destinationAccount.Id, 200m);

        _transactionRepository.GetByIdAsync(outgoing.Id, userId).Returns(outgoing);
        _transactionRepository.GetByIdAsync(incoming.Id, userId).Returns(incoming);
        _accountRepository.GetByIdAsync(sourceAccount.Id, userId).Returns(sourceAccount);
        _accountRepository.GetByIdAsync(destinationAccount.Id, userId).Returns(destinationAccount);

        await _sut.Execute(new DeleteTransactionInput(userId, outgoing.Id));

        // RevertDelta(outgoing) = -(-200) = +200 → source = -200 + 200 = 0
        Assert.Equal(0m, sourceAccount.Balance);
    }

    [Fact]
    public async Task Execute_Transfer_RevertsDestinationAccountBalance()
    {
        var userId = Guid.NewGuid();
        var sourceAccount = ActiveAccount(userId);
        sourceAccount.ApplyDelta(-200m);
        var destinationAccount = ActiveAccount(userId);
        destinationAccount.ApplyDelta(200m); // incoming applied +200 → balance = 200

        var (outgoing, incoming) = TransferPair(userId, sourceAccount.Id, destinationAccount.Id, 200m);

        _transactionRepository.GetByIdAsync(outgoing.Id, userId).Returns(outgoing);
        _transactionRepository.GetByIdAsync(incoming.Id, userId).Returns(incoming);
        _accountRepository.GetByIdAsync(sourceAccount.Id, userId).Returns(sourceAccount);
        _accountRepository.GetByIdAsync(destinationAccount.Id, userId).Returns(destinationAccount);

        await _sut.Execute(new DeleteTransactionInput(userId, outgoing.Id));

        // RevertDelta(incoming, IsIncomingTransfer=true) = -200 → destination = 200 - 200 = 0
        Assert.Equal(0m, destinationAccount.Balance);
    }

    [Fact]
    public async Task Execute_Transfer_DeletesBothTransactionsAndUpdatesAccountsAndCommits()
    {
        var userId = Guid.NewGuid();
        var sourceAccount = ActiveAccount(userId);
        sourceAccount.ApplyDelta(-200m);
        var destinationAccount = ActiveAccount(userId);
        destinationAccount.ApplyDelta(200m);

        var (outgoing, incoming) = TransferPair(userId, sourceAccount.Id, destinationAccount.Id, 200m);

        _transactionRepository.GetByIdAsync(outgoing.Id, userId).Returns(outgoing);
        _transactionRepository.GetByIdAsync(incoming.Id, userId).Returns(incoming);
        _accountRepository.GetByIdAsync(sourceAccount.Id, userId).Returns(sourceAccount);
        _accountRepository.GetByIdAsync(destinationAccount.Id, userId).Returns(destinationAccount);

        await _sut.Execute(new DeleteTransactionInput(userId, outgoing.Id));

        await _transactionRepository.Received(1).DeleteRangeAsync(Arg.Any<IEnumerable<Transaction>>());
        await _accountRepository.Received(1).UpdateAsync(sourceAccount);
        await _accountRepository.Received(1).UpdateAsync(destinationAccount);
        await _unitOfWork.Received(1).CommitAsync();
    }

    [Fact]
    public async Task Execute_Transfer_DoesNotCallDeleteAsync()
    {
        var userId = Guid.NewGuid();
        var sourceAccount = ActiveAccount(userId);
        sourceAccount.ApplyDelta(-200m);
        var destinationAccount = ActiveAccount(userId);
        destinationAccount.ApplyDelta(200m);

        var (outgoing, incoming) = TransferPair(userId, sourceAccount.Id, destinationAccount.Id, 200m);

        _transactionRepository.GetByIdAsync(outgoing.Id, userId).Returns(outgoing);
        _transactionRepository.GetByIdAsync(incoming.Id, userId).Returns(incoming);
        _accountRepository.GetByIdAsync(sourceAccount.Id, userId).Returns(sourceAccount);
        _accountRepository.GetByIdAsync(destinationAccount.Id, userId).Returns(destinationAccount);

        await _sut.Execute(new DeleteTransactionInput(userId, outgoing.Id));

        await _transactionRepository.DidNotReceive().DeleteAsync(Arg.Any<Transaction>());
    }

    // -------------------------------------------------------------------------
    // Execute — errors
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Execute_WithNonExistentTransaction_ThrowsTransactionNotFoundException()
    {
        var userId = Guid.NewGuid();
        var transactionId = Guid.NewGuid();
        _transactionRepository.GetByIdAsync(transactionId, userId).Returns((Transaction?)null);

        await Assert.ThrowsAsync<TransactionNotFoundException>(() =>
            _sut.Execute(new DeleteTransactionInput(userId, transactionId)));
    }

    [Fact]
    public async Task Execute_WithTransactionBelongingToAnotherUser_ThrowsTransactionNotFoundException()
    {
        var requestingUserId = Guid.NewGuid();
        var transactionId = Guid.NewGuid();
        _transactionRepository.GetByIdAsync(transactionId, requestingUserId).Returns((Transaction?)null);

        await Assert.ThrowsAsync<TransactionNotFoundException>(() =>
            _sut.Execute(new DeleteTransactionInput(requestingUserId, transactionId)));
    }

    [Fact]
    public async Task Execute_WhenNotFound_DoesNotCallDeleteAsync()
    {
        var userId = Guid.NewGuid();
        var transactionId = Guid.NewGuid();
        _transactionRepository.GetByIdAsync(transactionId, userId).Returns((Transaction?)null);

        await Assert.ThrowsAsync<TransactionNotFoundException>(() =>
            _sut.Execute(new DeleteTransactionInput(userId, transactionId)));

        await _transactionRepository.DidNotReceive().DeleteAsync(Arg.Any<Transaction>());
        await _transactionRepository.DidNotReceive().DeleteRangeAsync(Arg.Any<IEnumerable<Transaction>>());
    }
}
