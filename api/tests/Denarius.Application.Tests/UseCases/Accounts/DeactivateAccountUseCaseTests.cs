using Denarius.Application.Exceptions.Accounts;
using Denarius.Application.Inputs.Accounts;
using Denarius.Application.UseCases.Accounts;
using Denarius.Domain.Entities;
using Denarius.Domain.Interfaces.Repositories;
using NSubstitute;

namespace Denarius.Application.Tests.UseCases.Accounts;

public class DeactivateAccountUseCaseTests
{
    private readonly IAccountRepository _accountRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly DeactivateAccountUseCase _sut;

    public DeactivateAccountUseCaseTests()
    {
        _accountRepository = Substitute.For<IAccountRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _sut = new DeactivateAccountUseCase(_accountRepository, _unitOfWork);
    }

    // -------------------------------------------------------------------------
    // Execute — happy path
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Execute_WithActiveAccount_DeactivatesAccount()
    {
        var userId = Guid.NewGuid();
        var account = new Account(userId, "Nubank", "BRL", "#8A05BE");
        _accountRepository.GetByIdAsync(account.Id, userId).Returns(account);

        await _sut.Execute(new DeactivateAccountInput(userId, account.Id));

        Assert.False(account.IsActive);
    }

    [Fact]
    public async Task Execute_WithActiveAccount_CallsUpdateAsyncAndCommit()
    {
        var userId = Guid.NewGuid();
        var account = new Account(userId, "Nubank", "BRL", "#8A05BE");
        _accountRepository.GetByIdAsync(account.Id, userId).Returns(account);

        await _sut.Execute(new DeactivateAccountInput(userId, account.Id));

        await _accountRepository.Received(1).UpdateAsync(account);
        await _unitOfWork.Received(1).CommitAsync();
    }

    [Fact]
    public async Task Execute_WithAlreadyInactiveAccount_IsIdempotent()
    {
        var userId = Guid.NewGuid();
        var account = new Account(userId, "Nubank", "BRL", "#8A05BE");
        account.Deactivate();
        _accountRepository.GetByIdAsync(account.Id, userId).Returns(account);

        var exception = await Record.ExceptionAsync(() =>
            _sut.Execute(new DeactivateAccountInput(userId, account.Id)));

        Assert.Null(exception);
        Assert.False(account.IsActive);
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
            _sut.Execute(new DeactivateAccountInput(userId, accountId)));
    }

    [Fact]
    public async Task Execute_WithAccountBelongingToAnotherUser_ThrowsAccountNotFoundException()
    {
        var requestingUserId = Guid.NewGuid();
        var accountId = Guid.NewGuid();
        _accountRepository.GetByIdAsync(accountId, requestingUserId).Returns((Account?)null);

        await Assert.ThrowsAsync<AccountNotFoundException>(() =>
            _sut.Execute(new DeactivateAccountInput(requestingUserId, accountId)));
    }

    [Fact]
    public async Task Execute_WithNonExistentAccount_DoesNotCallUpdateAsync()
    {
        var userId = Guid.NewGuid();
        var accountId = Guid.NewGuid();
        _accountRepository.GetByIdAsync(accountId, userId).Returns((Account?)null);

        await Assert.ThrowsAsync<AccountNotFoundException>(() =>
            _sut.Execute(new DeactivateAccountInput(userId, accountId)));

        await _accountRepository.DidNotReceive().UpdateAsync(Arg.Any<Account>());
    }
}
