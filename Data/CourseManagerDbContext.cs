using Microsoft.EntityFrameworkCore;
using CourseManager.API.Models;
using CourseManager.API.Models.Auth;

namespace CourseManager.API.Data
{
    public class CourseManagerDbContext : DbContext
    {
        public CourseManagerDbContext(DbContextOptions<CourseManagerDbContext> options) : base(options)
        {
        }

        public DbSet<Course> Courses { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Course entity
            modelBuilder.Entity<Course>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Price).HasPrecision(18, 2);
                entity.Property(e => e.OriginalPrice).HasPrecision(18, 2);
                entity.HasIndex(e => e.Title);
                entity.HasIndex(e => e.Category);
            });

            // Configure Category entity
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Name).IsUnique();
            });

            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Email).IsUnique();
            });

            // Configure Order entity
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TotalAmount).HasPrecision(18, 2);
                entity.HasIndex(e => e.OrderNumber).IsUnique();
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.CreatedAt);
            });

            // Configure OrderItem entity
            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Price).HasPrecision(18, 2);
                entity.Property(e => e.OriginalPrice).HasPrecision(18, 2);
            });

            // Configure RefreshToken entity
            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Token).IsUnique();
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.ExpiresAt);
            });

            // Configure relationships
            modelBuilder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Course)
                .WithMany(c => c.OrderItems)
                .HasForeignKey(oi => oi.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RefreshToken>()
                .HasOne(rt => rt.User)
                .WithMany()
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Seed data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed Categories
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Programming", Description = "Lập trình và phát triển phần mềm", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Category { Id = 2, Name = "Design", Description = "Thiết kế đồ họa và UI/UX", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Category { Id = 3, Name = "Business", Description = "Kinh doanh và khởi nghiệp", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Category { Id = 4, Name = "Marketing", Description = "Marketing và bán hàng", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
            );

            // Seed Courses
            modelBuilder.Entity<Course>().HasData(
                new Course 
                { 
                    Id = 1, 
                    Title = "Lập trình C# từ cơ bản đến nâng cao", 
                    Description = "Khóa học toàn diện về lập trình C# và .NET Framework", 
                    Category = "Programming", 
                    Price = 299000, 
                    OriginalPrice = 499000, 
                    ImageUrl = "/images/csharp-course.jpg", 
                    Instructor = "Nguyễn Văn A", 
                    Duration = 1200, 
                    Level = "Beginner", 
                    IsActive = true, 
                    CreatedAt = DateTime.UtcNow, 
                    UpdatedAt = DateTime.UtcNow 
                },
                new Course 
                { 
                    Id = 2, 
                    Title = "Thiết kế UI/UX với Figma", 
                    Description = "Học thiết kế giao diện người dùng chuyên nghiệp", 
                    Category = "Design", 
                    Price = 199000, 
                    OriginalPrice = 299000, 
                    ImageUrl = "/images/figma-course.jpg", 
                    Instructor = "Trần Thị B", 
                    Duration = 800, 
                    Level = "Intermediate", 
                    IsActive = true, 
                    CreatedAt = DateTime.UtcNow, 
                    UpdatedAt = DateTime.UtcNow 
                },
                new Course 
                { 
                    Id = 3, 
                    Title = "Digital Marketing cho người mới bắt đầu", 
                    Description = "Chiến lược marketing online hiệu quả", 
                    Category = "Marketing", 
                    Price = 149000, 
                    OriginalPrice = 249000, 
                    ImageUrl = "/images/marketing-course.jpg", 
                    Instructor = "Lê Văn C", 
                    Duration = 600, 
                    Level = "Beginner", 
                    IsActive = true, 
                    CreatedAt = DateTime.UtcNow, 
                    UpdatedAt = DateTime.UtcNow 
                }
            );

            // Seed Admin User
            modelBuilder.Entity<User>().HasData(
                new User 
                { 
                    Id = 1, 
                    FirstName = "Admin", 
                    LastName = "System", 
                    Email = "admin@coursemanager.com", 
                    Phone = "0901234567", 
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"), 
                    Role = "Admin", 
                    IsActive = true, 
                    CreatedAt = DateTime.UtcNow, 
                    UpdatedAt = DateTime.UtcNow 
                }
            );
        }
    }
}

