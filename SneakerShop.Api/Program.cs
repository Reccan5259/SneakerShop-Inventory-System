using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SneakerShop.Api.Data;
using SneakerShop.Api.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler =
            ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<SneakerShopDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString(
            "DefaultConnection")));

builder.Services.AddScoped<
    IPasswordHasher<User>,
    PasswordHasher<User>>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowClients", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowClients");

app.UseAuthorization();

app.MapControllers();

/*
 * Automatically applies pending migrations and creates
 * the default administrator account.
 */
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var context =
        services.GetRequiredService<SneakerShopDbContext>();

    await context.Database.MigrateAsync();

    bool adminExists = await context.Users.AnyAsync(
        user => user.Username == "admin");

    if (!adminExists)
    {
        var passwordHasher =
            services.GetRequiredService<IPasswordHasher<User>>();

        var admin = new User
        {
            FullName = "System Administrator",
            Username = "admin",
            Email = "admin@solestock.local",
            Role = "Admin",
            CreatedAt = DateTime.UtcNow
        };

        admin.PasswordHash =
            passwordHasher.HashPassword(admin, "Admin@123");

        context.Users.Add(admin);
        await context.SaveChangesAsync();
    }
}

app.Run();