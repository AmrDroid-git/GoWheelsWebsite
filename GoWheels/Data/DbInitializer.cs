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
            
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var context = services.GetRequiredService<GoWheelsDbContext>();
            var ratingsService = services.GetRequiredService<IRatingsService>();
            
            await SeedRolesAndDefaultUsersAsync(context, roleManager, services);
            await SeedJsonDataAsync(context, ratingsService);
            
            _logger.LogInformation("Database seeding completed.");
        }

        private async Task SeedRolesAndDefaultUsersAsync(GoWheelsDbContext context, RoleManager<IdentityRole> roleManager, IServiceProvider services)
        {
            var roles = new[] { "ADMIN", "EXPERT", "USER" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            var rolesMap = await roleManager.Roles.ToDictionaryAsync(r => r.Name!, r => r.Id);

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

        private async Task SeedJsonDataAsync(GoWheelsDbContext context, IRatingsService ratingsService)
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var basePath = Path.Combine("Data", "Seed");

            context.ChangeTracker.AutoDetectChangesEnabled = false;

            // 1. Seed Users
            var seededUsers = await SeedTable("users.json", context.Users, basePath, options, context);
            var seededUserIds = new HashSet<string>(seededUsers.Select(u => u.Id));
            
            // Add default users to the set of available users for referential integrity
            var defaultUserIds = await context.Users.Select(u => u.Id).ToListAsync();
            foreach (var id in defaultUserIds) seededUserIds.Add(id);

            // 2. Seed Posts
            var seededPosts = await SeedTable("posts_clean.json", context.Posts, basePath, options, context, 
                p => seededUserIds.Contains(p.OwnerId));
            var seededPostIds = new HashSet<string>(seededPosts.Select(p => p.Id));

            // 3. Seed Dependent Data
            await SeedTable("post_images.json", context.PostImages, basePath, options, context,
                pi => seededPostIds.Contains(pi.PostId));
            
            await SeedTable("ratings_posts.json", context.PostsRatings, basePath, options, context,
                r => seededUserIds.Contains(r.OwnerId) && seededPostIds.Contains(r.RatedPostId));
            
            await SeedTable("comments_seed.json", context.Comments, basePath, options, context,
                c => seededUserIds.Contains(c.UserId) && seededPostIds.Contains(c.PostId));

            await context.SaveChangesAsync();
            context.ChangeTracker.AutoDetectChangesEnabled = true;
            
            await ratingsService.RecalculateAllPostsRateAverageAsync();
            await ratingsService.RecalculateAllUsersRateAverageAsync();
        }

        private async Task<List<T>> SeedTable<T>(string fileName, DbSet<T> dbSet, string basePath, JsonSerializerOptions options, GoWheelsDbContext context, Func<T, bool>? filter = null) where T : class
        {
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
                            u.NormalizedUserName ??= u.UserName?.ToUpper();
                            u.NormalizedEmail ??= u.Email?.ToUpper();
                            u.SecurityStamp ??= Guid.NewGuid().ToString();
                        }
                    }
                    await dbSet.AddRangeAsync(seededData);
                }
            }
            return seededData;
        }
    }
}
