using CESIZen.Data.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace CESIZen.Tests.Integration.Database;

[TestClass]
public class ArticleRepositoryIntegrationTests : IntegrationTestBase
{
    [TestMethod]
    [TestCategory("Integration")]
    [TestCategory("Database")]
    public async Task CreateArticle_ShouldPersistInDatabase()
    {
        // Arrange
        var article = new InformationalArticle
        {
            Title = "Article d'intégration",
            Content = "Contenu de test pour l'intégration avec la base de données",
            CategoryId = 1 // Catégorie "Bien-être" créée dans SeedTestData
        };

        // Act
        DbContext.InformationalArticles.Add(article);
        await DbContext.SaveChangesAsync();

        // Assert
        var savedArticle = await DbContext.InformationalArticles
            .Include(a => a.Category)
            .FirstOrDefaultAsync(a => a.Title == "Article d'intégration");

        savedArticle.Should().NotBeNull();
        savedArticle!.Title.Should().Be("Article d'intégration");
        savedArticle.Content.Should().Be("Contenu de test pour l'intégration avec la base de données");
        savedArticle.CategoryId.Should().Be(1);
        savedArticle.Category.Should().NotBeNull();
        savedArticle.Category!.Name.Should().Be("Bien-être");
        savedArticle.Id.Should().BeGreaterThan(0);
    }

    [TestMethod]
    [TestCategory("Integration")]
    [TestCategory("Database")]
    public async Task UpdateArticle_ShouldModifyInDatabase()
    {
        // Arrange - Créer un article
        var article = new InformationalArticle
        {
            Title = "Article original",
            Content = "Contenu original",
            CategoryId = 1
        };

        DbContext.InformationalArticles.Add(article);
        await DbContext.SaveChangesAsync();
        var articleId = article.Id;

        // Act - Modifier l'article
        await ExecuteInScope(async context =>
        {
            var articleToUpdate = await context.InformationalArticles
                .FirstOrDefaultAsync(a => a.Id == articleId);

            articleToUpdate!.Title = "Article modifié";
            articleToUpdate.Content = "Contenu modifié";
            articleToUpdate.CategoryId = 2; // Changer vers "Méditation"

            await context.SaveChangesAsync();
        });

        // Assert - Vérifier les modifications
        var updatedArticle = await ExecuteInScope(async context =>
            await context.InformationalArticles
                .Include(a => a.Category)
                .FirstOrDefaultAsync(a => a.Id == articleId));

        updatedArticle.Should().NotBeNull();
        updatedArticle!.Title.Should().Be("Article modifié");
        updatedArticle.Content.Should().Be("Contenu modifié");
        updatedArticle.CategoryId.Should().Be(2);
        updatedArticle.Category!.Name.Should().Be("Méditation");
    }

    [TestMethod]
    [TestCategory("Integration")]
    [TestCategory("Database")]
    public async Task DeleteArticle_ShouldRemoveFromDatabase()
    {
        // Arrange - Créer un article
        var article = new InformationalArticle
        {
            Title = "Article à supprimer",
            Content = "Contenu à supprimer",
            CategoryId = 1
        };

        DbContext.InformationalArticles.Add(article);
        await DbContext.SaveChangesAsync();
        var articleId = article.Id;

        // Act - Supprimer l'article
        await ExecuteInScope(async context =>
        {
            var articleToDelete = await context.InformationalArticles
                .FirstOrDefaultAsync(a => a.Id == articleId);

            if (articleToDelete != null)
            {
                context.InformationalArticles.Remove(articleToDelete);
                await context.SaveChangesAsync();
            }
        });

        // Assert - Vérifier la suppression
        var deletedArticle = await ExecuteInScope(async context =>
            await context.InformationalArticles
                .FirstOrDefaultAsync(a => a.Id == articleId));

        deletedArticle.Should().BeNull();
    }

    [TestMethod]
    [TestCategory("Integration")]
    [TestCategory("Database")]
    public async Task GetArticlesByCategory_ShouldReturnFilteredResults()
    {
        // Arrange - Créer plusieurs articles dans différentes catégories
        var articles = new List<InformationalArticle>
        {
            new() { Title = "Article Bien-être 1", Content = "Contenu 1", CategoryId = 1 },
            new() { Title = "Article Bien-être 2", Content = "Contenu 2", CategoryId = 1 },
            new() { Title = "Article Méditation", Content = "Contenu 3", CategoryId = 2 },
            new() { Title = "Article Sport", Content = "Contenu 4", CategoryId = 3 }
        };

        DbContext.InformationalArticles.AddRange(articles);
        await DbContext.SaveChangesAsync();

        // Act - Récupérer les articles de la catégorie "Bien-être"
        var bienEtreArticles = await DbContext.InformationalArticles
            .Where(a => a.CategoryId == 1)
            .Include(a => a.Category)
            .ToListAsync();

        // Assert
        bienEtreArticles.Should().HaveCount(2);
        bienEtreArticles.Should().OnlyContain(a => a.CategoryId == 1);
        bienEtreArticles.Should().OnlyContain(a => a.Category!.Name == "Bien-être");
        bienEtreArticles.Select(a => a.Title).Should().Contain("Article Bien-être 1", "Article Bien-être 2");
    }

    [TestMethod]
    [TestCategory("Integration")]
    [TestCategory("Database")]
    public async Task GetArticlesOrderByDate_ShouldReturnSortedResults()
    {
        // Arrange - Créer des articles avec des dates différentes
        var now = DateTime.Now;
        var articles = new List<InformationalArticle>
        {
            new() { Title = "Article Récent", Content = "Contenu", CategoryId = 1, CreationDate = now },
            new() { Title = "Article Ancien", Content = "Contenu", CategoryId = 1, CreationDate = now.AddDays(-5) },
            new() { Title = "Article Moyen", Content = "Contenu", CategoryId = 1, CreationDate = now.AddDays(-2) }
        };

        DbContext.InformationalArticles.AddRange(articles);
        await DbContext.SaveChangesAsync();

        // Act - Récupérer les articles triés par date (plus récent d'abord)
        var sortedArticles = await DbContext.InformationalArticles
            .OrderByDescending(a => a.CreationDate)
            .ToListAsync();

        // Assert
        sortedArticles.Should().HaveCount(3);
        sortedArticles[0].Title.Should().Be("Article Récent");
        sortedArticles[1].Title.Should().Be("Article Moyen");
        sortedArticles[2].Title.Should().Be("Article Ancien");
    }
}