using System.Net;
using System.Net.Http.Json;
using Denarius.Api.Requests.Categories;
using Denarius.Api.Tests.Shared;
using Denarius.Application.Inputs.Categories;
using Denarius.Application.Interfaces.UseCases.Categories;
using Denarius.Application.Outputs.Categories;
using Denarius.Domain.Enums;
using Microsoft.AspNetCore.Mvc.Testing;
using NSubstitute;

namespace Denarius.Api.Tests.Endpoints;

public class CategoryEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly IListCategoriesUseCase _listCategories = Substitute.For<IListCategoriesUseCase>();
    private readonly IGetCategoryByIdUseCase _getCategoryById = Substitute.For<IGetCategoryByIdUseCase>();
    private readonly ICreateCategoryUseCase _createCategory = Substitute.For<ICreateCategoryUseCase>();
    private readonly IUpdateCategoryUseCase _updateCategory = Substitute.For<IUpdateCategoryUseCase>();
    private readonly IDeleteCategoryUseCase _deleteCategory = Substitute.For<IDeleteCategoryUseCase>();
    private readonly HttpClient _client;

    public CategoryEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateTestClient(services =>
        {
            services.AddScoped<IListCategoriesUseCase>(_ => _listCategories);
            services.AddScoped<IGetCategoryByIdUseCase>(_ => _getCategoryById);
            services.AddScoped<ICreateCategoryUseCase>(_ => _createCategory);
            services.AddScoped<IUpdateCategoryUseCase>(_ => _updateCategory);
            services.AddScoped<IDeleteCategoryUseCase>(_ => _deleteCategory);
        });
    }

    private static CategoryOutput SampleCategory(Guid? id = null) => new(
        id ?? Guid.NewGuid(), "Test Category", "#FF0000", CategoryType.Expense,
        DateTime.UtcNow, DateTime.UtcNow);

    [Fact]
    public async Task ListCategories_ReturnsOk()
    {
        _listCategories
            .Execute(Arg.Any<ListCategoriesInput>())
            .Returns(Task.FromResult<IEnumerable<CategoryOutput>>([SampleCategory()]));

        var response = await _client.GetAsync("/api/categories");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ListCategories_CallsUseCaseWithCurrentUserId()
    {
        _listCategories
            .Execute(Arg.Any<ListCategoriesInput>())
            .Returns(Task.FromResult<IEnumerable<CategoryOutput>>([]));

        await _client.GetAsync("/api/categories");

        await _listCategories
            .Received(1)
            .Execute(Arg.Is<ListCategoriesInput>(i => i.UserId == TestAuthHandler.UserId));
    }

    [Fact]
    public async Task GetCategoryById_ReturnsOk()
    {
        var categoryId = Guid.NewGuid();
        _getCategoryById
            .Execute(Arg.Any<GetCategoryByIdInput>())
            .Returns(Task.FromResult(SampleCategory(categoryId)));

        var response = await _client.GetAsync($"/api/categories/{categoryId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CreateCategory_Returns201Created()
    {
        var categoryId = Guid.NewGuid();
        _createCategory
            .Execute(Arg.Any<CreateCategoryInput>())
            .Returns(Task.FromResult(SampleCategory(categoryId)));

        var response = await _client.PostAsJsonAsync("/api/categories",
            new CreateCategoryRequest("Food", "#FF0000", CategoryType.Expense));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal($"/api/categories/{categoryId}", response.Headers.Location?.ToString());
    }

    [Fact]
    public async Task UpdateCategory_ReturnsOk()
    {
        var categoryId = Guid.NewGuid();
        _updateCategory
            .Execute(Arg.Any<UpdateCategoryInput>())
            .Returns(Task.FromResult(SampleCategory(categoryId)));

        var response = await _client.PutAsJsonAsync($"/api/categories/{categoryId}",
            new UpdateCategoryRequest("Updated Name", "#00FF00"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task DeleteCategory_Returns204NoContent()
    {
        _deleteCategory
            .Execute(Arg.Any<DeleteCategoryInput>())
            .Returns(Task.CompletedTask);

        var response = await _client.DeleteAsync($"/api/categories/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
}
