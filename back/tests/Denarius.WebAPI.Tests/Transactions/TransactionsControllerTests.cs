using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Denarius.Application.Exceptions;
using Denarius.Application.IO.Transactions;
using Denarius.Application.UseCases.Transactions.Create;
using Denarius.Application.UseCases.Transactions.Delete;
using Denarius.Application.UseCases.Transactions.GetById;
using Denarius.Application.UseCases.Transactions.List;
using Denarius.Application.UseCases.Transactions.Update;
using Denarius.Domain.Entities;
using Denarius.Domain.Exceptions;
using Denarius.WebAPI.Controllers;
using Denarius.WebAPI.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Denarius.WebAPI.Tests.Transactions;

public class TransactionsControllerTests
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private static readonly DateTime ValidDate = new(2026, 9, 15, 0, 0, 0, DateTimeKind.Utc);

    private static TransactionOutput MakeOutput(string? description = "Mercado", decimal value = 150.75m, Guid? categoryId = null) =>
        new(new Transaction(description, ValidDate, value, categoryId ?? Guid.NewGuid()));

    private static async Task<IHost> CreateHostAsync(
        Func<CreateTransactionInput, TransactionOutput>? create = null,
        Func<(Guid Id, UpdateTransactionInput Input), TransactionOutput>? update = null,
        Action<Guid>? delete = null,
        Func<Guid, TransactionOutput>? getById = null,
        Func<object?, IEnumerable<TransactionOutput>>? list = null)
    {
        var host = await new HostBuilder()
            .ConfigureWebHost(webHost =>
            {
                webHost.UseTestServer();
                webHost.ConfigureServices(services =>
                {
                    services.AddSingleton<ICreateTransactionUseCase>(new FakeCreateTransactionUseCase(
                        create ?? (_ => throw new InvalidOperationException("Create not configured for this test."))));
                    services.AddSingleton<IUpdateTransactionUseCase>(new FakeUpdateTransactionUseCase(
                        update ?? (_ => throw new InvalidOperationException("Update not configured for this test."))));
                    services.AddSingleton<IDeleteTransactionUseCase>(new FakeDeleteTransactionUseCase(
                        delete ?? (_ => throw new InvalidOperationException("Delete not configured for this test."))));
                    services.AddSingleton<IGetTransactionByIdUseCase>(new FakeGetTransactionByIdUseCase(
                        getById ?? (_ => throw new InvalidOperationException("GetById not configured for this test."))));
                    services.AddSingleton<IListTransactionsUseCase>(new FakeListTransactionsUseCase(
                        list ?? (_ => throw new InvalidOperationException("List not configured for this test."))));
                    services.AddExceptionHandler<GlobalExceptionHandler>();
                    services.AddProblemDetails();
                    services.AddControllers().AddApplicationPart(typeof(TransactionsController).Assembly);
                });
                webHost.Configure(app =>
                {
                    app.UseExceptionHandler();
                    app.UseRouting();
                    app.UseEndpoints(endpoints => endpoints.MapControllers());
                });
            })
            .StartAsync();

        return host;
    }

    private static async Task<TransactionResponseBody> ReadTransactionAsync(HttpResponseMessage response)
    {
        var body = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<TransactionResponseBody>(body, JsonOptions)!;
    }

    private static async Task<List<TransactionResponseBody>> ReadTransactionsAsync(HttpResponseMessage response)
    {
        var body = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<TransactionResponseBody>>(body, JsonOptions)!;
    }

    private static async Task<ProblemDetails> ReadProblemDetailsAsync(HttpResponseMessage response)
    {
        var body = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<ProblemDetails>(body)!;
    }

    [Fact]
    public async Task Create_ValidInput_Returns201WithLocationHeaderAndBody()
    {
        var categoryId = Guid.NewGuid();
        var output = MakeOutput(categoryId: categoryId);
        CreateTransactionInput? received = null;
        using var host = await CreateHostAsync(create: input =>
        {
            received = input;
            return output;
        });
        var client = host.GetTestClient();

        var response = await client.PostAsJsonAsync("/api/transactions", new { description = "Mercado", date = ValidDate, value = 150.75m, categoryId });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal($"/api/transactions/{output.Id}", response.Headers.Location?.ToString());
        Assert.Equal("Mercado", received!.Description);
        Assert.Equal(150.75m, received.Value);
        Assert.Equal(categoryId, received.CategoryId);
        var body = await ReadTransactionAsync(response);
        Assert.Equal(output.Id, body.Id);
    }

    [Fact]
    public async Task Create_UseCaseThrowsDomainException_Returns400()
    {
        using var host = await CreateHostAsync(create: _ => throw new DomainException("O valor da transação não pode ser zero."));
        var client = host.GetTestClient();

        var response = await client.PostAsJsonAsync("/api/transactions", new { description = "Mercado", date = ValidDate, value = 0m, categoryId = Guid.NewGuid() });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await ReadProblemDetailsAsync(response);
        Assert.Equal("O valor da transação não pode ser zero.", problem.Detail);
    }

    [Fact]
    public async Task Create_UnknownCategory_Returns404()
    {
        using var host = await CreateHostAsync(create: _ => throw new NotFoundException("Categoria não encontrada."));
        var client = host.GetTestClient();

        var response = await client.PostAsJsonAsync("/api/transactions", new { description = "Mercado", date = ValidDate, value = 150.75m, categoryId = Guid.NewGuid() });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var problem = await ReadProblemDetailsAsync(response);
        Assert.Equal("Categoria não encontrada.", problem.Detail);
    }

    [Fact]
    public async Task GetById_ExistingTransaction_Returns200WithBody()
    {
        var output = MakeOutput(description: "Farmácia", value: 42.5m);
        Guid? requestedId = null;
        using var host = await CreateHostAsync(getById: id =>
        {
            requestedId = id;
            return output;
        });
        var client = host.GetTestClient();

        var response = await client.GetAsync($"/api/transactions/{output.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(output.Id, requestedId);
        var body = await ReadTransactionAsync(response);
        Assert.Equal("Farmácia", body.Description);
        Assert.Equal(42.5m, body.Value);
    }

    [Fact]
    public async Task GetById_UnknownTransaction_Returns404()
    {
        using var host = await CreateHostAsync(getById: _ => throw new NotFoundException("Transação não encontrada."));
        var client = host.GetTestClient();

        var response = await client.GetAsync($"/api/transactions/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var problem = await ReadProblemDetailsAsync(response);
        Assert.Equal("Transação não encontrada.", problem.Detail);
    }

    [Fact]
    public async Task Update_ValidInput_Returns200WithUpdatedBody()
    {
        var id = Guid.NewGuid();
        var newCategoryId = Guid.NewGuid();
        var updated = MakeOutput(description: "Farmácia", value: 42.5m, categoryId: newCategoryId);
        (Guid Id, UpdateTransactionInput Input)? received = null;
        using var host = await CreateHostAsync(update: request =>
        {
            received = request;
            return updated;
        });
        var client = host.GetTestClient();

        var response = await client.PutAsJsonAsync($"/api/transactions/{id}", new { description = "Farmacia", date = ValidDate.AddDays(1), value = 42.5m, categoryId = newCategoryId });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(id, received!.Value.Id);
        Assert.Equal("Farmacia", received.Value.Input.Description);
        Assert.Equal(42.5m, received.Value.Input.Value);
        Assert.Equal(newCategoryId, received.Value.Input.CategoryId);
        var body = await ReadTransactionAsync(response);
        Assert.Equal("Farmácia", body.Description);
        Assert.Equal(42.5m, body.Value);
    }

    [Fact]
    public async Task Update_UnknownTransaction_Returns404()
    {
        using var host = await CreateHostAsync(update: _ => throw new NotFoundException("Transação não encontrada."));
        var client = host.GetTestClient();

        var response = await client.PutAsJsonAsync($"/api/transactions/{Guid.NewGuid()}", new { description = "X", date = ValidDate, value = 1m, categoryId = Guid.NewGuid() });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_ExistingTransaction_Returns204()
    {
        var id = Guid.NewGuid();
        Guid? deletedId = null;
        using var host = await CreateHostAsync(delete: deletedIdArg => deletedId = deletedIdArg);
        var client = host.GetTestClient();

        var response = await client.DeleteAsync($"/api/transactions/{id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(id, deletedId);
    }

    [Fact]
    public async Task Delete_UnknownTransaction_Returns404()
    {
        using var host = await CreateHostAsync(delete: _ => throw new NotFoundException("Transação não encontrada."));
        var client = host.GetTestClient();

        var response = await client.DeleteAsync($"/api/transactions/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task List_TransactionsExist_Returns200WithArrayAndNoQueryParametersRequired()
    {
        var outputs = new[] { MakeOutput(description: "A"), MakeOutput(description: "B") };
        using var host = await CreateHostAsync(list: _ => outputs);
        var client = host.GetTestClient();

        var response = await client.GetAsync("/api/transactions");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await ReadTransactionsAsync(response);
        Assert.Equal(2, body.Count);
    }

    [Fact]
    public async Task List_NoTransactions_Returns200WithEmptyArray()
    {
        using var host = await CreateHostAsync(list: _ => []);
        var client = host.GetTestClient();

        var response = await client.GetAsync("/api/transactions");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await ReadTransactionsAsync(response);
        Assert.Empty(body);
    }

    private sealed record TransactionResponseBody(Guid Id, string? Description, DateTime Date, decimal Value, Guid CategoryId);

    private sealed class FakeCreateTransactionUseCase(Func<CreateTransactionInput, TransactionOutput> handler) : ICreateTransactionUseCase
    {
        public Task<TransactionOutput> Execute(CreateTransactionInput input) => Task.FromResult(handler(input));
    }

    private sealed class FakeUpdateTransactionUseCase(Func<(Guid Id, UpdateTransactionInput Input), TransactionOutput> handler) : IUpdateTransactionUseCase
    {
        public Task<TransactionOutput> Execute((Guid Id, UpdateTransactionInput Input) input) => Task.FromResult(handler(input));
    }

    private sealed class FakeDeleteTransactionUseCase(Action<Guid> handler) : IDeleteTransactionUseCase
    {
        public Task Execute(Guid input)
        {
            handler(input);
            return Task.CompletedTask;
        }
    }

    private sealed class FakeGetTransactionByIdUseCase(Func<Guid, TransactionOutput> handler) : IGetTransactionByIdUseCase
    {
        public Task<TransactionOutput> Execute(Guid input) => Task.FromResult(handler(input));
    }

    private sealed class FakeListTransactionsUseCase(Func<object?, IEnumerable<TransactionOutput>> handler) : IListTransactionsUseCase
    {
        public Task<IEnumerable<TransactionOutput>> Execute(object? input) => Task.FromResult(handler(input));
    }
}
