using Microsoft.EntityFrameworkCore;
using SitesLend.Api.Entities;

namespace SitesLend.Api.Data;

/// <summary>Development sample data. Synthetic only. Program.cs calls it in Development only.</summary>
public class DbSeeder(AppDbContext db)
{
    /// <summary>Adds the sample data, but only when both tables are empty.</summary>
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (await db.AssetCategories.AnyAsync(cancellationToken) || await db.AssetUnits.AnyAsync(cancellationToken))
        {
            return;
        }

        var laptops = new AssetCategory { Name = "Laptops", Description = "Portable computers for staff and trainees." };
        var projectors = new AssetCategory { Name = "Projectors", Description = "Meeting room and training projectors." };
        var monitors = new AssetCategory { Name = "Monitors", Description = "External display screens." };
        var accessories = new AssetCategory { Name = "Accessories", Description = "Cables, adapters and peripherals." };
        db.AssetCategories.AddRange(laptops, projectors, monitors, accessories);

        db.AssetUnits.AddRange(
            Unit("LAP-0001", laptops, "14-inch demo laptop", AssetCondition.Good, AssetState.Available),
            Unit("LAP-0002", laptops, "14-inch demo laptop", AssetCondition.Good, AssetState.Assigned),
            Unit("LAP-0003", laptops, "15-inch demo laptop", AssetCondition.Fair, AssetState.Reserved),
            Unit("LAP-0004", laptops, "15-inch demo laptop, cracked hinge", AssetCondition.Damaged, AssetState.UnderRepair),
            Unit("PRJ-0001", projectors, "Short-throw demo projector", AssetCondition.Good, AssetState.Available),
            Unit("PRJ-0002", projectors, "Demo projector, old lamp", AssetCondition.Fair, AssetState.Archived),
            Unit("MON-0001", monitors, "24-inch demo monitor", AssetCondition.Good, AssetState.Available),
            Unit("MON-0002", monitors, "24-inch demo monitor", AssetCondition.Good, AssetState.Assigned),
            Unit("MON-0003", monitors, "27-inch demo monitor, dead pixels", AssetCondition.Damaged, AssetState.UnderRepair),
            Unit("ACC-0001", accessories, "USB-C docking station", AssetCondition.Good, AssetState.Available),
            Unit("ACC-0002", accessories, "HDMI cable, 3 m", AssetCondition.Fair, AssetState.Available),
            Unit("ACC-0003", accessories, "Wireless presenter remote", AssetCondition.Good, AssetState.Reserved));

        await db.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Runs the seeder from a new scope and never throws. If the database is down or not
    /// migrated yet, the API still starts and the reason goes to the log.
    /// Refuses to run unless the environment is Development, so Testing and Production are never seeded.
    /// </summary>
    public static async Task RunSafelyAsync(IServiceProvider services)
    {
        if (!services.GetRequiredService<IHostEnvironment>().IsDevelopment())
        {
            services.GetRequiredService<ILogger<DbSeeder>>()
                .LogWarning("Seeding refused: the environment is not Development.");
            return;
        }

        try
        {
            using var scope = services.CreateScope();
            await scope.ServiceProvider.GetRequiredService<DbSeeder>().SeedAsync();
        }
        catch (Exception ex)
        {
            services.GetRequiredService<ILogger<DbSeeder>>()
                .LogWarning(ex, "Development seeding skipped (is the database migrated?)");
        }
    }

    private static AssetUnit Unit(string tag, AssetCategory category, string description, AssetCondition condition, AssetState state)
        => new()
        {
            AssetTag = tag,
            Category = category,
            Description = description,
            Condition = condition,
            State = state,
        };
}
