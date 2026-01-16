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
        public DbSet<Rating> Ratings { get; set; }
        public DbSet<RatingUser> UserRatings { get; set; }
        public DbSet<RatingPost> PostRatings { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // 1. Configure the Specification Dictionary to store as JSONB in Postgres
            builder.Entity<Post>()
                .Property(p => p.Specifications)
                .HasColumnType("jsonb");

            // 2. Configure Inheritance for Ratings (Table-Per-Hierarchy)
            builder.Entity<Rating>()
                .HasDiscriminator<string>("RatingType")
                .HasValue<RatingUser>("UserRating")
                .HasValue<RatingPost>("PostRating");

            // 3. Prevent Cascade Delete Cycles
            builder.Entity<Comment>()
                .HasOne(c => c.User)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}