namespace SitesLend.Api.Entities;

public class AssetUnit
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    public required string AssetTag { get; set; }

    public Guid CategoryId { get; set; }

    public AssetCategory Category { get; set; } = null!;

    public string? Description { get; set; }

    public AssetCondition Condition { get; set; } = AssetCondition.Good;

    public AssetState State { get; set; } = AssetState.Available;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>PostgreSQL xmin system column. Concurrency token only, never sent to clients.</summary>
    public uint Version { get; set; }
}
