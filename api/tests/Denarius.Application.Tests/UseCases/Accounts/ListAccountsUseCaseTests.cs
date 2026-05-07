using Denarius.Application.Inputs.Accounts;
using Denarius.Application.UseCases.Accounts;
using Denarius.Domain.Entities;
using Denarius.Domain.Interfaces.Repositories;
using NSubstitute;

namespace Denarius.Application.Tests.UseCases.Accounts;

public class ListAccountsUseCaseTests
{
    private readonly IAccountRepository _accountRepository;
    private readonly ListAccountsUseCase _sut;

    public ListAccountsUseCaseTests()
    {
        _accountRepository = Substitute.For<IAccountRepository>();
        _sut = new ListAccountsUseCase(_accountRepository);
    }

    // -------------------------------------------------------------------------
    // Execute — happy path
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Execute_WithMultipleAccounts_ReturnsAllAccounts()
    {
        var userId = Guid.NewGuid();
        var accounts = new List<Account>
        {
            new(userId, "Nubank", "BRL", "#8A05BE"),
            new(userId, "Itaú", "BRL", "#FF6700")
        };
        _accountRepository.ListByUserAsync(userId).Returns(accounts);

        var result = await _sut.Execute(new ListAccountsInput(userId));

        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task Execute_IncludesInactiveAccounts()
    {
        var userId = Guid.NewGuid();
        var activeAccount = new Account(userId, "Nubank", "BRL", "#8A05BE");
        var inactiveAccount = new Account(userId, "Itaú", "BRL", "#FF6700");
        inactiveAccount.Deactivate();
        _accountRepository.ListByUserAsync(userId).Returns(new[] { activeAccount, inactiveAccount });

        var result = await _sut.Execute(new ListAccountsInput(userId));

        Assert.Equal(2, result.Count());
        Assert.Contains(result, a => !a.IsActive);
    }

    [Fact]
    public async Task Execute_WithNoAccounts_ReturnsEmptyList()
    {
        var userId = Guid.NewGuid();
        _accountRepository.ListByUserAsync(userId).Returns(Enumerable.Empty<Account>());

        var result = await _sut.Execute(new ListAccountsInput(userId));

        Assert.Empty(result);
    }
}
