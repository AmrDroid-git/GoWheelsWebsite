using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using GoWheels.Models;
using GoWheels.Services.Interfaces;

namespace GoWheels.Data
{
    public class DbInitializer
    {
        private readonly ILogger<DbInitializer> _logger;

        public DbInitializer(ILogger<DbInitializer> logger)
        {
            _logger = logger;
        }

        public static async Task DropAndMigrateDatabaseAsync(IServiceProvider services)
        {
            var context = services.GetRequiredService<GoWheelsDbContext>();
            await context.Database.EnsureDeletedAsync();
            await context.Database.MigrateAsync();
        }

        public async Task SeedAsync(IServiceProvider services)
        {
            _logger.LogInformation("Seeding database...");
            
            await SeedJsonDataAsync(services);
            
            _logger.LogInformation("Database seeding completed.");
        }

        private async Task SeedJsonDataAsync(IServiceProvider services)
        {
            var context = services.GetRequiredService<GoWheelsDbContext>();
            var ratingsService = services.GetRequiredService<IRatingsService>();
            
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var basePath = Path.Combine("Data", "Seed");

            // Fast BUT Unsecure Operations: Enabled
            context.ChangeTracker.AutoDetectChangesEnabled = false;

            // 1. Seed Users
            var seededUsers = await SeedTable("users.json", context.Users, basePath, options, services);
            var seededUserIds = new HashSet<string>(seededUsers.Select(u => u.Id));

            // 2. Seed Posts
            var seededPosts = await SeedTable("posts_clean.json", context.Posts, basePath, options, services, 
                p => seededUserIds.Contains(p.OwnerId));
            var seededPostIds = new HashSet<string>(seededPosts.Select(p => p.Id));

            // 3. Seed Dependent Data
            await SeedTable("post_images.json", context.PostImages, basePath, options, services,
                pi => seededPostIds.Contains(pi.PostId));
            
            await SeedTable("ratings_posts.json", context.PostsRatings, basePath, options, services,
                r => seededUserIds.Contains(r.OwnerId) && seededPostIds.Contains(r.RatedPostId));
            
            await SeedTable("comments_seed.json", context.Comments, basePath, options, services,
                c => seededUserIds.Contains(c.UserId) && seededPostIds.Contains(c.PostId));

            await context.SaveChangesAsync();
            
            // Fast BUT Unsecure Operations: Disabled
            context.ChangeTracker.AutoDetectChangesEnabled = true;
            
            await ratingsService.RecalculateAllPostsRateAverageAsync();
            await ratingsService.RecalculateAllUsersRateAverageAsync();
        }

        private async Task<List<T>> SeedTable<T>(string fileName, DbSet<T> dbSet, string basePath, JsonSerializerOptions options, IServiceProvider services, Func<T, bool>? filter = null) where T : class
        {
            var context = services.GetRequiredService<GoWheelsDbContext>();
            var passwordHasher = services.GetRequiredService<IPasswordHasher<ApplicationUser>>();
            
            
            var path = Path.Combine(basePath, fileName);
            var seededData = new List<T>();
            
            if (File.Exists(path))
            {
                using var stream = File.OpenRead(path);
                var asyncData = JsonSerializer.DeserializeAsyncEnumerable<T>(stream, options);

                await foreach (var item in asyncData)
                {
                    if (item == null) continue;
                    if (filter != null && !filter(item)) continue;
                    
                    seededData.Add(item);
                }

                if (seededData.Any())
                {
                    if (typeof(T) == typeof(ApplicationUser))
                    {
                        foreach (var u in seededData.Cast<ApplicationUser>())
                        {
                            u.PasswordHash = passwordHasher.HashPassword(u, "Password123!");
                        }
                    }
                    await dbSet.AddRangeAsync(seededData);
                    if (typeof(T) == typeof(ApplicationUser))
                    {
                        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
                        var roles = new[] { "ADMIN", "EXPERT", "USER" };
                        foreach (var role in roles)
                        {
                            if (!await roleManager.RoleExistsAsync(role))
                            {
                                await roleManager.CreateAsync(new IdentityRole(role));
                            }
                        }
                        var rolesMap = await roleManager.Roles.ToDictionaryAsync(r => r.Name!, r => r.Id);
                        var userRolesToAdd = seededData
                            .Cast<ApplicationUser>()
                            .Select(x => new IdentityUserRole<string>
                            {
                                UserId = x.Id,
                                RoleId = rolesMap[x.PhoneNumber switch
                                {
                                    "99999999" => "ADMIN",
                                    "99999998" => "EXPERT",
                                    _ => "USER"
                                }]
                            });
                        await context.UserRoles.AddRangeAsync(userRolesToAdd);
                        await context.SaveChangesAsync();
                    }
                }
            }
            return seededData;
        }
    }
}
