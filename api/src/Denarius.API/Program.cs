using Denarius.Infrastructure;
using Denarius.Application;
using Scalar.AspNetCore;
using Denarius.API.Middlewares;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services
    .AddOpenApi() // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app
    .UseHttpsRedirection()
    .UseMiddleware<ExceptionMiddleware>()
    .UseAuthorization();

app.MapControllers();

app.Run();
