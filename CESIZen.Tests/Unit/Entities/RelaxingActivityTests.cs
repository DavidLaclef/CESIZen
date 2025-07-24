using CESIZen.Data.Entities;
using CESIZen.Data.Enums;
using FluentAssertions;

namespace CESIZen.Tests.Unit.Entities;

[TestClass]
public class RelaxingActivityTests
{
    [TestMethod]
    [TestCategory("Unit")]
    public void RelaxingActivity_Creation_ShouldSetDefaultValues()
    {
        // Arrange & Act
        var activity = new RelaxingActivity
        {
            Name = "Méditation guidée",
            Description = "Séance de méditation de 10 minutes",
            Duration = 10,
            DifficultyLevel = DifficultyLevel.Begginer,
            CategoryId = 1
        };

        // Assert
        activity.Name.Should().Be("Méditation guidée");
        activity.Description.Should().Be("Séance de méditation de 10 minutes");
        activity.Duration.Should().Be(10);
        activity.DifficultyLevel.Should().Be(DifficultyLevel.Begginer);
        activity.Enabled.Should().BeTrue(); // Valeur par défaut
        activity.CreationDate.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
    }

    [TestMethod]
    [TestCategory("Unit")]
    [DataRow(DifficultyLevel.Begginer)]
    [DataRow(DifficultyLevel.Intermediate)]
    [DataRow(DifficultyLevel.Advanced)]
    public void RelaxingActivity_DifficultyLevel_ShouldAcceptAllValues(DifficultyLevel level)
    {
        // Arrange & Act
        var activity = new RelaxingActivity
        {
            Name = "Test Activity",
            Description = "Test Description",
            Duration = 15,
            DifficultyLevel = level,
            CategoryId = 1
        };

        // Assert
        activity.DifficultyLevel.Should().Be(level);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void RelaxingActivity_EnabledFlag_ShouldBeToggleable()
    {
        // Arrange
        var activity = new RelaxingActivity
        {
            Name = "Test Activity",
            Description = "Test Description",
            Duration = 20,
            CategoryId = 1
        };

        // Act & Assert - Par défaut activé
        activity.Enabled.Should().BeTrue();

        // Act - Désactiver
        activity.Enabled = false;

        // Assert
        activity.Enabled.Should().BeFalse();

        // Act - Réactiver
        activity.Enabled = true;

        // Assert
        activity.Enabled.Should().BeTrue();
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void RelaxingActivity_WithCategory_ShouldHaveValidRelation()
    {
        // Arrange
        var category = new CategoryRelaxingActivity
        {
            Id = 1,
            Name = "Yoga"
        };

        // Act
        var activity = new RelaxingActivity
        {
            Name = "Séance de Yoga",
            Description = "Yoga pour débutants",
            Duration = 30,
            DifficultyLevel = DifficultyLevel.Begginer,
            CategoryId = category.Id,
            Category = category
        };

        // Assert
        activity.CategoryId.Should().Be(category.Id);
        activity.Category.Should().NotBeNull();
        activity.Category.Name.Should().Be("Yoga");
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void RelaxingActivity_Duration_ShouldAcceptPositiveValues()
    {
        // Arrange
        var durations = new[] { 5, 10, 15, 30, 45, 60, 90 };

        foreach (var duration in durations)
        {
            // Act
            var activity = new RelaxingActivity
            {
                Name = $"Activity {duration}min",
                Description = "Test",
                Duration = duration,
                CategoryId = 1
            };

            // Assert
            activity.Duration.Should().Be(duration);
            activity.Duration.Should().BePositive();
        }
    }
}