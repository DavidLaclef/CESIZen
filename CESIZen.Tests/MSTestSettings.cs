namespace CESIZen.Tests;

/// <summary>
/// Configuration globale pour les tests MSTest
/// </summary>
[TestClass]
public static class MSTestSettings
{
    /// <summary>
    /// Initialisation une seule fois pour tous les tests
    /// </summary>
    /// <param name="context">Contexte de test</param>
    [AssemblyInitialize]
    public static void AssemblyInitialize(TestContext context)
    {
        // Configuration globale pour tous les tests
        Console.WriteLine("🚀 Initialisation des tests CESIZen");
        Console.WriteLine($"📅 Date d'exécution: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        Console.WriteLine($"🖥️ Machine: {Environment.MachineName}");
        Console.WriteLine($"👤 Utilisateur: {Environment.UserName}");
        Console.WriteLine($"📂 Répertoire: {context.TestRunDirectory}");
    }

    /// <summary>
    /// Nettoyage final après tous les tests
    /// </summary>
    [AssemblyCleanup]
    public static void AssemblyCleanup()
    {
        Console.WriteLine("✅ Nettoyage final des tests CESIZen terminé");
    }
}

/// <summary>
/// Classe de base pour tous les tests unitaires
/// </summary>
[TestClass]
public abstract class UnitTestBase
{
    /// <summary>
    /// Contexte de test MSTest
    /// </summary>
    public TestContext TestContext { get; set; } = null!;

    /// <summary>
    /// Initialisation avant chaque test
    /// </summary>
    [TestInitialize]
    public virtual void Setup()
    {
        Console.WriteLine($"🧪 Début du test: {TestContext.TestName}");
    }

    /// <summary>
    /// Nettoyage après chaque test
    /// </summary>
    [TestCleanup]
    public virtual void Cleanup()
    {
        Console.WriteLine($"✅ Fin du test: {TestContext.TestName} - Résultat: {TestContext.CurrentTestOutcome}");
    }
}