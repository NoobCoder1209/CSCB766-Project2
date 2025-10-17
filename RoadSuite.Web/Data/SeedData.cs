using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RoadSuite.Web.Models;

namespace RoadSuite.Web.Data;

public static class SeedData
{
    private static readonly string[] RoleNames =
    {
        "Admin",
        "Moderator",
        "Dealer"
    };

    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await context.Database.MigrateAsync();

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        foreach (var roleName in RoleNames)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        var admin = await EnsureUserAsync(userManager, "admin@roadsuite.local", "Admin!23");
        var moderator = await EnsureUserAsync(userManager, "moderator@roadsuite.local", "Moderator!23");
        var dealer = await EnsureUserAsync(userManager, "dealer@roadsuite.local", "Dealer!23");

        await EnsureRoleAsync(userManager, admin, "Admin");
        await EnsureRoleAsync(userManager, moderator, "Moderator");
        await EnsureRoleAsync(userManager, dealer, "Dealer");

        if (!await context.DealerProfiles.AnyAsync())
        {
            var dealerProfile = new DealerProfile
            {
                Id = Guid.NewGuid(),
                UserId = dealer.Id,
                DealershipName = "Pan-Asia Motors",
                ContactEmail = "dealer@roadsuite.local",
                ContactPhone = "+1-555-0101",
                City = "Seattle"
            };

            context.DealerProfiles.Add(dealerProfile);
            await context.SaveChangesAsync();
        }

        if (!await context.Categories.AnyAsync())
        {
            var categories = new[]
            {
                new Category { Id = Guid.NewGuid(), Name = "Sedan" },
                new Category { Id = Guid.NewGuid(), Name = "SUV" },
                new Category { Id = Guid.NewGuid(), Name = "Hatchback" },
                new Category { Id = Guid.NewGuid(), Name = "Coupe" },
                new Category { Id = Guid.NewGuid(), Name = "Pickup" }
            };

            context.Categories.AddRange(categories);
            await context.SaveChangesAsync();
        }

        var dealerProfileId = await context.DealerProfiles
            .Where(p => p.UserId == dealer.Id)
            .Select(p => p.Id)
            .FirstAsync();

        if (!await context.Cars.AnyAsync())
        {
            var categories = await context.Categories.ToListAsync();
            Guid Cat(string name) => categories.First(c => c.Name == name).Id;

            var cars = new List<Car>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Make = "Toyota",
                    Model = "Camry",
                    Year = 2022,
                    Price = 28999,
                    Description = "Reliable midsize sedan with advanced safety tech.",
                    CategoryId = Cat("Sedan"),
                    DealerProfileId = dealerProfileId,
                    Status = CarStatus.Approved,
                    CreatedUtc = DateTime.UtcNow.AddDays(-30)
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Make = "Honda",
                    Model = "Civic",
                    Year = 2023,
                    Price = 24999,
                    Description = "Sporty hatchback with excellent fuel economy.",
                    CategoryId = Cat("Hatchback"),
                    DealerProfileId = dealerProfileId,
                    Status = CarStatus.Approved,
                    CreatedUtc = DateTime.UtcNow.AddDays(-20)
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Make = "Nissan",
                    Model = "Ariya",
                    Year = 2023,
                    Price = 45999,
                    Description = "Electric crossover with premium interior and long range.",
                    CategoryId = Cat("SUV"),
                    DealerProfileId = dealerProfileId,
                    Status = CarStatus.Pending,
                    CreatedUtc = DateTime.UtcNow.AddDays(-10)
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Make = "Hyundai",
                    Model = "Ioniq 5",
                    Year = 2024,
                    Price = 47999,
                    Description = "All-electric crossover with ultra-fast charging.",
                    CategoryId = Cat("SUV"),
                    DealerProfileId = dealerProfileId,
                    Status = CarStatus.Pending,
                    CreatedUtc = DateTime.UtcNow.AddDays(-5)
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Make = "Mazda",
                    Model = "MX-5 Miata",
                    Year = 2021,
                    Price = 31999,
                    Description = "Lightweight roadster delivering pure driving joy.",
                    CategoryId = Cat("Coupe"),
                    DealerProfileId = dealerProfileId,
                    Status = CarStatus.Approved,
                    CreatedUtc = DateTime.UtcNow.AddDays(-60)
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Make = "Lexus",
                    Model = "RX 350",
                    Year = 2022,
                    Price = 52999,
                    Description = "Luxury SUV with refined ride and advanced features.",
                    CategoryId = Cat("SUV"),
                    DealerProfileId = dealerProfileId,
                    Status = CarStatus.Approved,
                    CreatedUtc = DateTime.UtcNow.AddDays(-45)
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Make = "Kia",
                    Model = "Telluride",
                    Year = 2023,
                    Price = 38999,
                    Description = "Spacious SUV with upscale amenities for families.",
                    CategoryId = Cat("SUV"),
                    DealerProfileId = dealerProfileId,
                    Status = CarStatus.Approved,
                    CreatedUtc = DateTime.UtcNow.AddDays(-15)
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Make = "Subaru",
                    Model = "Outback",
                    Year = 2022,
                    Price = 34999,
                    Description = "Adventure-ready wagon with standard AWD.",
                    CategoryId = Cat("SUV"),
                    DealerProfileId = dealerProfileId,
                    Status = CarStatus.Approved,
                    CreatedUtc = DateTime.UtcNow.AddDays(-25)
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Make = "Mitsubishi",
                    Model = "Outlander PHEV",
                    Year = 2023,
                    Price = 40999,
                    Description = "Plug-in hybrid SUV with versatile seating.",
                    CategoryId = Cat("SUV"),
                    DealerProfileId = dealerProfileId,
                    Status = CarStatus.Pending,
                    CreatedUtc = DateTime.UtcNow.AddDays(-7)
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Make = "Suzuki",
                    Model = "Jimny",
                    Year = 2021,
                    Price = 27999,
                    Description = "Compact off-roader with legendary capability.",
                    CategoryId = Cat("SUV"),
                    DealerProfileId = dealerProfileId,
                    Status = CarStatus.Rejected,
                    CreatedUtc = DateTime.UtcNow.AddDays(-90)
                }
            };

            context.Cars.AddRange(cars);
            await context.SaveChangesAsync();
        }
    }

    private static async Task<ApplicationUser> EnsureUserAsync(UserManager<ApplicationUser> userManager, string email, string password)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user != null)
        {
            return user;
        }

        user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException($"Failed to create user {email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }

        return user;
    }

    private static async Task EnsureRoleAsync(UserManager<ApplicationUser> userManager, ApplicationUser user, string role)
    {
        if (!await userManager.IsInRoleAsync(user, role))
        {
            await userManager.AddToRoleAsync(user, role);
        }
    }
}
