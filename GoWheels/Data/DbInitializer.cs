using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using GoWheels.Models;
using GoWheels.Services.Interfaces;

namespace GoWheels.Data
{
    public class DbInitializer : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DbInitializer> _logger;

        public DbInitializer(IServiceProvider serviceProvider, ILogger<DbInitializer> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Database Initializer Service is starting.");

            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var services = scope.ServiceProvider;
                    _logger.LogInformation("Seeding database...");
                    await SeedAsync(services);
                    _logger.LogInformation("Database seeding completed.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while seeding the database.");
            }
        }

        public static async Task DropAndMigrateDatabaseAsync(IServiceProvider services)
        {
            var context = services.GetRequiredService<GoWheelsDbContext>();
            // await context.Database.EnsureDeletedAsync();
            await context.Database.MigrateAsync();
        }

        private async Task SeedAsync(IServiceProvider services)
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var context = services.GetRequiredService<GoWheelsDbContext>();
            var ratingsService = services.GetRequiredService<IRatingsService>();
            
            await SeedRolesAndDefaultUsersAsync(context, roleManager, services);
            await SeedJsonDataAsync(context);

            await ratingsService.RecalculateAllPostsRateAverageAsync();
            await ratingsService.RecalculateAllUsersRateAverageAsync();
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

        private async Task SeedJsonDataAsync(GoWheelsDbContext context)
        {
            if (!await context.Posts.AnyAsync())
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var basePath = Path.Combine("Data", "Seed");

                context.ChangeTracker.AutoDetectChangesEnabled = false;

                var requiredUserIds = new HashSet<string>();
                var requiredPostIds = new HashSet<string>();

                async Task PeekAndCollectIds<T>(string fileName, int limit, Action<T> collector)
                {
                    var path = Path.Combine(basePath, fileName);
                    if (File.Exists(path))
                    {
                        using var stream = File.OpenRead(path);
                        var asyncData = JsonSerializer.DeserializeAsyncEnumerable<T>(stream, options);
                        int count = 0;
                        await foreach (var item in asyncData)
                        {
                            if (item != null) collector(item);
                            if (++count >= limit) break;
                        }
                    }
                }

                await PeekAndCollectIds<RatingPost>("ratings_posts.json", 10, r => {
                    requiredUserIds.Add(r.OwnerId);
                    requiredPostIds.Add(r.RatedPostId);
                });

                await PeekAndCollectIds<Comment>("comments_seed.json", 10, c => {
                    requiredUserIds.Add(c.UserId);
                    requiredPostIds.Add(c.PostId);
                });

                await PeekAndCollectIds<PostImage>("post_images.json", 10, pi => {
                    requiredPostIds.Add(pi.PostId);
                });

                var ownerIdsForRequiredPosts = new HashSet<string>();
                await PeekAndCollectIds<Post>("posts_clean.json", int.MaxValue, p => {
                    if (requiredPostIds.Contains(p.Id))
                        ownerIdsForRequiredPosts.Add(p.OwnerId);
                });
                foreach (var id in ownerIdsForRequiredPosts) requiredUserIds.Add(id);

                var seededUsers = await SeedTable("users.json", context.Users, basePath, options, context, null, 
                    u => requiredUserIds.Contains(u.Id));
                
                var seededUserIds = new HashSet<string>(seededUsers.Select(u => u.Id));
                var defaultUserIds = await context.Users.Select(u => u.Id).ToListAsync();
                foreach (var id in defaultUserIds) seededUserIds.Add(id);

                var seededPosts = await SeedTable("posts_clean.json", context.Posts, basePath, options, context, null, 
                    p => requiredPostIds.Contains(p.Id) 
                    && seededUserIds.Contains(p.OwnerId)
                    );
                
                var seededPostIds = new HashSet<string>(seededPosts.Select(p => p.Id));

                await SeedTable("post_images.json", context.PostImages, basePath, options, context, 10,
                    pi => seededPostIds.Contains(pi.PostId));
                
                await SeedTable("ratings_posts.json", context.PostsRatings, basePath, options, context, 10,
                    r => seededUserIds.Contains(r.OwnerId) && seededPostIds.Contains(r.RatedPostId));
                
                await SeedTable("comments_seed.json", context.Comments, basePath, options, context, 10,
                    c => seededUserIds.Contains(c.UserId) && seededPostIds.Contains(c.PostId));

                await context.SaveChangesAsync();
                context.ChangeTracker.AutoDetectChangesEnabled = true;
            }
        }

        private async Task<List<T>> SeedTable<T>(string fileName, DbSet<T> dbSet, string basePath, JsonSerializerOptions options, GoWheelsDbContext context, int? maxRecords = null, Func<T, bool>? filter = null) where T : class
        {
            var path = Path.Combine(basePath, fileName);
            var seededData = new List<T>();
            
            if (File.Exists(path))
            {
                using var stream = File.OpenRead(path);
                var asyncData = JsonSerializer.DeserializeAsyncEnumerable<T>(stream, options);
                
                int count = 0;

                await foreach (var item in asyncData)
                {
                    if (item == null) continue;
                    if (filter != null && !filter(item)) continue;
                    
                    seededData.Add(item);
                    count++;

                    if (maxRecords.HasValue && count >= maxRecords.Value)
                        break;
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
