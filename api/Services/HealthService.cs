using Microsoft.EntityFrameworkCore;
using SitesLend.Api.Data;
using SitesLend.Api.Dtos;

namespace SitesLend.Api.Services;

public class HealthService(AppDbContext db, ILogger<HealthService> logger) : IHealthService
{
    private static readonly TimeSpan DatabaseTimeout = TimeSpan.FromSeconds(5);

    public async Task<HealthResponse> CheckAsync(CancellationToken cancellationToken)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(DatabaseTimeout);

        var databaseUp = false;
        try
        {
            // Open the connection directly: CanConnectAsync swallows the real error.
            var connection = db.Database.GetDbConnection();
            await connection.OpenAsync(timeout.Token);
            await connection.CloseAsync();
            databaseUp = true;
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
        {
            // The reason goes to the server log only, never to the response.
            logger.LogWarning(ex, "Database health check failed");
        }

        return new HealthResponse("ok", databaseUp ? "up" : "down");
    }
}
