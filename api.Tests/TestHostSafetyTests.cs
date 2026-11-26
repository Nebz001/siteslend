using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;
using SitesLend.Api.Data;

namespace SitesLend.Api.Tests;

// Guards that keep the test host away from the dev database.
[Collection("Database")]
public class TestHostSafetyTests(DatabaseFixture database)
{
    [Fact]
    public void Test_host_uses_only_the_siteslend_test_database()
    {
        using var scope = database.CreateScope();
        var connectionString = scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.GetConnectionString();

        Assert.False(string.IsNullOrWhiteSpace(connectionString));
        Assert.Equal(
            DatabaseFixture.TestDatabaseName,
            new NpgsqlConnectionStringBuilder(connectionString).Database);
    }

    [Fact]
    public void Test_host_runs_in_the_Testing_environment()
    {
        using var scope = database.CreateScope();

        Assert.Equal("Testing", scope.ServiceProvider.GetRequiredService<IHostEnvironment>().EnvironmentName);
    }

    [Fact]
    public async Task Seeder_refuses_to_run_outside_Development_and_leaves_the_tables_empty()
    {
        await database.ResetAsync();

        await DbSeeder.RunSafelyAsync(database.Services);

        using var scope = database.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.False(await db.AssetCategories.AnyAsync());
        Assert.False(await db.AssetUnits.AnyAsync());
    }
}
