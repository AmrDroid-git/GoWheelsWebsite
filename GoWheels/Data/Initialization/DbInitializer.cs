using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using GoWheels.Models;
using GoWheels.Data;

namespace GoWheels.Data.Initialization
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var context = services.GetRequiredService<GoWheelsDbContext>();
            
            await SeedRolesAndDefaultUsersAsync(context, roleManager, services);
            await SeedJsonDataAsync(context);
        }

        public static async Task DropAndMigrateDatabaseAsync(IServiceProvider services)
        {
            var context = services.GetRequiredService<GoWheelsDbContext>();
            
            // WARNING: This will drop the database!
            // await context.Database.EnsureDeletedAsync();
            
            // This will apply any pending migrations
            await context.Database.MigrateAsync();
        }

        private static async Task SeedRolesAndDefaultUsersAsync(GoWheelsDbContext context, RoleManager<IdentityRole> roleManager, IServiceProvider services)
        {
            // 1. Hardcoded Roles
            var roles = new[] { "ADMIN", "EXPERT", "USER" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // Fetch roles into memory for a quick lookup when assigning to users
            var rolesMap = await roleManager.Roles.ToDictionaryAsync(r => r.Name!, r => r.Id);

            // 2. Hardcoded Default Users
            var defaultUsers = new List<(string UserName, string Email, string Role)>
            {
                ("admin", "admin@gowheels.local", "ADMIN"),
                ("expert", "expert@gowheels.local", "EXPERT"),
                ("user", "user@gowheels.local", "USER")
            };

            var passwordHasher = services.GetRequiredService<IPasswordHasher<ApplicationUser>>();
            var usersToCreate = new List<(ApplicationUser User, string Role)>();

            foreach (var seed in defaultUsers)
            {
                var existingUser = await context.Users.FirstOrDefaultAsync(u => u.UserName == seed.UserName);
                if (existingUser == null)
                {
                    var user = new ApplicationUser
                    {
                        UserName = seed.UserName,
                        NormalizedUserName = seed.UserName.ToUpper(),
                        Email = seed.Email,
                        NormalizedEmail = seed.Email.ToUpper(),
                        EmailConfirmed = true,
                        PhoneNumber = "98756683",
                        Address = "CUN",
                        SecurityStamp = Guid.NewGuid().ToString()
                    };
                    user.PasswordHash = passwordHasher.HashPassword(user, "Password123!");
                    usersToCreate.Add((user, seed.Role));
                }
            }

            if (usersToCreate.Any())
            {
                await context.Users.AddRangeAsync(usersToCreate.Select(x => x.User));
                await context.SaveChangesAsync();

                var userRolesToAdd = usersToCreate
                    .Where(x => rolesMap.ContainsKey(x.Role))
                    .Select(x => new IdentityUserRole<string>
                    {
                        UserId = x.User.Id,
                        RoleId = rolesMap[x.Role]
                    });
                await context.UserRoles.AddRangeAsync(userRolesToAdd);
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedJsonDataAsync(GoWheelsDbContext context)
        {
            // 3. Feed JSON Seed (Manual fallback if not seeded by HasData)
            if (!await context.Posts.AnyAsync())
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var basePath = Path.Combine("Data", "Seed");

                // Disable change tracking for high-performance bulk insertion
                context.ChangeTracker.AutoDetectChangesEnabled = false;

                await SeedTable("users.json", context.Users, basePath, options, context);
                await SeedTable("posts_clean.json", context.Posts, basePath, options, context);
                await SeedTable("post_images.json", context.PostImages, basePath, options, context);
                await SeedTable("ratings_posts.json", context.PostsRatings, basePath, options, context);
                await SeedTable("comments_seed.json", context.Comments, basePath, options, context);

                // One single SaveChanges call for all seeded data
                await context.SaveChangesAsync();
                context.ChangeTracker.AutoDetectChangesEnabled = true;
            }
        }

        private static async Task SeedTable<T>(string fileName, DbSet<T> dbSet, string basePath, JsonSerializerOptions options, GoWheelsDbContext context) where T : class
        {
            var path = Path.Combine(basePath, fileName);
            if (File.Exists(path))
            {
                var data = JsonSerializer.Deserialize<List<T>>(await File.ReadAllTextAsync(path), options);
                if (data != null)
                {
                    // For users, we should handle Normalized fields and SecurityStamp if they are missing in JSON
                    if (typeof(T) == typeof(ApplicationUser))
                    {
                        foreach (var u in data.Cast<ApplicationUser>())
                        {
                            u.NormalizedUserName ??= u.UserName?.ToUpper();
                            u.NormalizedEmail ??= u.Email?.ToUpper();
                            u.SecurityStamp ??= Guid.NewGuid().ToString();
                        }
                    }
                    // Use AddRange for batching; EF Core will optimize this into fewer SQL commands
                    await dbSet.AddRangeAsync(data);
                }
            }
        }
    }

}
