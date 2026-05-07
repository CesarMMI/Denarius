using Denarius.Application.Exceptions.Accounts;
using Denarius.Application.Inputs.Accounts;
using Denarius.Application.UseCases.Accounts;
using Denarius.Domain.Entities;
using Denarius.Domain.Interfaces.Repositories;
using NSubstitute;

namespace Denarius.Application.Tests.UseCases.Accounts;

public class GetAccountByIdUseCaseTests
{
    private readonly IAccountRepository _accountRepository;
    private readonly GetAccountByIdUseCase _sut;

    public GetAccountByIdUseCaseTests()
    {
        _accountRepository = Substitute.For<IAccountRepository>();
        _sut = new GetAccountByIdUseCase(_accountRepository);
    }

    // -------------------------------------------------------------------------
    // Execute — happy path
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Execute_WithExistingAccount_ReturnsAccount()
    {
        var userId = Guid.NewGuid();
        var account = new Account(userId, "Nubank", "BRL", "#8A05BE");
        _accountRepository.GetByIdAsync(account.Id, userId).Returns(account);

        var result = await _sut.Execute(new GetAccountByIdInput(userId, account.Id));

        Assert.Equal(account.Id, result.Id);
        Assert.Equal(account.Name, result.Name);
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
            _sut.Execute(new GetAccountByIdInput(userId, accountId)));
    }

    [Fact]
    public async Task Execute_WithAccountBelongingToAnotherUser_ThrowsAccountNotFoundException()
    {
        var requestingUserId = Guid.NewGuid();
        var accountId = Guid.NewGuid();
        _accountRepository.GetByIdAsync(accountId, requestingUserId).Returns((Account?)null);

        await Assert.ThrowsAsync<AccountNotFoundException>(() =>
            _sut.Execute(new GetAccountByIdInput(requestingUserId, accountId)));
    }
}
