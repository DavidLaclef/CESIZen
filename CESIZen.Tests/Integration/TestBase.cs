using CESIZen.Data.Context;
using CESIZen.Data.Entities;
using CESIZen.UI.Components;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CESIZen.Tests.Integration
{
    [TestClass]
    public abstract class IntegrationTestBase
    {
        protected WebApplicationFactory<App> Factory { get; private set; } = null!;
        protected HttpClient Client { get; private set; } = null!;
        protected CESIZenDbContext DbContext { get; private set; } = null!;
        protected IServiceScope Scope { get; private set; } = null!;

        [TestInitialize]
        public virtual async Task Setup()
        {
            Factory = new WebApplicationFactory<App>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureServices(services =>
                    {
                        // Supprimer la configuration de base de données existante
                        var descriptor = services.SingleOrDefault(
                            d => d.ServiceType == typeof(DbContextOptions<CESIZenDbContext>));
                        if (descriptor != null)
                        {
                            services.Remove(descriptor);
                        }

                        // Supprimer aussi IDbContextFactory si présent
                        var factoryDescriptor = services.SingleOrDefault(
                            d => d.ServiceType == typeof(IDbContextFactory<CESIZenDbContext>));
                        if (factoryDescriptor != null)
                        {
                            services.Remove(factoryDescriptor);
                        }

                        // Ajouter la base de données en mémoire pour les tests
                        var dbName = $"TestDb_{Guid.NewGuid()}";
                        services.AddDbContext<CESIZenDbContext>(options =>
                        {
                            options.UseInMemoryDatabase(dbName);
                        });

                        // Ajouter le factory pour les tests
                        services.AddDbContextFactory<CESIZenDbContext>(options =>
                        {
                            options.UseInMemoryDatabase(dbName);
                        });
                    });

                    builder.UseEnvironment("Testing");
                });

            Client = Factory.CreateClient();
            Scope = Factory.Services.CreateScope();
            DbContext = Scope.ServiceProvider.GetRequiredService<CESIZenDbContext>();

            // Créer la base de données et ajouter les données de test
            await DbContext.Database.EnsureCreatedAsync();
            await SeedTestData();
        }

        [TestCleanup]
        public virtual void Cleanup()
        {
            DbContext?.Database.EnsureDeleted();
            DbContext?.Dispose();
            Scope?.Dispose();
            Client?.Dispose();
            Factory?.Dispose();
        }

        protected virtual async Task SeedTestData()
        {
            // Vérifier si les données n'existent pas déjà
            if (await DbContext.CategoriesInformation.AnyAsync())
                return;

            // Données de test de base
            var categories = new List<CategoryInformation>
            {
                new() { Id = 1, Name = "Bien-être" },
                new() { Id = 2, Name = "Méditation" },
                new() { Id = 3, Name = "Sport" }
            };

            var categoriesActivities = new List<CategoryRelaxingActivity>
            {
                new() { Id = 1, Name = "Relaxation" },
                new() { Id = 2, Name = "Yoga" },
                new() { Id = 3, Name = "Respiration" }
            };

            DbContext.CategoriesInformation.AddRange(categories);
            DbContext.CategoriesRelaxingActivity.AddRange(categoriesActivities);
            await DbContext.SaveChangesAsync();
        }

        protected async Task<T> ExecuteInScope<T>(Func<CESIZenDbContext, Task<T>> action)
        {
            using var scope = Factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<CESIZenDbContext>();
            return await action(context);
        }

        protected async Task ExecuteInScope(Func<CESIZenDbContext, Task> action)
        {
            using var scope = Factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<CESIZenDbContext>();
            await action(context);
        }
    }
}