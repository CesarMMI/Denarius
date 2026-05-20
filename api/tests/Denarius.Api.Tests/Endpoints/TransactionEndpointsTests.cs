using System.Net;
using System.Net.Http.Json;
using Denarius.Api.Requests.Transactions;
using Denarius.Api.Tests.Shared;
using Denarius.Application.Inputs.Transactions;
using Denarius.Application.Interfaces.UseCases.Transactions;
using Denarius.Application.Outputs.Transactions;
using Denarius.Domain.Enums;
using Microsoft.AspNetCore.Mvc.Testing;
using NSubstitute;

namespace Denarius.Api.Tests.Endpoints;

public class TransactionEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly IListTransactionsUseCase _listTransactions = Substitute.For<IListTransactionsUseCase>();
    private readonly IGetTransactionByIdUseCase _getTransactionById = Substitute.For<IGetTransactionByIdUseCase>();
    private readonly ICreateTransactionUseCase _createTransaction = Substitute.For<ICreateTransactionUseCase>();
    private readonly ICreateTransferUseCase _createTransfer = Substitute.For<ICreateTransferUseCase>();
    private readonly IUpdateTransactionUseCase _updateTransaction = Substitute.For<IUpdateTransactionUseCase>();
    private readonly IDeleteTransactionUseCase _deleteTransaction = Substitute.For<IDeleteTransactionUseCase>();
    private readonly HttpClient _client;

    public TransactionEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateTestClient(services =>
        {
            services.AddScoped<IListTransactionsUseCase>(_ => _listTransactions);
            services.AddScoped<IGetTransactionByIdUseCase>(_ => _getTransactionById);
            services.AddScoped<ICreateTransactionUseCase>(_ => _createTransaction);
            services.AddScoped<ICreateTransferUseCase>(_ => _createTransfer);
            services.AddScoped<IUpdateTransactionUseCase>(_ => _updateTransaction);
            services.AddScoped<IDeleteTransactionUseCase>(_ => _deleteTransaction);
        });
    }

    private static TransactionOutput SampleTransaction(Guid? id = null) => new(
        id ?? Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null,
        TransactionType.Expense, 100m, "Test", DateTime.UtcNow, false,
        DateTime.UtcNow, DateTime.UtcNow);

    [Fact]
    public async Task ListTransactions_ReturnsOk()
    {
        _listTransactions
            .Execute(Arg.Any<ListTransactionsInput>())
            .Returns(Task.FromResult<IEnumerable<TransactionOutput>>([SampleTransaction()]));

        var response = await _client.GetAsync("/api/transactions");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ListTransactions_CallsUseCaseWithCurrentUserId()
    {
        _listTransactions
            .Execute(Arg.Any<ListTransactionsInput>())
            .Returns(Task.FromResult<IEnumerable<TransactionOutput>>([]));

        await _client.GetAsync("/api/transactions");

        await _listTransactions
            .Received(1)
            .Execute(Arg.Is<ListTransactionsInput>(i => i.UserId == TestAuthHandler.UserId));
    }

    [Fact]
    public async Task GetTransactionById_ReturnsOk()
    {
        var txId = Guid.NewGuid();
        _getTransactionById
            .Execute(Arg.Any<GetTransactionByIdInput>())
            .Returns(Task.FromResult(SampleTransaction(txId)));

        var response = await _client.GetAsync($"/api/transactions/{txId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CreateTransaction_Returns201Created()
    {
        var txId = Guid.NewGuid();
        _createTransaction
            .Execute(Arg.Any<CreateTransactionInput>())
            .Returns(Task.FromResult(SampleTransaction(txId)));

        var response = await _client.PostAsJsonAsync("/api/transactions", new CreateTransactionRequest(
            Guid.NewGuid(), Guid.NewGuid(), TransactionType.Expense, 100m, "Groceries", DateTime.UtcNow));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal($"/api/transactions/{txId}", response.Headers.Location?.ToString());
    }

    [Fact]
    public async Task CreateTransfer_Returns201Created()
    {
        var outgoingId = Guid.NewGuid();
        _createTransfer
            .Execute(Arg.Any<CreateTransferInput>())
            .Returns(Task.FromResult(new CreateTransferOutput(SampleTransaction(outgoingId), SampleTransaction())));

        var response = await _client.PostAsJsonAsync("/api/transactions/transfers", new CreateTransferRequest(
            Guid.NewGuid(), Guid.NewGuid(), 100m, "Transfer", DateTime.UtcNow));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal($"/api/transactions/{outgoingId}", response.Headers.Location?.ToString());
    }

    [Fact]
    public async Task UpdateTransaction_ReturnsOk()
    {
        var txId = Guid.NewGuid();
        _updateTransaction
            .Execute(Arg.Any<UpdateTransactionInput>())
            .Returns(Task.FromResult(SampleTransaction(txId)));

        var response = await _client.PutAsJsonAsync($"/api/transactions/{txId}",
            new UpdateTransactionRequest(150m, "Updated", null));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task DeleteTransaction_Returns204NoContent()
    {
        _deleteTransaction
            .Execute(Arg.Any<DeleteTransactionInput>())
            .Returns(Task.CompletedTask);

        var response = await _client.DeleteAsync($"/api/transactions/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
}
