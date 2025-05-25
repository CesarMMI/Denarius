using Denarius.Application.Auth.Services;
using Denarius.Application.Shared.Services;
using Denarius.Domain.Repositories;
using Denarius.Infrastructure.Identity.Password;
using Denarius.Infrastructure.Identity.Token;
using Denarius.Infrastructure.Persistence;
using Denarius.Infrastructure.Persistence.Repositories;
using Denarius.Infrastructure.Persistence.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Denarius.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        return services
            .AddPersistence(configuration)
            .AddIdentity(configuration);
    }

    private static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options => options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();

        services.AddScoped<IDbTransactionService, DbTransactionService>();

        return services;
    }

    private static IServiceCollection AddIdentity(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<ITokenService, TokenService>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
        {
            options.TokenValidationParameters = TokenServiceExtensions.GetValidationParameters(configuration["JWT:Issuer"]!, configuration["JWT:AccessSecret"]!);
            options.Events = new TokenEventsHandler();
        });
        services.AddAuthorization();

        return services;
    }
}
