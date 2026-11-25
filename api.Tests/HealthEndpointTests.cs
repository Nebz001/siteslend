using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using SitesLend.Api.Dtos;

namespace SitesLend.Api.Tests;

public class HealthEndpointTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    // Needs a reachable PostgreSQL and the user-secrets connection string from the README.
    [Fact]
    public async Task Get_health_returns_ok_and_database_up()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<HealthResponse>();
        Assert.Equal(new HealthResponse("ok", "up"), body);
    }

    // Needs no database: the connection string points at a port nothing listens on.
    [Fact]
    public async Task Get_health_reports_database_down_and_leaks_no_details()
    {
        const string fakePassword = "NotARealPassword-123";
        var client = factory.WithWebHostBuilder(builder =>
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
