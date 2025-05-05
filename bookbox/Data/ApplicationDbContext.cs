using Microsoft.EntityFrameworkCore;
using bookbox.Models;

namespace bookbox.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Address> Addresses { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configure User entity
            modelBuilder.Entity<User>()
                .HasMany(u => u.Addresses)
                .WithOne(a => a.User)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);
                
            // // Add indexes
            // modelBuilder.Entity<User>()
            //     .HasIndex(u => u.Email)
            //     .IsUnique();
                
            // modelBuilder.Entity<User>()
            //     .HasIndex(u => u.Username)
            //     .IsUnique();
                
            // // Configure PostgreSQL UUID column types for Guid properties
            // modelBuilder.Entity<User>()
            //     .Property(u => u.Id)
            //     .HasColumnType("uuid");
                
            // modelBuilder.Entity<Address>()
            //     .Property(a => a.UserId)
            //     .HasColumnType("uuid");
        }
    }
}