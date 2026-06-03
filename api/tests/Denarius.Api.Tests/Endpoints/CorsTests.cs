using System.Net;
using System.Net.Http.Json;
using Denarius.Api.Extensions;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace Denarius.Api.Tests.Endpoints;

public class CorsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public CorsTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    private HttpClient CreateClientWithCorsPolicy(string[] origins, string[] methods, string[] headers)
    {
        var policyBuilder = new CorsPolicyBuilder();

        if (origins.Length > 0) policyBuilder.WithOrigins(origins);
        else policyBuilder.AllowAnyOrigin();

        if (methods.Length > 0) policyBuilder.WithMethods(methods);
        else policyBuilder.AllowAnyMethod();

        if (headers.Length > 0) policyBuilder.WithHeaders(headers);
        else policyBuilder.AllowAnyHeader();

        var policy = policyBuilder.Build();

        return _factory.WithWebHostBuilder(b =>
            b.ConfigureTestServices(services =>
                services.AddSingleton<ICorsPolicyProvider>(new FixedCorsPolicyProvider(policy)))
        ).CreateClient();
    }

    private static HttpRequestMessage Preflight(string origin, string method, string? requestHeader = null)
    {
        var req = new HttpRequestMessage(HttpMethod.Options, "/api/auth/login");
        req.Headers.Add("Origin", origin);
        req.Headers.Add("Access-Control-Request-Method", method);
        if (requestHeader is not null)
            req.Headers.Add("Access-Control-Request-Headers", requestHeader);
        return req;
    }

    [Fact]
    public async Task Preflight_WithAllowedOriginAndMethod_Returns204WithCorsHeaders()
    {
        var client = CreateClientWithCorsPolicy(
            origins: ["http://allowed.com"],
            methods: ["GET", "POST"],
            headers: ["Content-Type", "Authorization"]);

        var response = await client.SendAsync(Preflight("http://allowed.com", "POST", "Content-Type"));

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.True(response.Headers.Contains("Access-Control-Allow-Origin"));
        Assert.Equal("http://allowed.com", response.Headers.GetValues("Access-Control-Allow-Origin").First());
    }

    [Fact]
    public async Task Preflight_WithDisallowedOrigin_OmitsCorsHeaders()
    {
        var client = CreateClientWithCorsPolicy(
            origins: ["http://allowed.com"],
            methods: ["GET"],
            headers: ["Content-Type"]);

        var response = await client.SendAsync(Preflight("http://evil.com", "GET"));

        Assert.False(response.Headers.Contains("Access-Control-Allow-Origin"));
    }

    [Fact]
    public async Task Preflight_WithDisallowedMethod_DoesNotAllowDisallowedMethod()
    {
        var client = CreateClientWithCorsPolicy(
            origins: ["http://allowed.com"],
            methods: ["GET", "POST"],
            headers: ["Content-Type"]);

        var response = await client.SendAsync(Preflight("http://allowed.com", "DELETE"));

        // Origin is allowed so ACAO is present; browser blocks because DELETE is absent from ACAM
        Assert.True(response.Headers.Contains("Access-Control-Allow-Origin"));
        var acam = response.Headers.TryGetValues("Access-Control-Allow-Methods", out var acamVals)
            ? string.Join(",", acamVals) : string.Empty;
        Assert.DoesNotContain("DELETE", acam, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Preflight_WithDisallowedHeader_DoesNotAllowDisallowedHeader()
    {
        var client = CreateClientWithCorsPolicy(
            origins: ["http://allowed.com"],
            methods: ["POST"],
            headers: ["Content-Type"]);

        var response = await client.SendAsync(Preflight("http://allowed.com", "POST", "X-Custom-Header"));

        // Origin is allowed so ACAO is present; browser blocks because X-Custom-Header is absent from ACAH
        Assert.True(response.Headers.Contains("Access-Control-Allow-Origin"));
        var acah = response.Headers.TryGetValues("Access-Control-Allow-Headers", out var acahVals)
            ? string.Join(",", acahVals) : string.Empty;
        Assert.DoesNotContain("X-Custom-Header", acah, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Request_WithAllowedOrigin_IncludesCorsHeader()
    {
        var client = CreateClientWithCorsPolicy(
            origins: ["http://allowed.com"],
            methods: ["POST"],
            headers: ["Content-Type"]);

        var req = new HttpRequestMessage(HttpMethod.Post, "/api/auth/login");
        req.Headers.Add("Origin", "http://allowed.com");
        req.Content = JsonContent.Create(new { email = "t@t.com", password = "pass" });

        var response = await client.SendAsync(req);

        Assert.True(response.Headers.Contains("Access-Control-Allow-Origin"));
        Assert.Equal("http://allowed.com", response.Headers.GetValues("Access-Control-Allow-Origin").First());
    }

    [Fact]
    public async Task Request_WithoutOrigin_OmitsCorsHeaders()
    {
        var client = CreateClientWithCorsPolicy(
            origins: ["http://allowed.com"],
            methods: ["POST"],
            headers: ["Content-Type"]);

        var req = new HttpRequestMessage(HttpMethod.Post, "/api/auth/login");
        req.Content = JsonContent.Create(new { email = "t@t.com", password = "pass" });

        var response = await client.SendAsync(req);

        Assert.False(response.Headers.Contains("Access-Control-Allow-Origin"));
    }

    [Fact]
    public async Task Preflight_WithEmptyPolicy_AllowsAnyOrigin()
    {
        var client = CreateClientWithCorsPolicy(origins: [], methods: [], headers: []);

        var response = await client.SendAsync(Preflight("http://any-random-origin.com", "GET"));

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.True(response.Headers.Contains("Access-Control-Allow-Origin"));
    }

    private sealed class FixedCorsPolicyProvider : ICorsPolicyProvider
    {
        private readonly CorsPolicy _policy;
        public FixedCorsPolicyProvider(CorsPolicy policy) => _policy = policy;
        public Task<CorsPolicy?> GetPolicyAsync(HttpContext context, string? policyName)
            => Task.FromResult<CorsPolicy?>(_policy);
    }
}
