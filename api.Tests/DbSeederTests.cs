using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SitesLend.Api.Data;

namespace SitesLend.Api.Tests;

// Needs a reachable PostgreSQL and a test database connection (see DatabaseFixture).
[Collection("Database")]
public class DbSeederTests(DatabaseFixture database)
{
    [Fact]
    public async Task Seed_fills_an_empty_database_once_and_skips_a_database_that_has_data()
    {
        await database.ResetAsync();
        using var scope = database.CreateScope();
        var provider = scope.ServiceProvider;

        await provider.GetRequiredService<DbSeeder>().SeedAsync();
        var db = provider.GetRequiredService<AppDbContext>();
        Assert.Equal(4, await db.AssetCategories.CountAsync());
        Assert.Equal(12, await db.AssetUnits.CountAsync());

        // Second run: the tables are not empty, so nothing is added.
        await provider.GetRequiredService<DbSeeder>().SeedAsync();
        Assert.Equal(4, await db.AssetCategories.CountAsync());
        Assert.Equal(12, await db.AssetUnits.CountAsync());
    }
}
