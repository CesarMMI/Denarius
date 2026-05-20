using System.Net;
using System.Net.Http.Json;
using Denarius.Api.Requests.Auth;
using Denarius.Api.Tests.Shared;
using Denarius.Application.Inputs.Auth;
using Denarius.Application.Interfaces.UseCases.Auth;
using Denarius.Application.Outputs.Auth;
using Microsoft.AspNetCore.Mvc.Testing;
using NSubstitute;

namespace Denarius.Api.Tests.Endpoints;

public class AuthEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly IRegisterUserUseCase _registerUser = Substitute.For<IRegisterUserUseCase>();
    private readonly ILoginUseCase _login = Substitute.For<ILoginUseCase>();
    private readonly HttpClient _client;

    public AuthEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateTestClient(services =>
        {
            services.AddScoped<IRegisterUserUseCase>(_ => _registerUser);
            services.AddScoped<ILoginUseCase>(_ => _login);
        });
    }

    [Fact]
    public async Task Register_WithValidRequest_Returns201Created()
    {
        _registerUser
            .Execute(Arg.Any<RegisterUserInput>())
            .Returns(Task.FromResult(new UserOutput(Guid.NewGuid(), "user@example.com", "Test User", DateTime.UtcNow)));

        var response = await _client.PostAsJsonAsync("/api/auth/register",
            new RegisterUserRequest("user@example.com", "Password123!", "Test User"));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Register_CallsUseCaseWithCorrectInput()
    {
        _registerUser
            .Execute(Arg.Any<RegisterUserInput>())
            .Returns(Task.FromResult(new UserOutput(Guid.NewGuid(), "user@example.com", "Test User", DateTime.UtcNow)));

        await _client.PostAsJsonAsync("/api/auth/register",
            new RegisterUserRequest("user@example.com", "Password123!", "Test User"));

        await _registerUser
            .Received(1)
            .Execute(Arg.Is<RegisterUserInput>(i =>
                i.Email == "user@example.com" &&
                i.Password == "Password123!" &&
                i.Name == "Test User"));
    }

    [Fact]
    public async Task Login_WithValidRequest_Returns200Ok()
    {
        var userOutput = new UserOutput(Guid.NewGuid(), "user@example.com", "Test User", DateTime.UtcNow);
        _login
            .Execute(Arg.Any<LoginInput>())
            .Returns(Task.FromResult(new LoginOutput("jwt_token", userOutput)));

        var response = await _client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest("user@example.com", "Password123!"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Login_CallsUseCaseWithCorrectInput()
    {
        var userOutput = new UserOutput(Guid.NewGuid(), "user@example.com", "Test User", DateTime.UtcNow);
        _login
            .Execute(Arg.Any<LoginInput>())
            .Returns(Task.FromResult(new LoginOutput("jwt_token", userOutput)));

        await _client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest("user@example.com", "Password123!"));

        await _login
            .Received(1)
            .Execute(Arg.Is<LoginInput>(i =>
                i.Email == "user@example.com" &&
                i.Password == "Password123!"));
    }
}
