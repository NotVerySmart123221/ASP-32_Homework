using ASP_32.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ASP_32.Data
{
    public class DataContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<UserAccess> UserAccesses { get; set; }
        public DbSet<ProductGroup> ProductGroups { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }

        public DataContext(DbContextOptions options) : base(options)
        { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            #region Seed
            modelBuilder.Entity<UserRole>()
            .HasData([
                new UserRole()
                {
                    Id = "Admin",
                    Description = "System Root Administrator",
                    CanCreate = true,
                    CanRead   = true,
                    CanUpdate = true,
                    CanDelete = true,
                },
                new UserRole()
                {
                    Id = "Guest",
                    Description = "Self Registered User",
                    CanCreate = false,
                    CanRead   = false,
                    CanUpdate = false,
                    CanDelete = false,
                },
            ]);

            modelBuilder.Entity<User>()
                .HasData(new User()
                {
                    Id = Guid.Parse("27745D91-2DAF-4088-8925-74E5F88BF415"),
                    Name = "Default Administrator",
                    Email = "admin@i.ua",
                    RegisteredAt = DateTime.UnixEpoch,
                });

            modelBuilder.Entity<UserAccess>()
                .HasData(new UserAccess()
                {
                    Id = Guid.Parse("09DF387C-7050-4B76-9DB9-564EC352FD44"),
                    UserId = Guid.Parse("27745D91-2DAF-4088-8925-74E5F88BF415"),
                    RoleId = "Admin",
                    Login = "Admin",
                    Salt = "4506C746-8FDD-4586-9BF4-95D6933C3B4F",
                    Dk = "2744FC45FF2F7CACD2EB"  // password = Admin
                });
            #endregion
            modelBuilder.Entity<Cart>()
    .HasOne(c => c.User)
    .WithMany(u => u.Carts)  // явно указываем коллекцию
    .HasForeignKey(c => c.UserId); // явно указываем FK

            modelBuilder.Entity<CartItem>()
                .HasOne(сi => сi.Cart)
                .WithMany(c => c.CartItems);

            modelBuilder.Entity<CartItem>()
                .HasOne(сi => сi.Product)
                .WithMany();

            modelBuilder.Entity<Product>()
                .HasMany(p => p.Feedbacks)
                .WithOne()
                .HasForeignKey(p => p.ProductId);

            modelBuilder.Entity<Product>()
                .HasOne(p => p.Group)
                .WithMany(pg => pg.Products)
                .HasForeignKey(p => p.GroupId);

            modelBuilder.Entity<Product>()
                .HasIndex(p => p.Slug)
                .IsUnique();

            modelBuilder.Entity<ProductGroup>()
                .HasIndex(p => p.Slug)
                .IsUnique();

            modelBuilder.Entity<UserAccess>()
                .HasOne(ua => ua.User)
                .WithMany(u => u.Accesses);

            modelBuilder.Entity<UserAccess>()
                .HasOne(ua => ua.Role)
                .WithMany()
                .HasForeignKey(ua => ua.RoleId);

            modelBuilder.Entity<UserAccess>()
                .HasIndex(ua => ua.Login)
                .IsUnique();

        }
    }
}
