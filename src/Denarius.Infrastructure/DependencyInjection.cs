using Denarius.Domain.Repositories;
using Denarius.Domain.Services;
using Denarius.Domain.UnitOfWork;
using Denarius.Infrastructure.Identity.Password;
using Denarius.Infrastructure.Identity.Token;
using Denarius.Infrastructure.Persistence.Ef.AppDbContext;
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
            .AddAuth(configuration)
            .AddDatabase(configuration)
            .AddRepositories()
            .AddServices();
    }

    private static IServiceCollection AddAuth(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
        {
            options.MapInboundClaims = false;
            options.Events = new JwtBearerEventsHandler();
            string issuer = configuration["JWT:Issuer"]!;
            string accessSecret = configuration["JWT:AccessSecret"]!;
            options.TokenValidationParameters = TokenServiceExtensions.GetValidationParameters(issuer, accessSecret);
        });
        services.AddAuthorization();
        return services;
    }

    private static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<EfAppDbContext>(options =>
        {
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
        });
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();
        return services;
    }

    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, EfUserRepository>();
        services.AddScoped<IAccountRepository, EfAccountRepository>();
        services.AddScoped<ICategoryRepository, EfCategoryRepository>();
        services.AddScoped<ITransactionRepository, EfTransactionRepository>();
        return services;
    }

    private static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<ITokenService, TokenService>();
        return services;
    }
}
