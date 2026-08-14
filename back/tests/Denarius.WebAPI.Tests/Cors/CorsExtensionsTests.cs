using Denarius.WebAPI.Cors;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Denarius.WebAPI.Tests.Cors;

public class CorsExtensionsTests
{
    private static async Task<IHost> CreateHostAsync(Dictionary<string, string?> corsConfig)
    {
        var host = await new HostBuilder()
            .ConfigureWebHost(webHost =>
            {
                webHost.UseTestServer();
                webHost.ConfigureAppConfiguration(config => config.AddInMemoryCollection(corsConfig));
                webHost.ConfigureServices((context, services) =>
                {
                    services.AddRouting();
                    services.AddCorsPolicy(context.Configuration);
                });
                webHost.Configure(app =>
                {
                    app.UseRouting();
                    app.UseCors();
                    app.UseEndpoints(endpoints => endpoints.MapGet("/ok", () => Results.Ok("ok")));
                });
            })
            .StartAsync();

        return host;
    }

    private static HttpRequestMessage RequestWithOrigin(string origin) =>
        new(HttpMethod.Get, "/ok") { Headers = { { "Origin", origin } } };

    [Fact]
    public async Task AllowedOrigin_ReturnsMatchingAccessControlAllowOriginHeader()
    {
        using var host = await CreateHostAsync(new Dictionary<string, string?>
        {
            ["Cors:AllowedOrigins:0"] = "http://localhost:4200",
        });
        var client = host.GetTestClient();

        var response = await client.SendAsync(RequestWithOrigin("http://localhost:4200"));

        Assert.Equal("http://localhost:4200", response.Headers.GetValues("Access-Control-Allow-Origin").Single());
    }

    [Fact]
    public async Task DisallowedOrigin_DoesNotReturnAccessControlAllowOriginHeader()
    {
        using var host = await CreateHostAsync(new Dictionary<string, string?>
        {
            ["Cors:AllowedOrigins:0"] = "http://localhost:4200",
        });
        var client = host.GetTestClient();

        var response = await client.SendAsync(RequestWithOrigin("http://evil.example.com"));

        Assert.False(response.Headers.Contains("Access-Control-Allow-Origin"));
    }

    [Fact]
    public async Task NoOriginsConfigured_DoesNotAllowAnyOrigin()
    {
        using var host = await CreateHostAsync([]);
        var client = host.GetTestClient();

        var response = await client.SendAsync(RequestWithOrigin("http://localhost:4200"));

        Assert.False(response.Headers.Contains("Access-Control-Allow-Origin"));
    }

    [Fact]
    public async Task AllowCredentialsTrue_ReturnsAccessControlAllowCredentialsHeader()
    {
        using var host = await CreateHostAsync(new Dictionary<string, string?>
        {
            ["Cors:AllowedOrigins:0"] = "http://localhost:4200",
            ["Cors:AllowCredentials"] = "true",
        });
        var client = host.GetTestClient();

        var response = await client.SendAsync(RequestWithOrigin("http://localhost:4200"));

        Assert.Equal("true", response.Headers.GetValues("Access-Control-Allow-Credentials").Single());
    }

    [Fact]
    public async Task AllowCredentialsFalse_DoesNotReturnAccessControlAllowCredentialsHeader()
    {
        using var host = await CreateHostAsync(new Dictionary<string, string?>
        {
            ["Cors:AllowedOrigins:0"] = "http://localhost:4200",
        });
        var client = host.GetTestClient();

        var response = await client.SendAsync(RequestWithOrigin("http://localhost:4200"));

        Assert.False(response.Headers.Contains("Access-Control-Allow-Credentials"));
    }
}
