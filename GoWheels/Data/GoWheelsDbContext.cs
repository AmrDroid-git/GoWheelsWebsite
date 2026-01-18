using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using GoWheels.Models;

namespace GoWheels.Data
{
    // Inherit from IdentityDbContext to handle Users/Roles automatically
    public class GoWheelsDbContext : IdentityDbContext<ApplicationUser>
    {
        public GoWheelsDbContext(DbContextOptions<GoWheelsDbContext> options)
            : base(options)
        {
        }

        public DbSet<Post> Posts { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<RatingUser> UsersRatings { get; set; }
        public DbSet<RatingPost> PostsRatings { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // 1. Configure the Specification Dictionary to store as JSONB in Postgres
            builder.Entity<Post>()
                .Property(p => p.Specifications)
                .HasColumnType("jsonb");
            
            // Force Table-Per-Type (TPT) Inheritance
            // This creates three tables: Ratings, UserRatings, and PostRatings
            builder.Entity<RatingUser>().ToTable("UserRatings");
            builder.Entity<RatingPost>().ToTable("PostRatings");
            
        }
    }
}