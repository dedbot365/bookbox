using Bookbox.Models;
using Microsoft.EntityFrameworkCore;

namespace Bookbox.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Bookmark> Bookmarks { get; set; }
        public DbSet<Discount> Discounts { get; set; }
        public DbSet<Announcement> Announcements { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User entity configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.UserId);
                entity.Property(e => e.UserId).ValueGeneratedOnAdd();
                entity.HasIndex(e => e.Username).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
            });

            // Address entity configuration
            modelBuilder.Entity<Address>(entity =>
            {
                entity.HasKey(e => e.AddressId);
                entity.Property(e => e.AddressId).ValueGeneratedOnAdd();
                
                entity.HasOne(e => e.User)
                      .WithMany(u => u.Addresses)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Book entity configuration
            modelBuilder.Entity<Book>(entity =>
            {
                entity.HasKey(e => e.BookId);
                entity.Property(e => e.BookId).ValueGeneratedOnAdd();
                entity.HasIndex(e => e.ISBN).IsUnique();
                
                entity.HasOne(b => b.User)
                      .WithMany()  
                      .HasForeignKey(b => b.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Announcement entity configuration
            modelBuilder.Entity<Announcement>(entity =>
            {
                entity.HasKey(e => e.AnnouncementId);
                entity.Property(e => e.AnnouncementId).ValueGeneratedOnAdd();
                
                entity.HasOne(a => a.User)
                      .WithMany()
                      .HasForeignKey(a => a.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Order entity configuration
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(e => e.OrderId);
                entity.Property(e => e.OrderId).ValueGeneratedOnAdd();
                
                entity.HasOne(o => o.User)
                      .WithMany(u => u.Orders)
                      .HasForeignKey(o => o.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Order>()
                .Property(o => o.OrderNumber)
                .UseIdentityByDefaultColumn();

            modelBuilder.Entity<Order>()
                .HasMany(o => o.OrderItems)
                .WithOne(oi => oi.Order)
                .HasForeignKey(oi => oi.OrderId);

            // OrderItem entity configuration
            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.HasKey(e => e.OrderItemId);
                entity.Property(e => e.OrderItemId).ValueGeneratedOnAdd();
                
                entity.HasOne(oi => oi.Order)
                      .WithMany(o => o.OrderItems)
                      .HasForeignKey(oi => oi.OrderId)
                      .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasOne(oi => oi.Book)
                      .WithMany()
                      .HasForeignKey(oi => oi.BookId)
                      .OnDelete(DeleteBehavior.Restrict);  // Prevent cascade delete conflicts
            });

            // Review entity configuration
            modelBuilder.Entity<Review>(entity =>
            {
                entity.HasKey(e => e.ReviewId);
                entity.Property(e => e.ReviewId).ValueGeneratedOnAdd();
                
                entity.HasOne(r => r.User)
                      .WithMany(u => u.Reviews)
                      .HasForeignKey(r => r.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasOne(r => r.Book)
                      .WithMany(b => b.Reviews)
                      .HasForeignKey(r => r.BookId)
                      .OnDelete(DeleteBehavior.Restrict);  // Prevent cascade delete conflicts
            });

            // Bookmark entity configuration
            modelBuilder.Entity<Bookmark>(entity =>
            {
                entity.HasKey(e => e.BookmarkId);
                entity.Property(e => e.BookmarkId).ValueGeneratedOnAdd();
                
                entity.HasOne(b => b.User)
                      .WithMany(u => u.Bookmarks)
                      .HasForeignKey(b => b.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasOne(b => b.Book)
                      .WithMany()
                      .HasForeignKey(b => b.BookId)
                      .OnDelete(DeleteBehavior.Restrict);  // Prevent cascade delete conflicts
            });

            // Discount entity configuration
            modelBuilder.Entity<Discount>(entity =>
            {
                entity.HasKey(e => e.DiscountId);
                entity.Property(e => e.DiscountId).ValueGeneratedOnAdd();
                
                entity.HasOne(d => d.Book)
                      .WithMany(b => b.Discounts)
                      .HasForeignKey(d => d.BookId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Cart entity configuration
            modelBuilder.Entity<Cart>(entity =>
            {
                entity.HasKey(e => e.CartId);
                entity.Property(e => e.CartId).ValueGeneratedOnAdd();
                
                entity.HasOne(c => c.User)
                      .WithMany()
                      .HasForeignKey(c => c.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // CartItem entity configuration
            modelBuilder.Entity<CartItem>(entity =>
            {
                entity.HasKey(e => e.CartItemId);
                entity.Property(e => e.CartItemId).ValueGeneratedOnAdd();
                
                entity.HasOne(ci => ci.Cart)
                      .WithMany(c => c.CartItems)
                      .HasForeignKey(ci => ci.CartId)
                      .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasOne(ci => ci.Book)
                      .WithMany()
                      .HasForeignKey(ci => ci.BookId)
                      .OnDelete(DeleteBehavior.Restrict);  // Prevent cascade delete conflicts
            });
        }
    }
}