using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using SitesLend.Api.Data;
using SitesLend.Api.Dtos;
using SitesLend.Api.Entities;

namespace SitesLend.Api.Tests;

// Needs a reachable PostgreSQL and a test database connection (see DatabaseFixture).
[Collection("Database")]
public class AssetsEndpointTests(DatabaseFixture database)
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    private async Task SeedAsync()
    {
        await database.ResetAsync();
        using var scope = database.CreateScope();
        await scope.ServiceProvider.GetRequiredService<DbSeeder>().SeedAsync();
    }

    [Fact]
    public async Task Get_assets_returns_200_with_seeded_items_sorted_by_tag()
    {
        await SeedAsync();

        var response = await database.CreateClient().GetAsync("/api/assets");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var page = await response.Content.ReadFromJsonAsync<PagedResult<AssetUnitDto>>(Json);
        Assert.NotNull(page);
        Assert.Equal(12, page.TotalCount);
        Assert.Equal(12, page.Items.Count);
        Assert.Equal(1, page.Page);
        Assert.Equal(20, page.PageSize);
        Assert.Equal(page.Items.Select(i => i.AssetTag).Order(StringComparer.Ordinal), page.Items.Select(i => i.AssetTag));

        var laptop = page.Items.Single(i => i.AssetTag == "LAP-0004");
        Assert.Equal("Laptops", laptop.CategoryName);
        Assert.Equal(AssetCondition.Damaged, laptop.Condition);
        Assert.Equal(AssetState.UnderRepair, laptop.State);
    }

    [Fact]
    public async Task Get_assets_does_not_expose_internal_fields()
    {
        await SeedAsync();

        var raw = await database.CreateClient().GetStringAsync("/api/assets");

        Assert.DoesNotContain("version", raw, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("categoryId", raw, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("xmin", raw, StringComparison.OrdinalIgnoreCase);

        // Strongest check: every item has exactly the six public fields, nothing more.
        using var document = JsonDocument.Parse(raw);
        var expected = new[] { "assetTag", "categoryName", "condition", "description", "id", "state" };
        foreach (var item in document.RootElement.GetProperty("items").EnumerateArray())
        {
            Assert.Equal(expected, item.EnumerateObject().Select(p => p.Name).Order(StringComparer.Ordinal));
        }
    }

    [Fact]
    public async Task Get_assets_on_an_empty_database_returns_an_empty_list()
    {
        await database.ResetAsync();

        var response = await database.CreateClient().GetAsync("/api/assets");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var page = await response.Content.ReadFromJsonAsync<PagedResult<AssetUnitDto>>(Json);
        Assert.NotNull(page);
        Assert.Empty(page.Items);
        Assert.Equal(0, page.TotalCount);
    }

    [Fact]
    public async Task Get_assets_pages_the_sorted_list()
    {
        await SeedAsync();

        var response = await database.CreateClient().GetAsync("/api/assets?page=3&pageSize=5");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var page = await response.Content.ReadFromJsonAsync<PagedResult<AssetUnitDto>>(Json);
        Assert.NotNull(page);
        Assert.Equal(12, page.TotalCount);
        Assert.Equal(3, page.Page);
        Assert.Equal(5, page.PageSize);
        Assert.Equal(["PRJ-0001", "PRJ-0002"], page.Items.Select(i => i.AssetTag));
    }

    // The "should be blocked" test: the endpoint is anonymous for now, so the thing it must
    // refuse is bad paging input. The server validates; the browser is not trusted.
    [Theory]
    [InlineData("page=0")]
    [InlineData("pageSize=0")]
    [InlineData("pageSize=101")]
    [InlineData("pageSize=abc")]
    public async Task Get_assets_rejects_invalid_paging_with_400(string query)
    {
        var response = await database.CreateClient().GetAsync($"/api/assets?{query}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
