using Microsoft.EntityFrameworkCore;
using SitesLend.Api.Entities;

namespace SitesLend.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<AssetCategory> AssetCategories => Set<AssetCategory>();

    public DbSet<AssetUnit> AssetUnits => Set<AssetUnit>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
        => modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
}
