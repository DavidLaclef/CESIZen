using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using CESIZen.Data.Context;
using CESIZen.Data.DataSeed;
using CESIZen.Data.Entities;
using CESIZen.UI.Components;

var builder = WebApplication.CreateBuilder(args);
var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddHttpClient();
builder.Services.AddDbContextFactory<CESIZenDbContext>(options =>
    options.UseSqlServer(connectionString));

// Configuration Antiforgery
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    options.FormFieldName = "__RequestVerificationToken";

    if (builder.Environment.IsDevelopment())
    {
        options.SuppressXFrameOptionsHeader = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.None;
    }
});

// Configuration d'Identity
builder.Services.AddDefaultIdentity<User>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
    options.Password.RequiredUniqueChars = 1;

    options.User.RequireUniqueEmail = true;

    // Options de verrouillage de compte (optionnel)
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // Options de connexion (optionnel)
    options.SignIn.RequireConfirmedAccount = false;
})
    .AddRoles<IdentityRole<int>>()
    .AddEntityFrameworkStores<CESIZenDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Administrateur"));
});

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, ServerAuthenticationStateProvider>();

// Ajouter le HttpContextAccessor pour permettre aux composants d'accéder à HttpContext
builder.Services.AddHttpContextAccessor();

builder.Services.AddQuickGridEntityFrameworkAdapter();
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Migration avec retry logic - SEULEMENT en production ou développement
if (!app.Environment.IsEnvironment("Testing"))
{
    await MigrateDatabaseWithRetry(app);
    await InitializeRolesWithRetry(app);
    await CreateAdminUserWithRetry(app);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
else
{
    app.UseMigrationsEndPoint();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

// Méthode pour la migration avec retry
static async Task MigrateDatabaseWithRetry(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<CESIZenDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    var maxRetries = 30;
    var delay = TimeSpan.FromSeconds(2);

    for (int i = 0; i < maxRetries; i++)
    {
        try
        {
            logger.LogInformation("Database migration attempt {Attempt}...", i + 1);

            // Vérifier si c'est une base de données relationnelle
            if (dbContext.Database.IsRelational())
            {
                await dbContext.Database.MigrateAsync();
                logger.LogInformation("Database migration completed successfully.");
            }
            else
            {
                // Pour InMemory ou autres providers non-relationnels
                await dbContext.Database.EnsureCreatedAsync();
                logger.LogInformation("Database creation completed successfully.");
            }
            return;
        }
        catch (Exception ex)
        {
            logger.LogWarning("Database migration attempt {Attempt} failed: {Error}", i + 1, ex.Message);

            if (i == maxRetries - 1)
            {
                logger.LogError(ex, "All database migration attempts failed. Throwing exception.");
                throw;
            }

            logger.LogInformation("Waiting {Delay} seconds before retry...", delay.TotalSeconds);
            await Task.Delay(delay);
        }
    }
}

// Méthode pour l'initialisation des rôles avec retry
static async Task InitializeRolesWithRetry(WebApplication app)
{
    var maxRetries = 5;
    var delay = TimeSpan.FromSeconds(1);

    for (int i = 0; i < maxRetries; i++)
    {
        try
        {
            using var scope = app.Services.CreateScope();
            var services = scope.ServiceProvider;
            var logger = services.GetRequiredService<ILogger<Program>>();

            logger.LogInformation("Roles initialization attempt {Attempt}...", i + 1);
            await SeedRoles.InitializeRoles(services);
            logger.LogInformation("Roles initialization completed successfully.");
            return;
        }
        catch (Exception ex)
        {
            using var scope = app.Services.CreateScope();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            logger.LogWarning("Roles initialization attempt {Attempt} failed: {Error}", i + 1, ex.Message);

            if (i == maxRetries - 1)
            {
                logger.LogError(ex, "All roles initialization attempts failed.");
                throw;
            }

            await Task.Delay(delay);
        }
    }
}

// Méthode pour la création de l'utilisateur admin avec retry
static async Task CreateAdminUserWithRetry(WebApplication app)
{
    var maxRetries = 5;
    var delay = TimeSpan.FromSeconds(1);

    for (int i = 0; i < maxRetries; i++)
    {
        try
        {
            using var scope = app.Services.CreateScope();
            var services = scope.ServiceProvider;
            var userManager = services.GetRequiredService<UserManager<User>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole<int>>>();
            var logger = services.GetRequiredService<ILogger<Program>>();

            logger.LogInformation("Admin user creation attempt {Attempt}...", i + 1);

            var existingUser = await userManager.FindByEmailAsync("admin@admin.com");

            if (existingUser == null)
            {
                var testUser = new User
                {
                    UserName = "Admin",
                    Email = "admin@admin.com",
                    Name = "Admin",
                    LastName = "Admin",
                    Pseudo = "Admin",
                    EmailConfirmed = true,
                    IsAccountActivated = true
                };

                var result = await userManager.CreateAsync(testUser, "P@ssw0rd123");

                if (result.Succeeded)
                {
                    if (!await roleManager.RoleExistsAsync("Administrateur"))
                    {
                        await roleManager.CreateAsync(new IdentityRole<int>("Administrateur"));
                    }
                    var roleResult = await userManager.AddToRoleAsync(testUser, "Administrateur");

                    if (roleResult.Succeeded)
                    {
                        logger.LogInformation("Admin user created and role assigned successfully.");
                    }
                    else
                    {
                        logger.LogWarning("Admin user created but role assignment failed: {Errors}",
                            string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                    }
                }
                else
                {
                    logger.LogWarning("Admin user creation failed: {Errors}",
                        string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
            else
            {
                if (!await userManager.IsInRoleAsync(existingUser, "Administrateur"))
                {
                    var roleResult = await userManager.AddToRoleAsync(existingUser, "Administrateur");

                    if (roleResult.Succeeded)
                    {
                        logger.LogInformation("Rôle Administrateur attribué à l'utilisateur Admin existant");
                    }
                    else
                    {
                        logger.LogWarning("Échec de l'attribution du rôle Administrateur à l'utilisateur existant: {Errors}",
                            string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                    }
                }
                else
                {
                    logger.LogInformation("Admin user already exists with correct role.");
                }
            }

            logger.LogInformation("Admin user setup completed successfully.");
            return;
        }
        catch (Exception ex)
        {
            using var scope = app.Services.CreateScope();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            logger.LogWarning("Admin user creation attempt {Attempt} failed: {Error}", i + 1, ex.Message);

            if (i == maxRetries - 1)
            {
                logger.LogError(ex, "All admin user creation attempts failed.");
                throw;
            }

            await Task.Delay(delay);
        }
    }
}