using Denarius.WebAPI.Controllers;
using Denarius.WebAPI.Routing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Denarius.WebAPI.Tests.Routing;

public class RoutingExtensionsTests
{
    [Theory]
    [InlineData("Categories", "categories")]
    [InlineData("RecurringTransactions", "recurringTransactions")]
    [InlineData("categories", "categories")]
    public void CamelCaseRouteTransformer_TransformsToCamelCase(string value, string expected)
    {
        Assert.Equal(expected, new CamelCaseRouteTransformer().TransformOutbound(value));
    }

    [Fact]
    public void CamelCaseRouteTransformer_NullValue_ReturnsNull()
    {
        Assert.Null(new CamelCaseRouteTransformer().TransformOutbound(null));
    }

    [Fact]
    public async Task UseCamelCaseRoutes_ControllerRoutes_AreCamelCase()
    {
        using var host = await new HostBuilder()
            .ConfigureWebHost(webHost =>
            {
                webHost.UseTestServer();
                webHost.ConfigureServices(services =>
                    services.AddControllers(options => options.UseCamelCaseRoutes()).AddApplicationPart(typeof(CategoriesController).Assembly));
                webHost.Configure(app =>
                {
                    app.UseRouting();
                    app.UseEndpoints(endpoints => endpoints.MapControllers());
                });
            })
            .StartAsync();

        var routes = host.Services.GetRequiredService<EndpointDataSource>().Endpoints
            .OfType<RouteEndpoint>()
            .Select(endpoint => endpoint.RoutePattern.RawText)
            .Distinct()
            .Order()
            .ToList();

        Assert.Equal(["api/categories", "api/categories/{id}", "api/transactions", "api/transactions/{id}"], routes);
    }
}
