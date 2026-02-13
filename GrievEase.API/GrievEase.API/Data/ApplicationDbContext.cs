using Microsoft.EntityFrameworkCore;
using GrievEase.API.Models.Entities;

namespace GrievEase.API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Grievance> Grievances { get; set; }
        public DbSet<GrievanceUpvote> GrievanceUpvotes { get; set; }
        public DbSet<TokenBlacklist> TokenBlacklists { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // UNIQUE constraint for one user → one upvote per grievance
            modelBuilder.Entity<GrievanceUpvote>()
                .HasIndex(gu => new { gu.GrievanceId, gu.UserId })
                .IsUnique();

            // Store enums as string
            modelBuilder.Entity<User>()
                .Property(u => u.SignInType)
                .HasConversion<string>();

            modelBuilder.Entity<Grievance>()
                .Property(g => g.Status)
                .HasConversion<string>();


            // ************ THE IMPORTANT FIX ************
            // Prevent multiple cascade delete paths
            modelBuilder.Entity<GrievanceUpvote>()
                .HasOne(gu => gu.User)
                .WithMany(u => u.GrievanceUpvotes)
                .HasForeignKey(gu => gu.UserId)
                .OnDelete(DeleteBehavior.NoAction);  // THIS LINE FIXES THE ERROR
        }

    }
}
