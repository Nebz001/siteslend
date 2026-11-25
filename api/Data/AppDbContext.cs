using Microsoft.EntityFrameworkCore;

namespace SitesLend.Api.Data;

// Empty on purpose. Entities and DbSets arrive with the first migration task.
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
}
