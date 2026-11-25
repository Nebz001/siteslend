namespace SitesLend.Api.Dtos;

/// <param name="Status">"ok" whenever the API itself is running.</param>
/// <param name="Database">"up" or "down". Never carries error text or connection details.</param>
public record HealthResponse(string Status, string Database);
