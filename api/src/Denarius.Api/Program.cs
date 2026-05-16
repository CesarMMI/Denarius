using Denarius.Application;
using Denarius.Infrastructure;
using Denarius.Api.Middleware;
using Denarius.Api.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddOpenApi()
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseMiddleware<ExceptionMiddleware>();

app.MapAccountEndpoints();
app.MapCategoryEndpoints();
app.MapTransactionEndpoints();

app.Run();
