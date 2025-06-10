using Denarius.Application.Auth.Services;
using Denarius.Application.Shared.UnitOfWork;
using Denarius.Domain.Repositories;
using Denarius.Infrastructure.Identity.Password;
using Denarius.Infrastructure.Identity.Token;
using Denarius.Infrastructure.Persistence.Ef.Repositories;
using Denarius.Infrastructure.Persistence.Ef.UnitOfWork;
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
        services.AddDbContext<EfDbContext>(options =>
        {
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
        });

        services.AddScoped<IUnitOfWork, EfUnitOfWork>();

        services.AddScoped<IUserRepository, EfUserRepository>();
        services.AddScoped<IAccountRepository, EfAccountRepository>();
        services.AddScoped<ICategoryRepository, EfCategoryRepository>();
        services.AddScoped<ITransactionRepository, EfTransactionRepository>();

        return services;
    }

    private static IServiceCollection AddIdentity(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<ITokenService, TokenService>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
        {
            options.Events = new TokenEventsHandler();
            options.MapInboundClaims = false;
            options.TokenValidationParameters = TokenServiceExtensions.GetValidationParameters(
                configuration["JWT:Issuer"]!,
                configuration["JWT:AccessSecret"]!
            );
        });
        services.AddAuthorization();

        return services;
    }
}
