using Denarius.Application.Inputs.Accounts;
using Denarius.Application.UseCases.Accounts;
using Denarius.Domain.Entities;
using Denarius.Domain.Exceptions;
using Denarius.Domain.Exceptions.Accounts;
using Denarius.Domain.Interfaces.Repositories;
using NSubstitute;

namespace Denarius.Application.Tests.UseCases.Accounts;

public class CreateAccountUseCaseTests
{
    private readonly IAccountRepository _accountRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly CreateAccountUseCase _sut;

    public CreateAccountUseCaseTests()
    {
        _accountRepository = Substitute.For<IAccountRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _sut = new CreateAccountUseCase(_accountRepository, _unitOfWork);
    }

    // -------------------------------------------------------------------------
    // Execute — happy path
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Execute_WithValidInput_ReturnsCreatedAccount()
    {
        var input = new CreateAccountInput(Guid.NewGuid(), "Nubank", "BRL", "#8A05BE");

        var result = await _sut.Execute(input);

        Assert.Equal("Nubank", result.Name);
        Assert.Equal("BRL", result.CurrencyCode);
        Assert.Equal("#8A05BE", result.Color);
        Assert.NotEqual(Guid.Empty, result.Id);
    }

    [Fact]
    public async Task Execute_WithValidInput_ReturnsAccountWithZeroBalanceAndActive()
    {
        var input = new CreateAccountInput(Guid.NewGuid(), "Nubank", "BRL", "#8A05BE");

        var result = await _sut.Execute(input);

        Assert.Equal(0, result.Balance);
        Assert.True(result.IsActive);
    }

    [Fact]
    public async Task Execute_WithValidInput_CallsAddAsyncAndCommit()
    {
        var input = new CreateAccountInput(Guid.NewGuid(), "Nubank", "BRL", "#8A05BE");

        await _sut.Execute(input);

        await _accountRepository.Received(1).AddAsync(Arg.Any<Account>());
        await _unitOfWork.Received(1).CommitAsync();
    }

    // -------------------------------------------------------------------------
    // Execute — errors
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task Execute_WithInvalidName_Throws(string? name)
    {
        var input = new CreateAccountInput(Guid.NewGuid(), name!, "BRL", "#8A05BE");

        await Assert.ThrowsAsync<InvalidNameException>(() => _sut.Execute(input));
    }

    [Theory]
    [InlineData("BR")]
    [InlineData("BRLL")]
    [InlineData("brl")]
    [InlineData("Brl")]
    [InlineData("BR1")]
    public async Task Execute_WithInvalidCurrencyCode_Throws(string code)
    {
        var input = new CreateAccountInput(Guid.NewGuid(), "Nubank", code, "#8A05BE");

        await Assert.ThrowsAsync<InvalidCurrencyCodeException>(() => _sut.Execute(input));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task Execute_WithInvalidColor_Throws(string? color)
    {
        var input = new CreateAccountInput(Guid.NewGuid(), "Nubank", "BRL", color!);

        await Assert.ThrowsAsync<InvalidColorException>(() => _sut.Execute(input));
    }

    [Fact]
    public async Task Execute_WithInvalidInput_DoesNotCallAddAsync()
    {
        var input = new CreateAccountInput(Guid.NewGuid(), "", "BRL", "#8A05BE");

        await Assert.ThrowsAsync<InvalidNameException>(() => _sut.Execute(input));

        await _accountRepository.DidNotReceive().AddAsync(Arg.Any<Account>());
    }
}
