using Denarius.Domain.Interfaces;
using Denarius.Infrastructure.Contexts;
using Denarius.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Denarius.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection builder, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default");
        builder.AddDbContext<AppDbContext>(options => options.UseSqlite(connectionString));

        builder.AddScoped<IAccountRepository, AccountRepository>();
        builder.AddScoped<ITagRepository, TagRepository>();
        builder.AddScoped<ITransactionRepository, TransactionRepository>();

        return builder;
    }
}
