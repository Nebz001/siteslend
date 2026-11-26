using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using SitesLend.Api.Data;
using SitesLend.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Enums go over the wire as readable text ("Available"), matching how they are stored.
builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// The connection string lives in user-secrets (see README), never in a committed file.
// It is read lazily so tests can override it.
builder.Services.AddDbContext<AppDbContext>((sp, options) =>
{
    var connectionString = sp.GetRequiredService<IConfiguration>().GetConnectionString("DefaultConnection");
    options.UseNpgsql(connectionString ?? string.Empty);
});
builder.Services.AddScoped<IHealthService, HealthService>();
builder.Services.AddScoped<IAssetService, AssetService>();
builder.Services.AddScoped<DbSeeder>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    // Sample data for local work only. Runs after startup so a down database never blocks the API.
    app.Lifetime.ApplicationStarted.Register(() => _ = DbSeeder.RunSafelyAsync(app.Services));
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

// Lets the test project use WebApplicationFactory<Program>.
public partial class Program;
