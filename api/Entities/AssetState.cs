namespace SitesLend.Api.Entities;

/// <summary>Where an asset unit is in the borrowing cycle. Stored as text.</summary>
public enum AssetState
{
    Available,
    Reserved,
    Assigned,
    UnderRepair,
    Archived,
}
