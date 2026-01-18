using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using GoWheels.Models;
using System.Text.Json;

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

            builder.Entity<Post>()
                .Property(p => p.Specifications)
                .HasColumnType("jsonb");

            builder.Entity<Post>()
                .Property(p => p.CreatedAt)
                .HasDefaultValueSql("NOW()");

            builder.Entity<RatingUser>().ToTable("UserRatings");
            builder.Entity<RatingPost>().ToTable("PostRatings");

            SeedFromJson(builder);
        }


        private static void SeedFromJson(ModelBuilder builder)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            // Chemins relatifs à la racine du projet (et copiés en output)
            var usersPath = Path.Combine("Data", "Seed", "users.json");
            var postsPath = Path.Combine("Data", "Seed", "posts_clean.json");
            var postImagesPath = Path.Combine("Data", "Seed", "post_images.json");

            if (File.Exists(usersPath))
            {
                var usersJson = File.ReadAllText(usersPath);
                var users = JsonSerializer.Deserialize<List<ApplicationUser>>(usersJson, options) ?? new();

                // HasData: évite les navigations, ok pour Identity fields
                foreach (var u in users)
                    builder.Entity<ApplicationUser>().HasData(u);
            }

            if (File.Exists(postsPath))
            {
                var postsJson = File.ReadAllText(postsPath);
                var posts = JsonSerializer.Deserialize<List<Post>>(postsJson, options) ?? new();

                foreach (var p in posts)
                    builder.Entity<Post>().HasData(p);
            }

            if (File.Exists(postImagesPath))
            {
                var imagesJson = File.ReadAllText(postImagesPath);
                var images = JsonSerializer.Deserialize<List<PostImage>>(imagesJson, options) ?? new();

                foreach (var img in images)
                    builder.Entity<PostImage>().HasData(img);
            }
        }
    }
}
