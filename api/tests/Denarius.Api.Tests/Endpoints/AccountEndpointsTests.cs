using System.Net;
using System.Net.Http.Json;
using Denarius.Api.Requests.Accounts;
using Denarius.Api.Tests.Shared;
using Denarius.Application.Inputs.Accounts;
using Denarius.Application.Interfaces.UseCases.Accounts;
using Denarius.Application.Outputs.Accounts;
using Microsoft.AspNetCore.Mvc.Testing;
using NSubstitute;

namespace Denarius.Api.Tests.Endpoints;

public class AccountEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly IListAccountsUseCase _listAccounts = Substitute.For<IListAccountsUseCase>();
    private readonly IGetAccountByIdUseCase _getAccountById = Substitute.For<IGetAccountByIdUseCase>();
    private readonly ICreateAccountUseCase _createAccount = Substitute.For<ICreateAccountUseCase>();
    private readonly IUpdateAccountUseCase _updateAccount = Substitute.For<IUpdateAccountUseCase>();
    private readonly IDeactivateAccountUseCase _deactivateAccount = Substitute.For<IDeactivateAccountUseCase>();
    private readonly HttpClient _client;

    public AccountEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateTestClient(services =>
        {
            services.AddScoped<IListAccountsUseCase>(_ => _listAccounts);
            services.AddScoped<IGetAccountByIdUseCase>(_ => _getAccountById);
            services.AddScoped<ICreateAccountUseCase>(_ => _createAccount);
            services.AddScoped<IUpdateAccountUseCase>(_ => _updateAccount);
            services.AddScoped<IDeactivateAccountUseCase>(_ => _deactivateAccount);
        });
    }

    private static AccountOutput SampleAccount(Guid? id = null) => new(
        id ?? Guid.NewGuid(), "Test Account", "USD", 1000m, "#FF0000", true,
        DateTime.UtcNow, DateTime.UtcNow);

    [Fact]
    public async Task ListAccounts_ReturnsOk()
    {
        _listAccounts
            .Execute(Arg.Any<ListAccountsInput>())
            .Returns(Task.FromResult<IEnumerable<AccountOutput>>([SampleAccount()]));

        var response = await _client.GetAsync("/api/accounts");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ListAccounts_CallsUseCaseWithCurrentUserId()
    {
        _listAccounts
            .Execute(Arg.Any<ListAccountsInput>())
            .Returns(Task.FromResult<IEnumerable<AccountOutput>>([]));

        await _client.GetAsync("/api/accounts");

        await _listAccounts
            .Received(1)
            .Execute(Arg.Is<ListAccountsInput>(i => i.UserId == TestAuthHandler.UserId));
    }

    [Fact]
    public async Task GetAccountById_ReturnsOk()
    {
        var accountId = Guid.NewGuid();
        _getAccountById
            .Execute(Arg.Any<GetAccountByIdInput>())
            .Returns(Task.FromResult(SampleAccount(accountId)));

        var response = await _client.GetAsync($"/api/accounts/{accountId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetAccountById_CallsUseCaseWithCorrectIds()
    {
        var accountId = Guid.NewGuid();
        _getAccountById
            .Execute(Arg.Any<GetAccountByIdInput>())
            .Returns(Task.FromResult(SampleAccount(accountId)));

        await _client.GetAsync($"/api/accounts/{accountId}");

        await _getAccountById
            .Received(1)
            .Execute(Arg.Is<GetAccountByIdInput>(i =>
                i.UserId == TestAuthHandler.UserId && i.AccountId == accountId));
    }

    [Fact]
    public async Task CreateAccount_Returns201Created()
    {
        var accountId = Guid.NewGuid();
        _createAccount
            .Execute(Arg.Any<CreateAccountInput>())
            .Returns(Task.FromResult(SampleAccount(accountId)));

        var response = await _client.PostAsJsonAsync("/api/accounts",
            new CreateAccountRequest("Test Account", "USD", "#FF0000"));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal($"/api/accounts/{accountId}", response.Headers.Location?.ToString());
    }

    [Fact]
    public async Task UpdateAccount_ReturnsOk()
    {
        var accountId = Guid.NewGuid();
        _updateAccount
            .Execute(Arg.Any<UpdateAccountInput>())
            .Returns(Task.FromResult(SampleAccount(accountId)));

        var response = await _client.PutAsJsonAsync($"/api/accounts/{accountId}",
            new UpdateAccountRequest("Updated Name", "#00FF00"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task DeactivateAccount_Returns204NoContent()
    {
        _deactivateAccount
            .Execute(Arg.Any<DeactivateAccountInput>())
            .Returns(Task.CompletedTask);

        var response = await _client.DeleteAsync($"/api/accounts/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
}
