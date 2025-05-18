using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using CESIZen.Data.Context;
using CESIZen.Data.Entities;

var builder = WebApplication.CreateBuilder(args);

var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string"
        + "'DefaultConnection' not found.");

builder.Services.AddDbContext<CESIZenDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddAuthentication(IdentityConstants.ApplicationScheme)
 .AddIdentityCookies();
builder.Services.AddAuthorizationBuilder();

builder.Services.AddIdentityCore<User>()
 .AddEntityFrameworkStores<CESIZenDbContext>()
 .AddApiEndpoints();


// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
