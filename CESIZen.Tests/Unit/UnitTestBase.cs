using CESIZen.Data.Context;
using CESIZen.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CESIZen.Tests.Unit
{
    /// <summary>
    /// Classe de base pour les tests unitaires qui ont besoin d'un DbContext
    /// </summary>
    [TestClass]
    public abstract class UnitTestBase
    {
        protected CESIZenDbContext DbContext { get; private set; } = null!;
        public TestContext TestContext { get; set; } = null!;

        [TestInitialize]
        public virtual async Task Setup()
        {
            // Créer un DbContext en mémoire pour chaque test
            var options = new DbContextOptionsBuilder<CESIZenDbContext>()
                .UseInMemoryDatabase($"UnitTestDb_{Guid.NewGuid()}")
                .Options;

            DbContext = new CESIZenDbContext(options);
            await DbContext.Database.EnsureCreatedAsync();

            Console.WriteLine($"🧪 Test: {TestContext.TestName}");
        }

        [TestCleanup]
        public virtual void Cleanup()
        {
            DbContext?.Database.EnsureDeleted();
            DbContext?.Dispose();
            Console.WriteLine($"✅ Fin: {TestContext.TestName}");
        }

        /// <summary>
        /// Ajoute des données de test de base
        /// </summary>
        protected async Task SeedBasicData()
        {
            var categories = new List<CategoryInformation>
            {
                new() { Id = 1, Name = "Test Category 1" },
                new() { Id = 2, Name = "Test Category 2" }
            };

            var activityCategories = new List<CategoryRelaxingActivity>
            {
                new() { Id = 1, Name = "Test Activity Category 1" },
                new() { Id = 2, Name = "Test Activity Category 2" }
            };

            DbContext.CategoriesInformation.AddRange(categories);
            DbContext.CategoriesRelaxingActivity.AddRange(activityCategories);
            await DbContext.SaveChangesAsync();
        }
    }
}