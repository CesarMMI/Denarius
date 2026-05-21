using Denarius.Application;
using Denarius.Infrastructure;
using Denarius.Api.Endpoints;
using Denarius.Api.Extensions;
using Denarius.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddJwtAuthentication(builder.Configuration)
    .AddAuthorization()
    .AddOpenApiDocumentation()
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment()) app.MapOpenApi();

app.UseHttpsRedirection()
    .UseMiddleware<ExceptionMiddleware>()
    .UseAuthentication()
    .UseAuthorization();

app.MapAuthEndpoints()
    .MapAccountEndpoints()
    .MapCategoryEndpoints()
    .MapTransactionEndpoints();

app.Run();

public partial class Program { }
