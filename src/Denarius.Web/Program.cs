using Denarius.Application;
using Denarius.Infrastructure;
using Denarius.Web.Middlewares;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services
    .AddOpenApi()
    .AddInfrastructure(builder.Configuration)
    .AddApplication();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app
    .UseHttpsRedirection()
    .UseExceptionMiddleware()
    .UseAuthentication()
    .UseAuthorization();

app.MapControllers();

app.Run();
