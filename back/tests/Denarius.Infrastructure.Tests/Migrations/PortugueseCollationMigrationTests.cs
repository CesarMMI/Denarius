using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Denarius.Infrastructure.Tests.Migrations;

public class PortugueseCollationMigrationTests(PostgreSqlFixture fixture) : IClassFixture<PostgreSqlFixture>
{
    private const string PreviousMigration = "20261010013826_AddUnaccentExtension";

    /// <summary>The providers of the <c>pt_br_icu</c> collations in the database: <c>i</c> for ICU, empty when it does not exist.</summary>
    private async Task<List<string>> CollationProvidersAsync()
    {
        await using var context = fixture.CreateContext();

        return await context.Database
            .SqlQueryRaw<string>("""SELECT collprovider::text AS "Value" FROM pg_collation WHERE collname = 'pt_br_icu'""")
            .ToListAsync();
    }

    private async Task MigrateAsync(string? targetMigration)
    {
        await using var context = fixture.CreateContext();
        await context.GetService<IMigrator>().MigrateAsync(targetMigration);
    }

    [Fact]
    public async Task Up_CreatesTheIcuCollation()
    {
        Assert.Equal(["i"], await CollationProvidersAsync());
    }

    [Fact]
    public async Task Down_RemovesTheCollationAndUpCreatesItAgain()
    {
        try
        {
            await MigrateAsync(PreviousMigration);
            Assert.Empty(await CollationProvidersAsync());
        }
        finally
        {
            await MigrateAsync(null);
        }

        Assert.Equal(["i"], await CollationProvidersAsync());
    }
}
