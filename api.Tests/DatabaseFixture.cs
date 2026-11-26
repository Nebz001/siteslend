using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using SitesLend.Api.Data;

namespace SitesLend.Api.Tests;

/// <summary>
/// One API host (environment "Testing") and one real PostgreSQL test database (siteslend_test)
/// shared by the tests in the "Database" collection. The connection string comes from the SITESLEND_TEST_CONNECTION
/// environment variable, or from the API user-secret ConnectionStrings:TestConnection.
/// </summary>
public sealed class DatabaseFixture : IAsyncLifetime
{
    public const string TestDatabaseName = "siteslend_test";

    private WebApplicationFactory<Program>? _factory;

    public HttpClient CreateClient() => _factory!.CreateClient();

    /// <summary>A variation of the Testing host (for example with a different setting). Still not Development.</summary>
    public WebApplicationFactory<Program> WithWebHostBuilder(Action<IWebHostBuilder> configure)
        => _factory!.WithWebHostBuilder(configure);

    public IServiceProvider Services => _factory!.Services;

    public IServiceScope CreateScope() => _factory!.Services.CreateScope();

    public async Task InitializeAsync()
    {
        var connectionString = ReadConnectionString();

        // Safety: tests delete rows, so refuse any database except the dedicated test database.
        var databaseName = new NpgsqlConnectionStringBuilder(connectionString).Database;
        if (databaseName != TestDatabaseName)
        {
            throw new InvalidOperationException(
                $"Refusing to run: the test database must be \"{TestDatabaseName}\" (got \"{databaseName}\").");
        }

        // "Testing" is not Development, so the sample-data seeder and user-secrets stay out of it.
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:DefaultConnection", connectionString);
        });

        // Applies the real migrations (and creates siteslend_test if it does not exist yet).
        using var scope = CreateScope();
        await scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.MigrateAsync();
    }

    /// <summary>Empties both tables so each test starts from a known state.</summary>
    public async Task ResetAsync()
    {
        using var scope = CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.AssetUnits.ExecuteDeleteAsync();
        await db.AssetCategories.ExecuteDeleteAsync();
    }

    public Task DisposeAsync()
    {
        _factory?.Dispose();
        return Task.CompletedTask;
    }

    private static string ReadConnectionString()
    {
        var fromEnvironment = Environment.GetEnvironmentVariable("SITESLEND_TEST_CONNECTION");
        if (!string.IsNullOrWhiteSpace(fromEnvironment))
        {
            return fromEnvironment;
        }

        var fromSecrets = new ConfigurationBuilder()
            .AddUserSecrets<Program>()
            .Build()["ConnectionStrings:TestConnection"];
        if (!string.IsNullOrWhiteSpace(fromSecrets))
        {
            return fromSecrets;
        }

        throw new InvalidOperationException(
            "No test database configured. Set the SITESLEND_TEST_CONNECTION environment variable or run: " +
            "dotnet user-secrets set \"ConnectionStrings:TestConnection\" " +
            "\"Host=localhost;Port=5432;Database=siteslend_test;Username=siteslend_app;Password=YOUR_PASSWORD\" --project api");
    }
}

/// <summary>Tests in this collection share one database, so xUnit runs them one after another.</summary>
[CollectionDefinition("Database")]
public class DatabaseCollection : ICollectionFixture<DatabaseFixture>;
