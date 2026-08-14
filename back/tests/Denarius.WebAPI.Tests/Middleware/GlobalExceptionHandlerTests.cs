using System.Net;
using System.Text.Json;
using Denarius.Application.Exceptions;
using Denarius.Domain.Exceptions;
using Denarius.WebAPI.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using AppException = Denarius.Application.Exceptions.AppException;

namespace Denarius.WebAPI.Tests.Middleware;

public class GlobalExceptionHandlerTests : IAsyncLifetime
{
    private IHost _host = null!;
    private HttpClient _client = null!;

    public async Task InitializeAsync()
    {
        _host = await new HostBuilder()
            .ConfigureWebHost(webHost =>
            {
                webHost.UseTestServer();
                webHost.ConfigureServices(services =>
                {
                    services.AddRouting();
                    services.AddExceptionHandler<GlobalExceptionHandler>();
                    services.AddProblemDetails();
                });
                webHost.Configure(app =>
                {
                    app.UseExceptionHandler();
                    app.UseRouting();
                    app.UseEndpoints(endpoints =>
                    {
                        endpoints.MapGet("/not-found", () => { throw new NotFoundException("Categoria não encontrada."); });
                        endpoints.MapGet("/domain-error", () => { throw new DomainException("Nome inválido."); });
                        endpoints.MapGet("/application-error", () => { throw new AppException("Falha de aplicação."); });
                        endpoints.MapGet("/unexpected", () => { throw new InvalidOperationException("stack trace secret"); });
                        endpoints.MapGet("/ok", () => Results.Ok("ok"));
                    });
                });
            })
            .StartAsync();

        _client = _host.GetTestClient();
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _host.StopAsync();
        _host.Dispose();
    }

    [Fact]
    public async Task TryHandleAsync_NotFoundException_Returns404WithMessageAsDetail()
    {
        var response = await _client.GetAsync("/not-found");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var problem = await ReadProblemDetailsAsync(response);
        Assert.Equal("Not Found", problem.Title);
        Assert.Equal("Categoria não encontrada.", problem.Detail);
    }

    [Fact]
    public async Task TryHandleAsync_DomainException_Returns400WithMessageAsDetail()
    {
        var response = await _client.GetAsync("/domain-error");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await ReadProblemDetailsAsync(response);
        Assert.Equal("Nome inválido.", problem.Detail);
    }

    [Fact]
    public async Task TryHandleAsync_ApplicationException_Returns400WithMessageAsDetail()
    {
        var response = await _client.GetAsync("/application-error");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await ReadProblemDetailsAsync(response);
        Assert.Equal("Falha de aplicação.", problem.Detail);
    }

    [Fact]
    public async Task TryHandleAsync_UnmappedException_Returns500WithGenericDetailAndDoesNotLeakExceptionMessage()
    {
        var response = await _client.GetAsync("/unexpected");

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        var problem = JsonSerializer.Deserialize<ProblemDetails>(body)!;
        Assert.Equal("Ocorreu um erro inesperado.", problem.Detail);
        Assert.DoesNotContain("stack trace secret", body);
    }

    [Fact]
    public async Task NoException_RequestPassesThroughUnmodified()
    {
        var response = await _client.GetAsync("/ok");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private static async Task<ProblemDetails> ReadProblemDetailsAsync(HttpResponseMessage response)
    {
        var body = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<ProblemDetails>(body)!;
    }
}
