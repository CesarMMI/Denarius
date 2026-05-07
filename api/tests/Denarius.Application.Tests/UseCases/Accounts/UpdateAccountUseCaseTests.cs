using Denarius.Application.Exceptions.Accounts;
using Denarius.Application.Inputs.Accounts;
using Denarius.Application.UseCases.Accounts;
using Denarius.Domain.Entities;
using Denarius.Domain.Exceptions;
using Denarius.Domain.Interfaces.Repositories;
using NSubstitute;

namespace Denarius.Application.Tests.UseCases.Accounts;

public class UpdateAccountUseCaseTests
{
    private readonly IAccountRepository _accountRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly UpdateAccountUseCase _sut;

    public UpdateAccountUseCaseTests()
    {
        _accountRepository = Substitute.For<IAccountRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _sut = new UpdateAccountUseCase(_accountRepository, _unitOfWork);
    }

    // -------------------------------------------------------------------------
    // Execute — happy path
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Execute_WithValidInput_ReturnsUpdatedAccount()
    {
        var userId = Guid.NewGuid();
        var account = new Account(userId, "Nubank", "BRL", "#8A05BE");
        _accountRepository.GetByIdAsync(account.Id, userId).Returns(account);

        var result = await _sut.Execute(new UpdateAccountInput(userId, account.Id, "Nubank Premium", "#FF0000"));

        Assert.Equal("Nubank Premium", result.Name);
        Assert.Equal("#FF0000", result.Color);
    }

    [Fact]
    public async Task Execute_WithValidInput_DoesNotChangeCurrencyCode()
    {
        var userId = Guid.NewGuid();
        var account = new Account(userId, "Nubank", "BRL", "#8A05BE");
        _accountRepository.GetByIdAsync(account.Id, userId).Returns(account);

        var result = await _sut.Execute(new UpdateAccountInput(userId, account.Id, "Nubank Premium", "#FF0000"));

        Assert.Equal("BRL", result.CurrencyCode);
    }

    [Fact]
    public async Task Execute_WithValidInput_CallsUpdateAsyncAndCommit()
    {
        var userId = Guid.NewGuid();
        var account = new Account(userId, "Nubank", "BRL", "#8A05BE");
        _accountRepository.GetByIdAsync(account.Id, userId).Returns(account);

        await _sut.Execute(new UpdateAccountInput(userId, account.Id, "Nubank Premium", "#FF0000"));

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
            _sut.Execute(new UpdateAccountInput(userId, accountId, "Nubank Premium", "#FF0000")));
    }

    [Fact]
    public async Task Execute_WithAccountBelongingToAnotherUser_ThrowsAccountNotFoundException()
    {
        var requestingUserId = Guid.NewGuid();
        var accountId = Guid.NewGuid();
        _accountRepository.GetByIdAsync(accountId, requestingUserId).Returns((Account?)null);

        await Assert.ThrowsAsync<AccountNotFoundException>(() =>
            _sut.Execute(new UpdateAccountInput(requestingUserId, accountId, "Nubank Premium", "#FF0000")));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task Execute_WithInvalidName_Throws(string? name)
    {
        var userId = Guid.NewGuid();
        var account = new Account(userId, "Nubank", "BRL", "#8A05BE");
        _accountRepository.GetByIdAsync(account.Id, userId).Returns(account);

        await Assert.ThrowsAsync<InvalidNameException>(() =>
            _sut.Execute(new UpdateAccountInput(userId, account.Id, name!, "#FF0000")));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task Execute_WithInvalidColor_Throws(string? color)
    {
        var userId = Guid.NewGuid();
        var account = new Account(userId, "Nubank", "BRL", "#8A05BE");
        _accountRepository.GetByIdAsync(account.Id, userId).Returns(account);

        await Assert.ThrowsAsync<InvalidColorException>(() =>
            _sut.Execute(new UpdateAccountInput(userId, account.Id, "Nubank Premium", color!)));
    }
}
