using SitesLend.Api.Entities;

namespace SitesLend.Api.Dtos;

/// <summary>What the asset list shows. Never carries CategoryId, Version or other internals.</summary>
public record AssetUnitDto(
    Guid Id,
    string AssetTag,
    string CategoryName,
    string? Description,
    AssetCondition Condition,
    AssetState State);
