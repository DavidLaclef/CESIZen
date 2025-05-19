﻿using Microsoft.AspNetCore.Components.Authorization;
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

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await SeedRoles.InitializeRoles(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Une erreur s'est produite lors de l'initialisation des rôles.");
    }
}

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var userManager = services.GetRequiredService<UserManager<User>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole<int>>>();
        var logger = services.GetRequiredService<ILogger<Program>>();

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
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Une erreur s'est produite lors de la création/mise à jour de l'utilisateur Admin.");
    }
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