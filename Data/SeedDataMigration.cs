using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using System.IO;

namespace CourseManager.API.Data.Migrations
{
    /// <summary>
    /// Migration để tạo dữ liệu mặc định
    /// </summary>
    public partial class SeedDataMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Đọc file SQL seed data
            var sqlFile = Path.Combine(Directory.GetCurrentDirectory(), "Data", "SeedData.sql");
            
            if (File.Exists(sqlFile))
            {
                var sql = File.ReadAllText(sqlFile);
                migrationBuilder.Sql(sql);
            }
            else
            {
                // Nếu không tìm thấy file, tạo dữ liệu cơ bản
                CreateBasicSeedData(migrationBuilder);
            }
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Xóa dữ liệu seed khi rollback
            migrationBuilder.Sql(@"
                DELETE FROM MenuRoles;
                DELETE FROM UserRoles;
                DELETE FROM RolePermissions;
                DELETE FROM UserPermissions;
                DELETE FROM OrderItems;
                DELETE FROM Orders;
                DELETE FROM Courses;
                DELETE FROM Categories;
                DELETE FROM Menus;
                DELETE FROM Permissions;
                DELETE FROM Roles;
                DELETE FROM Users WHERE Email IN ('admin@coursemanager.com', 'john.doe@example.com', 'jane.smith@example.com');
            ");
        }

        private void CreateBasicSeedData(MigrationBuilder migrationBuilder)
        {
            // Tạo tài khoản admin cơ bản
            migrationBuilder.Sql(@"
                INSERT INTO Users (FirstName, LastName, Email, PasswordHash, Role, IsActive, CreatedAt, UpdatedAt)
                VALUES ('System', 'Administrator', 'admin@coursemanager.com', '$2a$11$K8Y1lZqN2Q5vR7sT9uWxYe3bC6dE8fG1hI4jK7lM0nO3pQ6rS9tU2vW5xY8zA', 'Admin', 1, GETUTCDATE(), GETUTCDATE());
            ");

            // Tạo role Admin
            migrationBuilder.Sql(@"
                INSERT INTO Roles (Name, Description, Color, Icon, Priority, IsSystemRole, IsActive, CreatedAt)
                VALUES ('Admin', 'Quản trị viên hệ thống', '#6f42c1', 'fas fa-user-shield', 1, 1, 1, GETUTCDATE());
            ");

            // Gán role Admin cho user admin
            migrationBuilder.Sql(@"
                INSERT INTO UserRoles (UserId, RoleId, AssignedAt, IsActive)
                SELECT u.Id, r.Id, GETUTCDATE(), 1
                FROM Users u, Roles r
                WHERE u.Email = 'admin@coursemanager.com' AND r.Name = 'Admin';
            ");

            // Tạo menu Dashboard cơ bản
            migrationBuilder.Sql(@"
                INSERT INTO Menus (Name, Description, Url, Icon, Color, SortOrder, IsVisible, IsActive, CreatedAt)
                VALUES ('Dashboard', 'Trang chủ', '/dashboard', 'fas fa-tachometer-alt', '#007bff', 1, 1, 1, GETUTCDATE());
            ");

            // Gán menu cho Admin
            migrationBuilder.Sql(@"
                INSERT INTO MenuRoles (MenuId, RoleId, AssignedAt, IsActive)
                SELECT m.Id, r.Id, GETUTCDATE(), 1
                FROM Menus m, Roles r
                WHERE m.Name = 'Dashboard' AND r.Name = 'Admin';
            ");
        }
    }
}
