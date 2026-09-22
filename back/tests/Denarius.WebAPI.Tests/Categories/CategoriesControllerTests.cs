using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Denarius.Application.Exceptions;
using Denarius.Application.IO.Categories;
using Denarius.Application.UseCases.Categories.Create;
using Denarius.Application.UseCases.Categories.Delete;
using Denarius.Application.UseCases.Categories.GetById;
using Denarius.Application.UseCases.Categories.List;
using Denarius.Application.UseCases.Categories.Update;
using Denarius.Domain.Entities;
using Denarius.Domain.Exceptions;
using Denarius.Domain.ValueObjects;
using Denarius.WebAPI.Controllers;
using Denarius.WebAPI.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Denarius.WebAPI.Tests.Categories;

public class CategoriesControllerTests
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private static CategoryOutput MakeOutput(string name = "Lazer", string color = "#FF0000", int transactionCount = 0, decimal balance = 0m) =>
        new(new Category(name, new Color(color)), transactionCount, balance);

    private static async Task<IHost> CreateHostAsync(
        Func<CreateCategoryInput, CategoryOutput>? create = null,
        Func<(Guid Id, UpdateCategoryInput Input), CategoryOutput>? update = null,
        Action<Guid>? delete = null,
        Func<Guid, CategoryOutput>? getById = null,
        Func<ListCategoriesInput, IEnumerable<CategoryOutput>>? list = null)
    {
        var host = await new HostBuilder()
            .ConfigureWebHost(webHost =>
            {
                webHost.UseTestServer();
                webHost.ConfigureServices(services =>
                {
                    services.AddSingleton<ICreateCategoryUseCase>(new FakeCreateCategoryUseCase(
                        create ?? (_ => throw new InvalidOperationException("Create not configured for this test."))));
                    services.AddSingleton<IUpdateCategoryUseCase>(new FakeUpdateCategoryUseCase(
                        update ?? (_ => throw new InvalidOperationException("Update not configured for this test."))));
                    services.AddSingleton<IDeleteCategoryUseCase>(new FakeDeleteCategoryUseCase(
                        delete ?? (_ => throw new InvalidOperationException("Delete not configured for this test."))));
                    services.AddSingleton<IGetCategoryByIdUseCase>(new FakeGetCategoryByIdUseCase(
                        getById ?? (_ => throw new InvalidOperationException("GetById not configured for this test."))));
                    services.AddSingleton<IListCategoriesUseCase>(new FakeListCategoriesUseCase(
                        list ?? (_ => throw new InvalidOperationException("List not configured for this test."))));
                    services.AddExceptionHandler<GlobalExceptionHandler>();
                    services.AddProblemDetails();
                    services.AddControllers().AddApplicationPart(typeof(CategoriesController).Assembly);
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

    private static async Task<CategoryResponseBody> ReadCategoryAsync(HttpResponseMessage response)
    {
        var body = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<CategoryResponseBody>(body, JsonOptions)!;
    }

    private static async Task<List<CategoryResponseBody>> ReadCategoriesAsync(HttpResponseMessage response)
    {
        var body = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<CategoryResponseBody>>(body, JsonOptions)!;
    }

    private static async Task<ProblemDetails> ReadProblemDetailsAsync(HttpResponseMessage response)
    {
        var body = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<ProblemDetails>(body)!;
    }

    [Fact]
    public async Task Create_ValidInput_Returns201WithLocationHeaderAndBody()
    {
        var output = MakeOutput();
        CreateCategoryInput? received = null;
        using var host = await CreateHostAsync(create: input =>
        {
            received = input;
            return output;
        });
        var client = host.GetTestClient();

        var response = await client.PostAsJsonAsync("/api/categories", new { name = "Lazer", color = "#FF0000" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal($"/api/categories/{output.Id}", response.Headers.Location?.ToString());
        Assert.Equal("Lazer", received!.Name);
        Assert.Equal("#FF0000", received.Color);
        var body = await ReadCategoryAsync(response);
        Assert.Equal(output.Id, body.Id);
    }

    [Fact]
    public async Task Create_UseCaseThrowsDomainException_Returns400()
    {
        using var host = await CreateHostAsync(create: _ => throw new DomainException("O nome da categoria não pode ser vazio."));
        var client = host.GetTestClient();

        var response = await client.PostAsJsonAsync("/api/categories", new { name = "", color = "#FF0000" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await ReadProblemDetailsAsync(response);
        Assert.Equal("O nome da categoria não pode ser vazio.", problem.Detail);
    }

    [Fact]
    public async Task GetById_ExistingCategory_Returns200WithBody()
    {
        var output = MakeOutput(name: "Alimentação", color: "#00FF00");
        Guid? requestedId = null;
        using var host = await CreateHostAsync(getById: id =>
        {
            requestedId = id;
            return output;
        });
        var client = host.GetTestClient();

        var response = await client.GetAsync($"/api/categories/{output.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(output.Id, requestedId);
        var body = await ReadCategoryAsync(response);
        Assert.Equal("Alimentação", body.Name);
        Assert.Equal("#00FF00", body.Color);
    }

    [Fact]
    public async Task GetById_UnknownCategory_Returns404()
    {
        using var host = await CreateHostAsync(getById: _ => throw new NotFoundException("Categoria não encontrada."));
        var client = host.GetTestClient();

        var response = await client.GetAsync($"/api/categories/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var problem = await ReadProblemDetailsAsync(response);
        Assert.Equal("Categoria não encontrada.", problem.Detail);
    }

    [Fact]
    public async Task Update_ValidInput_Returns200WithUpdatedBody()
    {
        var id = Guid.NewGuid();
        var updated = MakeOutput(name: "Diversão", color: "#00FF00");
        (Guid Id, UpdateCategoryInput Input)? received = null;
        using var host = await CreateHostAsync(update: request =>
        {
            received = request;
            return updated;
        });
        var client = host.GetTestClient();

        var response = await client.PutAsJsonAsync($"/api/categories/{id}", new { name = "Diversao", color = "#0F0" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(id, received!.Value.Id);
        Assert.Equal("Diversao", received.Value.Input.Name);
        Assert.Equal("#0F0", received.Value.Input.Color);
        var body = await ReadCategoryAsync(response);
        Assert.Equal("Diversão", body.Name);
        Assert.Equal("#00FF00", body.Color);
    }

    [Fact]
    public async Task Update_UnknownCategory_Returns404()
    {
        using var host = await CreateHostAsync(update: _ => throw new NotFoundException("Categoria não encontrada."));
        var client = host.GetTestClient();

        var response = await client.PutAsJsonAsync($"/api/categories/{Guid.NewGuid()}", new { name = "X", color = "#FFFFFF" });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_ExistingCategory_Returns204()
    {
        var id = Guid.NewGuid();
        Guid? deletedId = null;
        using var host = await CreateHostAsync(delete: deletedIdArg => deletedId = deletedIdArg);
        var client = host.GetTestClient();

        var response = await client.DeleteAsync($"/api/categories/{id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(id, deletedId);
    }

    [Fact]
    public async Task Delete_CategoryHasTransactions_Returns400()
    {
        using var host = await CreateHostAsync(delete: _ =>
            throw new DomainException("A categoria possui transações associadas e não pode ser removida."));
        var client = host.GetTestClient();

        var response = await client.DeleteAsync($"/api/categories/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await ReadProblemDetailsAsync(response);
        Assert.Equal("A categoria possui transações associadas e não pode ser removida.", problem.Detail);
    }

    [Fact]
    public async Task List_NoFilters_Returns200WithArray()
    {
        var outputs = new[] { MakeOutput(name: "A"), MakeOutput(name: "B") };
        using var host = await CreateHostAsync(list: _ => outputs);
        var client = host.GetTestClient();

        var response = await client.GetAsync("/api/categories");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await ReadCategoriesAsync(response);
        Assert.Equal(2, body.Count);
    }

    [Fact]
    public async Task List_WithQueryParameters_BindsThemAndPassesToUseCase()
    {
        ListCategoriesInput? captured = null;
        using var host = await CreateHostAsync(list: input =>
        {
            captured = input;
            return [];
        });
        var client = host.GetTestClient();

        var response = await client.GetAsync(
            "/api/categories?name=Laz&withTransaction=true&dateRef=2026-09-01&orderBy=Balance&asc=false");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(captured);
        Assert.Equal("Laz", captured!.Name);
        Assert.True(captured.WithTransaction);
        Assert.Equal(new DateTime(2026, 9, 1), captured.DateRef!.Value.Date);
        Assert.Equal(CategoryOrderField.Balance, captured.OrderBy);
        Assert.False(captured.Ascending);
    }

    [Fact]
    public async Task List_DefaultQueryParameters_OrdersByNameAscendingWithNoFilters()
    {
        ListCategoriesInput? captured = null;
        using var host = await CreateHostAsync(list: input =>
        {
            captured = input;
            return [];
        });
        var client = host.GetTestClient();

        await client.GetAsync("/api/categories");

        Assert.Null(captured!.Name);
        Assert.Null(captured.WithTransaction);
        Assert.Null(captured.DateRef);
        Assert.Equal(CategoryOrderField.Name, captured.OrderBy);
        Assert.True(captured.Ascending);
    }

    private sealed record CategoryResponseBody(Guid Id, string Name, string Color, int TransactionCount, decimal Balance);

    private sealed class FakeCreateCategoryUseCase(Func<CreateCategoryInput, CategoryOutput> handler) : ICreateCategoryUseCase
    {
        public Task<CategoryOutput> Execute(CreateCategoryInput input) => Task.FromResult(handler(input));
    }

    private sealed class FakeUpdateCategoryUseCase(Func<(Guid Id, UpdateCategoryInput Input), CategoryOutput> handler) : IUpdateCategoryUseCase
    {
        public Task<CategoryOutput> Execute((Guid Id, UpdateCategoryInput Input) input) => Task.FromResult(handler(input));
    }

    private sealed class FakeDeleteCategoryUseCase(Action<Guid> handler) : IDeleteCategoryUseCase
    {
        public Task Execute(Guid input)
        {
            handler(input);
            return Task.CompletedTask;
        }
    }

    private sealed class FakeGetCategoryByIdUseCase(Func<Guid, CategoryOutput> handler) : IGetCategoryByIdUseCase
    {
        public Task<CategoryOutput> Execute(Guid input) => Task.FromResult(handler(input));
    }

    private sealed class FakeListCategoriesUseCase(Func<ListCategoriesInput, IEnumerable<CategoryOutput>> handler) : IListCategoriesUseCase
    {
        public Task<IEnumerable<CategoryOutput>> Execute(ListCategoriesInput input) => Task.FromResult(handler(input));
    }
}
