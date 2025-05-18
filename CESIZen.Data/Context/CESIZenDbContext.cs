using CESIZen.Data.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace CESIZen.Data.Context;

public class CESIZenDbContext : IdentityDbContext<User, IdentityRole<int>, int>
{
    public DbSet<CategoryInformation> CategoriesInformation { get; set; }
    public DbSet<CategoryRelaxingActivity> CategoriesRelaxingActivity { get; set; }
    public DbSet<InformationalArticle> InformationalArticles { get; set; }
    public DbSet<RelaxingActivity> RelaxingActivities { get; set; }

    public CESIZenDbContext(DbContextOptions<CESIZenDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}