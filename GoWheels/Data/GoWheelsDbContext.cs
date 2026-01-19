using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using GoWheels.Models;
using System.Text.Json;
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



            // Configure Specifications as JSON using a value converter
            var dictConverter = new ValueConverter<Dictionary<string,string>, string>(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                v => JsonSerializer.Deserialize<Dictionary<string,string>>(v, (JsonSerializerOptions)null) ?? new Dictionary<string,string>()
            );


            builder.Entity<Post>()
                .Property(p => p.Specifications)
                .HasConversion(dictConverter)
                .HasColumnType("jsonb"); // tells PostgreSQL to store as jsonb

            builder.Entity<Post>()
                .Property(p => p.CreatedAt)
                .HasDefaultValueSql("NOW()");

            // ----------------------------
            // Comment configuration
            // ----------------------------
            builder.Entity<Comment>()
                .Property(c => c.CreatedAt)
                .HasDefaultValueSql("NOW()");

            // ----------------------------
            // Rating (TPT)
            // ----------------------------
            builder.Entity<RatingUser>().ToTable("UserRatings");
            builder.Entity<RatingPost>().ToTable("PostRatings");

            // ----------------------------
            // JSON Seed
            // ----------------------------
            SeedFromJson(builder);
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
                    File.ReadAllText(postImagesPath), options) ?? new();

                foreach (var img in images)
                    builder.Entity<PostImage>().HasData(img);
            }

            if (File.Exists(commentsPath))
            {
                var comments = JsonSerializer.Deserialize<List<Comment>>(
                    File.ReadAllText(commentsPath), options) ?? new();

                foreach (var c in comments)
                {
                    // IMPORTANT : CreatedAt NON injecté
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
