using Denarius.Application.Exceptions.Categories;
using Denarius.Application.Exceptions.Transactions;
using Denarius.Application.Inputs.Transactions;
using Denarius.Application.UseCases.Transactions;
using Denarius.Domain.Entities;
using Denarius.Domain.Enums;
using Denarius.Domain.Exceptions.Transactions;
using Denarius.Domain.Interfaces.Repositories;
using NSubstitute;

namespace Denarius.Application.Tests.UseCases.Transactions;

public class UpdateTransactionUseCaseTests
{
    private readonly IAccountRepository _accountRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly UpdateTransactionUseCase _sut;

    public UpdateTransactionUseCaseTests()
    {
        _accountRepository = Substitute.For<IAccountRepository>();
        _categoryRepository = Substitute.For<ICategoryRepository>();
        _transactionRepository = Substitute.For<ITransactionRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _sut = new UpdateTransactionUseCase(_accountRepository, _categoryRepository, _transactionRepository, _unitOfWork);
    }

    private static Account ActiveAccount(Guid userId) => new(userId, "Nubank", "BRL", "#000000");

    private static Category IncomeCategory(Guid userId) => new(userId, "Salário", "#00FF00", CategoryType.Income);

    private static Category ExpenseCategory(Guid userId) => new(userId, "Alimentação", "#FF0000", CategoryType.Expense);

    private static Transaction IncomeTransaction(Guid userId, Guid accountId, Guid categoryId, decimal amount = 100m) =>
        new(userId, accountId, categoryId, null, TransactionType.Income, amount, "Original", DateTime.UtcNow);

    private static Transaction ExpenseTransaction(Guid userId, Guid accountId, Guid categoryId, decimal amount = 100m) =>
        new(userId, accountId, categoryId, null, TransactionType.Expense, amount, "Original", DateTime.UtcNow);

    private static (Transaction Outgoing, Transaction Incoming) TransferPair(
        Guid userId, Guid sourceAccountId, Guid destinationAccountId, decimal amount = 100m)
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
    public async Task Execute_WithValidInput_ReturnsUpdatedTransaction()
    {
        var userId = Guid.NewGuid();
        var account = ActiveAccount(userId);
        var category = IncomeCategory(userId);
        var newCategory = IncomeCategory(userId);
        var transaction = IncomeTransaction(userId, account.Id, category.Id);
        _transactionRepository.GetByIdAsync(transaction.Id, userId).Returns(transaction);
        _accountRepository.GetByIdAsync(account.Id, userId).Returns(account);
        _categoryRepository.GetByIdAsync(newCategory.Id, userId).Returns(newCategory);

        var result = await _sut.Execute(new UpdateTransactionInput(userId, transaction.Id, 150m, "Updated", newCategory.Id));

        Assert.Equal(150m, result.Amount);
        Assert.Equal("Updated", result.Description);
        Assert.Equal(newCategory.Id, result.CategoryId);
    }

    [Fact]
    public async Task Execute_WithValidInput_CallsUpdateAsyncAndCommit()
    {
        var userId = Guid.NewGuid();
        var account = ActiveAccount(userId);
        var category = IncomeCategory(userId);
        var transaction = IncomeTransaction(userId, account.Id, category.Id);
        _transactionRepository.GetByIdAsync(transaction.Id, userId).Returns(transaction);
        _accountRepository.GetByIdAsync(account.Id, userId).Returns(account);
        _categoryRepository.GetByIdAsync(category.Id, userId).Returns(category);

        await _sut.Execute(new UpdateTransactionInput(userId, transaction.Id, 150m, "Updated", category.Id));

        await _transactionRepository.Received(1).UpdateAsync(transaction);
        await _accountRepository.Received(1).UpdateAsync(account);
        await _unitOfWork.Received(1).CommitAsync();
    }

    [Fact]
    public async Task Execute_IncomeAmountChange_CorrectlyAdjustsAccountBalance()
    {
        var userId = Guid.NewGuid();
        var account = ActiveAccount(userId);
        account.ApplyDelta(100m); // simulate income of 100 already applied → balance = 100
        var category = IncomeCategory(userId);
        var transaction = IncomeTransaction(userId, account.Id, category.Id, 100m);
        _transactionRepository.GetByIdAsync(transaction.Id, userId).Returns(transaction);
        _accountRepository.GetByIdAsync(account.Id, userId).Returns(account);
        _categoryRepository.GetByIdAsync(category.Id, userId).Returns(category);

        await _sut.Execute(new UpdateTransactionInput(userId, transaction.Id, 150m, "Updated", category.Id));

        // Income: new delta = +150, old delta = +100, correction = +50 → balance = 100 + 50 = 150
        Assert.Equal(150m, account.Balance);
    }

    [Fact]
    public async Task Execute_ExpenseAmountChange_CorrectlyAdjustsAccountBalance()
    {
        var userId = Guid.NewGuid();
        var account = ActiveAccount(userId);
        account.ApplyDelta(-100m); // simulate expense of 100 already applied → balance = -100
        var category = ExpenseCategory(userId);
        var transaction = ExpenseTransaction(userId, account.Id, category.Id, 100m);
        _transactionRepository.GetByIdAsync(transaction.Id, userId).Returns(transaction);
        _accountRepository.GetByIdAsync(account.Id, userId).Returns(account);
        _categoryRepository.GetByIdAsync(category.Id, userId).Returns(category);

        await _sut.Execute(new UpdateTransactionInput(userId, transaction.Id, 150m, "Updated", category.Id));

        // Expense: new delta = -150, old delta = -100, correction = -50 → balance = -100 + (-50) = -150
        Assert.Equal(-150m, account.Balance);
    }

    [Fact]
    public async Task Execute_WhenAmountUnchanged_DoesNotAdjustAccountBalance()
    {
        var userId = Guid.NewGuid();
        var account = ActiveAccount(userId);
        account.ApplyDelta(100m); // balance = 100
        var category = IncomeCategory(userId);
        var transaction = IncomeTransaction(userId, account.Id, category.Id, 100m);
        _transactionRepository.GetByIdAsync(transaction.Id, userId).Returns(transaction);
        _accountRepository.GetByIdAsync(account.Id, userId).Returns(account);
        _categoryRepository.GetByIdAsync(category.Id, userId).Returns(category);

        await _sut.Execute(new UpdateTransactionInput(userId, transaction.Id, 100m, "Updated", category.Id));

        Assert.Equal(100m, account.Balance);
    }

    // -------------------------------------------------------------------------
    // Execute — happy path (Transfer)
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Execute_TransferAmountChange_AdjustsBothAccountBalances()
    {
        var userId = Guid.NewGuid();
        var sourceAccount = ActiveAccount(userId);
        sourceAccount.ApplyDelta(-100m); // debit of 100 already applied → balance = -100
        var destinationAccount = ActiveAccount(userId);
        destinationAccount.ApplyDelta(100m); // credit of 100 already applied → balance = 100

        var (outgoing, incoming) = TransferPair(userId, sourceAccount.Id, destinationAccount.Id, 100m);

        _transactionRepository.GetByIdAsync(outgoing.Id, userId).Returns(outgoing);
        _transactionRepository.GetByIdAsync(incoming.Id, userId).Returns(incoming);
        _accountRepository.GetByIdAsync(sourceAccount.Id, userId).Returns(sourceAccount);
        _accountRepository.GetByIdAsync(destinationAccount.Id, userId).Returns(destinationAccount);
        _categoryRepository.GetByIdAsync(Arg.Any<Guid>(), userId).Returns((Category?)null);

        await _sut.Execute(new UpdateTransactionInput(userId, outgoing.Id, 150m, "Updated", null));

        // Outgoing (not incoming): correction = -150 - (-100) = -50 → source = -100 + (-50) = -150
        // Incoming: correction = +150 - (+100) = +50 → destination = 100 + 50 = 150
        Assert.Equal(-150m, sourceAccount.Balance);
        Assert.Equal(150m, destinationAccount.Balance);
    }

    [Fact]
    public async Task Execute_TransferAmountChange_UpdatesBothTransactions()
    {
        var userId = Guid.NewGuid();
        var sourceAccount = ActiveAccount(userId);
        sourceAccount.ApplyDelta(-100m);
        var destinationAccount = ActiveAccount(userId);
        destinationAccount.ApplyDelta(100m);

        var (outgoing, incoming) = TransferPair(userId, sourceAccount.Id, destinationAccount.Id, 100m);

        _transactionRepository.GetByIdAsync(outgoing.Id, userId).Returns(outgoing);
        _transactionRepository.GetByIdAsync(incoming.Id, userId).Returns(incoming);
        _accountRepository.GetByIdAsync(sourceAccount.Id, userId).Returns(sourceAccount);
        _accountRepository.GetByIdAsync(destinationAccount.Id, userId).Returns(destinationAccount);

        await _sut.Execute(new UpdateTransactionInput(userId, outgoing.Id, 150m, "Updated", null));

        Assert.Equal(150m, outgoing.Amount);
        Assert.Equal(150m, incoming.Amount);
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
            _sut.Execute(new UpdateTransactionInput(userId, transactionId, 100m, "Test", Guid.NewGuid())));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Execute_WithInvalidAmount_ThrowsInvalidAmountException(decimal amount)
    {
        var userId = Guid.NewGuid();
        var account = ActiveAccount(userId);
        var category = IncomeCategory(userId);
        var transaction = IncomeTransaction(userId, account.Id, category.Id);
        _transactionRepository.GetByIdAsync(transaction.Id, userId).Returns(transaction);
        _accountRepository.GetByIdAsync(account.Id, userId).Returns(account);

        await Assert.ThrowsAsync<InvalidAmountException>(() =>
            _sut.Execute(new UpdateTransactionInput(userId, transaction.Id, amount, "Test", category.Id)));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task Execute_WithInvalidDescription_ThrowsInvalidDescriptionException(string? description)
    {
        var userId = Guid.NewGuid();
        var account = ActiveAccount(userId);
        var category = IncomeCategory(userId);
        var transaction = IncomeTransaction(userId, account.Id, category.Id);
        _transactionRepository.GetByIdAsync(transaction.Id, userId).Returns(transaction);
        _accountRepository.GetByIdAsync(account.Id, userId).Returns(account);

        await Assert.ThrowsAsync<InvalidDescriptionException>(() =>
            _sut.Execute(new UpdateTransactionInput(userId, transaction.Id, 100m, description!, category.Id)));
    }

    [Fact]
    public async Task Execute_WithNullCategoryOnIncomeExpense_ThrowsInvalidCategoryException()
    {
        var userId = Guid.NewGuid();
        var account = ActiveAccount(userId);
        var category = IncomeCategory(userId);
        var transaction = IncomeTransaction(userId, account.Id, category.Id);
        _transactionRepository.GetByIdAsync(transaction.Id, userId).Returns(transaction);
        _accountRepository.GetByIdAsync(account.Id, userId).Returns(account);

        await Assert.ThrowsAsync<InvalidCategoryException>(() =>
            _sut.Execute(new UpdateTransactionInput(userId, transaction.Id, 100m, "Test", null)));
    }

    [Fact]
    public async Task Execute_WithNonExistentCategory_ThrowsCategoryNotFoundException()
    {
        var userId = Guid.NewGuid();
        var account = ActiveAccount(userId);
        var category = IncomeCategory(userId);
        var newCategoryId = Guid.NewGuid();
        var transaction = IncomeTransaction(userId, account.Id, category.Id);
        _transactionRepository.GetByIdAsync(transaction.Id, userId).Returns(transaction);
        _accountRepository.GetByIdAsync(account.Id, userId).Returns(account);
        _categoryRepository.GetByIdAsync(newCategoryId, userId).Returns((Category?)null);

        await Assert.ThrowsAsync<CategoryNotFoundException>(() =>
            _sut.Execute(new UpdateTransactionInput(userId, transaction.Id, 100m, "Test", newCategoryId)));
    }

    [Fact]
    public async Task Execute_WithIncompatibleCategory_ThrowsInvalidCategoryException()
    {
        var userId = Guid.NewGuid();
        var account = ActiveAccount(userId);
        var category = IncomeCategory(userId);
        var expenseCategory = ExpenseCategory(userId);
        var transaction = IncomeTransaction(userId, account.Id, category.Id);
        _transactionRepository.GetByIdAsync(transaction.Id, userId).Returns(transaction);
        _accountRepository.GetByIdAsync(account.Id, userId).Returns(account);
        _categoryRepository.GetByIdAsync(expenseCategory.Id, userId).Returns(expenseCategory);

        await Assert.ThrowsAsync<InvalidCategoryException>(() =>
            _sut.Execute(new UpdateTransactionInput(userId, transaction.Id, 100m, "Test", expenseCategory.Id)));
    }

    [Fact]
    public async Task Execute_SettingCategoryOnTransfer_ThrowsInvalidCategoryException()
    {
        var userId = Guid.NewGuid();
        var sourceAccount = ActiveAccount(userId);
        var destinationAccount = ActiveAccount(userId);
        sourceAccount.ApplyDelta(-100m);
        destinationAccount.ApplyDelta(100m);

        var (outgoing, incoming) = TransferPair(userId, sourceAccount.Id, destinationAccount.Id);

        _transactionRepository.GetByIdAsync(outgoing.Id, userId).Returns(outgoing);
        _transactionRepository.GetByIdAsync(incoming.Id, userId).Returns(incoming);
        _accountRepository.GetByIdAsync(sourceAccount.Id, userId).Returns(sourceAccount);
        _accountRepository.GetByIdAsync(destinationAccount.Id, userId).Returns(destinationAccount);

        await Assert.ThrowsAsync<InvalidCategoryException>(() =>
            _sut.Execute(new UpdateTransactionInput(userId, outgoing.Id, 100m, "Test", Guid.NewGuid())));
    }
}
