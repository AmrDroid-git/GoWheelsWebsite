using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using GoWheels.Models;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace GoWheels.Data
{
    public class GoWheelsDbContext : IdentityDbContext<ApplicationUser>
    {
        public GoWheelsDbContext(DbContextOptions<GoWheelsDbContext> options)
            : base(options)
        {
        }

        public DbSet<Post> Posts { get; set; }
        public DbSet<PostImage> PostImages { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<RatingUser> UsersRatings { get; set; }
        public DbSet<RatingPost> PostsRatings { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ----------------------------
            // Specifications (Dictionary → jsonb)
            // ----------------------------
            var dictConverter =
                new ValueConverter<Dictionary<string, string>, string>(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null) ?? "{}",
                    v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, (JsonSerializerOptions?)null)
                         ?? new Dictionary<string, string>()
                );
            var dictComparer = new ValueComparer<Dictionary<string, string>>(
                (c1, c2) => c1 != null && c2 != null ? c1.OrderBy(kv => kv.Key).SequenceEqual(c2.OrderBy(kv => kv.Key)) : c1 == c2, // Logic to check if they are equal
                c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.Key.GetHashCode(), v.Value.GetHashCode())), // Logic to generate hash
                c => c.ToDictionary(entry => entry.Key, entry => entry.Value) // Logic to create a deep copy
            );

            builder.Entity<Post>()
                .Property(p => p.Specifications)
                .HasConversion(dictConverter)
                .HasColumnType("jsonb")
                .Metadata.SetValueComparer(dictComparer);

            // ----------------------------
            // Defaults DB
            // ----------------------------
            builder.Entity<Post>()
                .Property(p => p.CreatedAt)
                .HasDefaultValueSql("NOW()");

            builder.Entity<Post>()
                .Property(p => p.IsForRent)
                .HasDefaultValue(false);

            builder.Entity<Comment>()
                .Property(c => c.CreatedAt)
                .HasDefaultValueSql("NOW()");

            // ----------------------------
            // Rating TPT
            // ----------------------------
            builder.Entity<RatingUser>().ToTable("UserRatings");
            builder.Entity<RatingPost>().ToTable("PostRatings");
            
            builder.Entity<Post>()
                .Property(p => p.RateAverage)
                .IsRequired(false);

        }

        private static void SeedFromJson(ModelBuilder builder)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var basePath = Path.Combine("Data", "Seed");

            var usersPath      = Path.Combine(basePath, "users.json");
            var postsPath      = Path.Combine(basePath, "posts_clean.json");
            var postImagesPath = Path.Combine(basePath, "post_images.json");
            var commentsPath   = Path.Combine(basePath, "comments_seed.json");
            var ratingsPath    = Path.Combine(basePath, "ratings_posts.json");

            if (File.Exists(usersPath))
            {
                var users = JsonSerializer.Deserialize<List<ApplicationUser>>(
                    File.ReadAllText(usersPath), options) ?? new();

                foreach (var u in users)
                    builder.Entity<ApplicationUser>().HasData(u);
            }

            if (File.Exists(postsPath))
            {
                var posts = JsonSerializer.Deserialize<List<Post>>(
                    File.ReadAllText(postsPath), options) ?? new();

                foreach (var p in posts)
                    builder.Entity<Post>().HasData(p);
            }
            
            if (File.Exists(postImagesPath))
            {
                var images = JsonSerializer.Deserialize<List<PostImage>>(
                    File.ReadAllText(postImagesPath), options);

                if (images != null)
                {
                    foreach (var img in images)
                    {
                        builder.Entity<PostImage>().HasData(new
                        {
                            img.Id,
                            img.ImageUrl,
                            img.PostId
                        });
                    }
                }
            }

            if (File.Exists(commentsPath))
            {
                var comments = JsonSerializer.Deserialize<List<Comment>>(
                    File.ReadAllText(commentsPath), options) ?? new();

                foreach (var c in comments)
                {
                    builder.Entity<Comment>().HasData(new
                    {
                        c.Id,
                        c.Body,
                        c.PostId,
                        c.UserId
                    });
                }
            }

            if (File.Exists(ratingsPath))
            {
                var ratings = JsonSerializer.Deserialize<List<RatingPost>>(
                    File.ReadAllText(ratingsPath), options) ?? new();

                foreach (var r in ratings)
                {
                    builder.Entity<RatingPost>().HasData(new
                    {
                        r.Id,
                        r.Value,
                        r.OwnerId,
                        r.RatedPostId
                    });
                }
            }
        }
    }
}
