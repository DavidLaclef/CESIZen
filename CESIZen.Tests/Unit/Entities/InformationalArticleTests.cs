using Microsoft.VisualStudio.TestTools.UnitTesting;
using CESIZen.Data.Entities;
using FluentAssertions;
using CESIZen.Tests.Unit;

namespace CESIZen.Tests.Unit.Entities
{
    [TestClass]
    public class InformationalArticleTests : UnitTestBase
    {
        [TestMethod]
        [TestCategory("Unit")]
        public void InformationalArticle_Creation_ShouldSetDefaultValues()
        {
            // Arrange & Act
            var article = new InformationalArticle
            {
                Title = "Article de Test",
                Content = "Contenu de test pour l'article",
                CategoryId = 1
            };

            // Assert
            article.Title.Should().Be("Article de Test");
            article.Content.Should().Be("Contenu de test pour l'article");
            article.CategoryId.Should().Be(1);
            article.CreationDate.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void InformationalArticle_Properties_ShouldBeSettable()
        {
            // Arrange
            var article = new InformationalArticle();
            var testDate = new DateTime(2024, 1, 1);

            // Act
            article.Id = 100;
            article.Title = "Titre modifié";
            article.Content = "Contenu modifié";
            article.CategoryId = 5;
            article.CreationDate = testDate;

            // Assert
            article.Id.Should().Be(100);
            article.Title.Should().Be("Titre modifié");
            article.Content.Should().Be("Contenu modifié");
            article.CategoryId.Should().Be(5);
            article.CreationDate.Should().Be(testDate);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void InformationalArticle_DefaultCreationDate_ShouldBeNow()
        {
            // Arrange
            var beforeCreation = DateTime.Now;

            // Act
            var article = new InformationalArticle
            {
                Title = "Test",
                Content = "Test Content",
                CategoryId = 1
            };

            var afterCreation = DateTime.Now;

            // Assert
            article.CreationDate.Should().BeOnOrAfter(beforeCreation);
            article.CreationDate.Should().BeOnOrBefore(afterCreation);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Database")]
        public async Task InformationalArticle_WithCategory_ShouldPersistCorrectly()
        {
            // Arrange - Ajouter des données de test
            await SeedBasicData();

            var article = new InformationalArticle
            {
                Title = "Article avec catégorie",
                Content = "Contenu de test",
                CategoryId = 1
            };

            // Act
            DbContext.InformationalArticles.Add(article);
            await DbContext.SaveChangesAsync();

            // Assert
            var savedArticle = await DbContext.InformationalArticles
                .FindAsync(article.Id);

            savedArticle.Should().NotBeNull();
            savedArticle!.Title.Should().Be("Article avec catégorie");
            savedArticle.CategoryId.Should().Be(1);
        }
    }
}