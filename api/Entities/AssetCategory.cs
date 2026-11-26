namespace SitesLend.Api.Entities;

public class AssetCategory
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    public required string Name { get; set; }

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public List<AssetUnit> Units { get; } = [];
}
