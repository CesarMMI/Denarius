using Denarius.Application.Exceptions.Accounts;
using Denarius.Application.Exceptions.Categories;
using Denarius.Application.Inputs.Transactions;
using Denarius.Application.UseCases.Transactions;
using Denarius.Domain.Entities;
using Denarius.Domain.Enums;
using Denarius.Domain.Exceptions.Transactions;
using Denarius.Domain.Interfaces.Repositories;
using NSubstitute;

namespace Denarius.Application.Tests.UseCases.Transactions;

public class CreateTransactionUseCaseTests
{
    private readonly IAccountRepository _accountRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly CreateTransactionUseCase _sut;

    public CreateTransactionUseCaseTests()
    {
        _accountRepository = Substitute.For<IAccountRepository>();
        _categoryRepository = Substitute.For<ICategoryRepository>();
        _transactionRepository = Substitute.For<ITransactionRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _sut = new CreateTransactionUseCase(_accountRepository, _categoryRepository, _transactionRepository, _unitOfWork);
    }

    private static Account ActiveAccount(Guid userId) => new(userId, "Nubank", "BRL", "#8A05BE");

    private static Category IncomeCategory(Guid userId) => new(userId, "Salário", "#00FF00", CategoryType.Income);

    private static Category ExpenseCategory(Guid userId) => new(userId, "Alimentação", "#FF0000", CategoryType.Expense);

    // -------------------------------------------------------------------------
    // Execute — happy path
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Execute_WithValidIncomeInput_ReturnsCreatedTransaction()
    {
        var userId = Guid.NewGuid();
        var account = ActiveAccount(userId);
        var category = IncomeCategory(userId);
        _accountRepository.GetByIdAsync(account.Id, userId).Returns(account);
        _categoryRepository.GetByIdAsync(category.Id, userId).Returns(category);

        var input = new CreateTransactionInput(userId, account.Id, category.Id, TransactionType.Income, 500m, "Salário", DateTime.UtcNow);
        var result = await _sut.Execute(input);

        Assert.Equal(account.Id, result.AccountId);
        Assert.Equal(category.Id, result.CategoryId);
        Assert.Equal(TransactionType.Income, result.Type);
        Assert.Equal(500m, result.Amount);
        Assert.NotEqual(Guid.Empty, result.Id);
    }

    [Fact]
    public async Task Execute_WithIncome_CreditsAccount()
    {
        var userId = Guid.NewGuid();
        var account = ActiveAccount(userId);
        var category = IncomeCategory(userId);
        _accountRepository.GetByIdAsync(account.Id, userId).Returns(account);
        _categoryRepository.GetByIdAsync(category.Id, userId).Returns(category);

        await _sut.Execute(new CreateTransactionInput(userId, account.Id, category.Id, TransactionType.Income, 500m, "Salário", DateTime.UtcNow));

        Assert.Equal(500m, account.Balance);
    }

    [Fact]
    public async Task Execute_WithExpense_DebitsAccount()
    {
        var userId = Guid.NewGuid();
        var account = ActiveAccount(userId);
        var category = ExpenseCategory(userId);
        _accountRepository.GetByIdAsync(account.Id, userId).Returns(account);
        _categoryRepository.GetByIdAsync(category.Id, userId).Returns(category);

        await _sut.Execute(new CreateTransactionInput(userId, account.Id, category.Id, TransactionType.Expense, 300m, "Almoço", DateTime.UtcNow));

        Assert.Equal(-300m, account.Balance);
    }

    [Fact]
    public async Task Execute_WithValidInput_CallsAddAsyncAndUpdateAsyncAndCommit()
    {
        var userId = Guid.NewGuid();
        var account = ActiveAccount(userId);
        var category = IncomeCategory(userId);
        _accountRepository.GetByIdAsync(account.Id, userId).Returns(account);
        _categoryRepository.GetByIdAsync(category.Id, userId).Returns(category);

        await _sut.Execute(new CreateTransactionInput(userId, account.Id, category.Id, TransactionType.Income, 500m, "Salário", DateTime.UtcNow));

        await _transactionRepository.Received(1).AddAsync(Arg.Any<Transaction>());
        await _accountRepository.Received(1).UpdateAsync(account);
        await _unitOfWork.Received(1).CommitAsync();
    }

    // -------------------------------------------------------------------------
    // Execute — errors
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Execute_WithNonExistentAccount_ThrowsAccountNotFoundException()
    {
        var userId = Guid.NewGuid();
        var accountId = Guid.NewGuid();
        _accountRepository.GetByIdAsync(accountId, userId).Returns((Account?)null);

        await Assert.ThrowsAsync<AccountNotFoundException>(() =>
            _sut.Execute(new CreateTransactionInput(userId, accountId, Guid.NewGuid(), TransactionType.Income, 500m, "Test", DateTime.UtcNow)));
    }

    [Fact]
    public async Task Execute_WithInactiveAccount_ThrowsInactiveAccountException()
    {
        var userId = Guid.NewGuid();
        var account = ActiveAccount(userId);
        account.Deactivate();
        _accountRepository.GetByIdAsync(account.Id, userId).Returns(account);

        await Assert.ThrowsAsync<InactiveAccountException>(() =>
            _sut.Execute(new CreateTransactionInput(userId, account.Id, Guid.NewGuid(), TransactionType.Income, 500m, "Test", DateTime.UtcNow)));
    }

    [Fact]
    public async Task Execute_WithNonExistentCategory_ThrowsCategoryNotFoundException()
    {
        var userId = Guid.NewGuid();
        var account = ActiveAccount(userId);
        var categoryId = Guid.NewGuid();
        _accountRepository.GetByIdAsync(account.Id, userId).Returns(account);
        _categoryRepository.GetByIdAsync(categoryId, userId).Returns((Category?)null);

        await Assert.ThrowsAsync<CategoryNotFoundException>(() =>
            _sut.Execute(new CreateTransactionInput(userId, account.Id, categoryId, TransactionType.Income, 500m, "Test", DateTime.UtcNow)));
    }

    [Fact]
    public async Task Execute_WithIncompatibleCategory_ThrowsInvalidCategoryException()
    {
        var userId = Guid.NewGuid();
        var account = ActiveAccount(userId);
        var expenseCategory = ExpenseCategory(userId);
        _accountRepository.GetByIdAsync(account.Id, userId).Returns(account);
        _categoryRepository.GetByIdAsync(expenseCategory.Id, userId).Returns(expenseCategory);

        await Assert.ThrowsAsync<InvalidCategoryException>(() =>
            _sut.Execute(new CreateTransactionInput(userId, account.Id, expenseCategory.Id, TransactionType.Income, 500m, "Test", DateTime.UtcNow)));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Execute_WithInvalidAmount_ThrowsInvalidAmountException(decimal amount)
    {
        var userId = Guid.NewGuid();
        var account = ActiveAccount(userId);
        var category = IncomeCategory(userId);
        _accountRepository.GetByIdAsync(account.Id, userId).Returns(account);
        _categoryRepository.GetByIdAsync(category.Id, userId).Returns(category);

        await Assert.ThrowsAsync<InvalidAmountException>(() =>
            _sut.Execute(new CreateTransactionInput(userId, account.Id, category.Id, TransactionType.Income, amount, "Test", DateTime.UtcNow)));
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
        _accountRepository.GetByIdAsync(account.Id, userId).Returns(account);
        _categoryRepository.GetByIdAsync(category.Id, userId).Returns(category);

        await Assert.ThrowsAsync<InvalidDescriptionException>(() =>
            _sut.Execute(new CreateTransactionInput(userId, account.Id, category.Id, TransactionType.Income, 500m, description!, DateTime.UtcNow)));
    }

    [Fact]
    public async Task Execute_WhenAccountNotFound_DoesNotAddTransaction()
    {
        var userId = Guid.NewGuid();
        var accountId = Guid.NewGuid();
        _accountRepository.GetByIdAsync(accountId, userId).Returns((Account?)null);

        await Assert.ThrowsAsync<AccountNotFoundException>(() =>
            _sut.Execute(new CreateTransactionInput(userId, accountId, Guid.NewGuid(), TransactionType.Income, 500m, "Test", DateTime.UtcNow)));

        await _transactionRepository.DidNotReceive().AddAsync(Arg.Any<Transaction>());
    }
}
