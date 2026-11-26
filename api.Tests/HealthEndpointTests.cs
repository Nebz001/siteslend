using System.Net;
using System.Net.Http.Json;
using SitesLend.Api.Dtos;

namespace SitesLend.Api.Tests;

// Both tests run on the Testing host from DatabaseFixture, never on the dev database.
[Collection("Database")]
public class HealthEndpointTests(DatabaseFixture database)
{
    // Needs a reachable PostgreSQL and the test database (see DatabaseFixture).
    [Fact]
    public async Task Get_health_returns_ok_and_database_up()
    {
        var client = database.CreateClient();

        var response = await client.GetAsync("/api/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<HealthResponse>();
        Assert.Equal(new HealthResponse("ok", "up"), body);
    }

    // The connection string points at a port nothing listens on, so no real database is touched.
    [Fact]
    public async Task Get_health_reports_database_down_and_leaks_no_details()
    {
        const string fakePassword = "NotARealPassword-123";
        var client = database.WithWebHostBuilder(builder =>
            builder.UseSetting(
                "ConnectionStrings:DefaultConnection",
                $"Host=127.0.0.1;Port=1;Database=nodb;Username=nouser;Password={fakePassword};Timeout=2"))
            .CreateClient();

        var response = await client.GetAsync("/api/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var raw = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain(fakePassword, raw);
        Assert.DoesNotContain("nouser", raw);
        Assert.DoesNotContain("127.0.0.1", raw);
        var body = await response.Content.ReadFromJsonAsync<HealthResponse>();
        Assert.Equal(new HealthResponse("ok", "down"), body);
    }
}
