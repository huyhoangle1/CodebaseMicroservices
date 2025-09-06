-- Script tạo dữ liệu mặc định cho Course Manager
-- Chạy sau khi migration để có tài khoản admin và dữ liệu cơ bản

-- 1. Tạo tài khoản Admin mặc định
-- Password: Admin123! (đã được hash bằng BCrypt)
INSERT INTO Users (FirstName, LastName, Email, PasswordHash, Role, IsActive, CreatedAt, UpdatedAt)
VALUES 
('System', 'Administrator', 'admin@coursemanager.com', '$2a$11$K8Y1lZqN2Q5vR7sT9uWxYe3bC6dE8fG1hI4jK7lM0nO3pQ6rS9tU2vW5xY8zA', 'Admin', 1, GETUTCDATE(), GETUTCDATE()),
('John', 'Doe', 'john.doe@example.com', '$2a$11$K8Y1lZqN2Q5vR7sT9uWxYe3bC6dE8fG1hI4jK7lM0nO3pQ6rS9tU2vW5xY8zA', 'User', 1, GETUTCDATE(), GETUTCDATE()),
('Jane', 'Smith', 'jane.smith@example.com', '$2a$11$K8Y1lZqN2Q5vR7sT9uWxYe3bC6dE8fG1hI4jK7lM0nO3pQ6rS9tU2vW5xY8zA', 'User', 1, GETUTCDATE(), GETUTCDATE());

-- 2. Tạo các quyền hạn cơ bản
INSERT INTO Permissions (Name, Description, Resource, Action, Module, IsActive, CreatedAt)
VALUES 
-- Course Management Permissions
('courses.create', 'Tạo khóa học mới', 'courses', 'create', 'Course Management', 1, GETUTCDATE()),
('courses.read', 'Xem danh sách khóa học', 'courses', 'read', 'Course Management', 1, GETUTCDATE()),
('courses.update', 'Cập nhật khóa học', 'courses', 'update', 'Course Management', 1, GETUTCDATE()),
('courses.delete', 'Xóa khóa học', 'courses', 'delete', 'Course Management', 1, GETUTCDATE()),

-- Order Management Permissions
('orders.create', 'Tạo đơn hàng mới', 'orders', 'create', 'Order Management', 1, GETUTCDATE()),
('orders.read', 'Xem danh sách đơn hàng', 'orders', 'read', 'Order Management', 1, GETUTCDATE()),
('orders.update', 'Cập nhật đơn hàng', 'orders', 'update', 'Order Management', 1, GETUTCDATE()),
('orders.delete', 'Xóa đơn hàng', 'orders', 'delete', 'Order Management', 1, GETUTCDATE()),

-- User Management Permissions
('users.create', 'Tạo người dùng mới', 'users', 'create', 'User Management', 1, GETUTCDATE()),
('users.read', 'Xem danh sách người dùng', 'users', 'read', 'User Management', 1, GETUTCDATE()),
('users.update', 'Cập nhật người dùng', 'users', 'update', 'User Management', 1, GETUTCDATE()),
('users.delete', 'Xóa người dùng', 'users', 'delete', 'User Management', 1, GETUTCDATE()),

-- Category Management Permissions
('categories.create', 'Tạo danh mục mới', 'categories', 'create', 'Category Management', 1, GETUTCDATE()),
('categories.read', 'Xem danh sách danh mục', 'categories', 'read', 'Category Management', 1, GETUTCDATE()),
('categories.update', 'Cập nhật danh mục', 'categories', 'update', 'Category Management', 1, GETUTCDATE()),
('categories.delete', 'Xóa danh mục', 'categories', 'delete', 'Category Management', 1, GETUTCDATE()),

-- Permission Management Permissions
('permissions.create', 'Tạo quyền hạn mới', 'permissions', 'create', 'Permission Management', 1, GETUTCDATE()),
('permissions.read', 'Xem danh sách quyền hạn', 'permissions', 'read', 'Permission Management', 1, GETUTCDATE()),
('permissions.update', 'Cập nhật quyền hạn', 'permissions', 'update', 'Permission Management', 1, GETUTCDATE()),
('permissions.delete', 'Xóa quyền hạn', 'permissions', 'delete', 'Permission Management', 1, GETUTCDATE()),

-- Role Management Permissions
('roles.create', 'Tạo vai trò mới', 'roles', 'create', 'Role Management', 1, GETUTCDATE()),
('roles.read', 'Xem danh sách vai trò', 'roles', 'read', 'Role Management', 1, GETUTCDATE()),
('roles.update', 'Cập nhật vai trò', 'roles', 'update', 'Role Management', 1, GETUTCDATE()),
('roles.delete', 'Xóa vai trò', 'roles', 'delete', 'Role Management', 1, GETUTCDATE()),

-- Menu Management Permissions
('menus.create', 'Tạo menu mới', 'menus', 'create', 'Menu Management', 1, GETUTCDATE()),
('menus.read', 'Xem danh sách menu', 'menus', 'read', 'Menu Management', 1, GETUTCDATE()),
('menus.update', 'Cập nhật menu', 'menus', 'update', 'Menu Management', 1, GETUTCDATE()),
('menus.delete', 'Xóa menu', 'menus', 'delete', 'Menu Management', 1, GETUTCDATE()),

-- Payment Management Permissions
('payments.create', 'Tạo thanh toán mới', 'payments', 'create', 'Payment Management', 1, GETUTCDATE()),
('payments.read', 'Xem danh sách thanh toán', 'payments', 'read', 'Payment Management', 1, GETUTCDATE()),
('payments.update', 'Cập nhật thanh toán', 'payments', 'update', 'Payment Management', 1, GETUTCDATE()),
('payments.delete', 'Xóa thanh toán', 'payments', 'delete', 'Payment Management', 1, GETUTCDATE()),

-- System Management Permissions
('system.settings', 'Quản lý cài đặt hệ thống', 'system', 'settings', 'System Management', 1, GETUTCDATE()),
('system.logs', 'Xem logs hệ thống', 'system', 'logs', 'System Management', 1, GETUTCDATE()),
('system.backup', 'Sao lưu dữ liệu', 'system', 'backup', 'System Management', 1, GETUTCDATE());

-- 3. Tạo các vai trò cơ bản
INSERT INTO Roles (Name, Description, Color, Icon, Priority, IsSystemRole, IsActive, CreatedAt)
VALUES 
('Super Admin', 'Quản trị viên cấp cao với tất cả quyền hạn', '#dc3545', 'fas fa-crown', 1, 1, 1, GETUTCDATE()),
('Admin', 'Quản trị viên hệ thống', '#6f42c1', 'fas fa-user-shield', 2, 1, 1, GETUTCDATE()),
('Manager', 'Quản lý cấp trung', '#fd7e14', 'fas fa-user-tie', 3, 0, 1, GETUTCDATE()),
('Instructor', 'Giảng viên', '#20c997', 'fas fa-chalkboard-teacher', 4, 0, 1, GETUTCDATE()),
('Student', 'Học viên', '#0d6efd', 'fas fa-user-graduate', 5, 0, 1, GETUTCDATE()),
('User', 'Người dùng thông thường', '#6c757d', 'fas fa-user', 6, 0, 1, GETUTCDATE());

-- 4. Gán quyền hạn cho Super Admin (tất cả quyền)
INSERT INTO RolePermissions (RoleId, PermissionId, AssignedAt, IsActive)
SELECT 
    r.Id as RoleId,
    p.Id as PermissionId,
    GETUTCDATE() as AssignedAt,
    1 as IsActive
FROM Roles r
CROSS JOIN Permissions p
WHERE r.Name = 'Super Admin';

-- 5. Gán quyền hạn cho Admin (trừ system permissions)
INSERT INTO RolePermissions (RoleId, PermissionId, AssignedAt, IsActive)
SELECT 
    r.Id as RoleId,
    p.Id as PermissionId,
    GETUTCDATE() as AssignedAt,
    1 as IsActive
FROM Roles r
CROSS JOIN Permissions p
WHERE r.Name = 'Admin' 
AND p.Module != 'System Management';

-- 6. Gán quyền hạn cho Manager (courses, orders, categories)
INSERT INTO RolePermissions (RoleId, PermissionId, AssignedAt, IsActive)
SELECT 
    r.Id as RoleId,
    p.Id as PermissionId,
    GETUTCDATE() as AssignedAt,
    1 as IsActive
FROM Roles r
CROSS JOIN Permissions p
WHERE r.Name = 'Manager' 
AND p.Module IN ('Course Management', 'Order Management', 'Category Management');

-- 7. Gán quyền hạn cho Instructor (courses read, orders read)
INSERT INTO RolePermissions (RoleId, PermissionId, AssignedAt, IsActive)
SELECT 
    r.Id as RoleId,
    p.Id as PermissionId,
    GETUTCDATE() as AssignedAt,
    1 as IsActive
FROM Roles r
CROSS JOIN Permissions p
WHERE r.Name = 'Instructor' 
AND p.Resource IN ('courses', 'orders') 
AND p.Action = 'read';

-- 8. Gán quyền hạn cho Student (courses read, orders create/read)
INSERT INTO RolePermissions (RoleId, PermissionId, AssignedAt, IsActive)
SELECT 
    r.Id as RoleId,
    p.Id as PermissionId,
    GETUTCDATE() as AssignedAt,
    1 as IsActive
FROM Roles r
CROSS JOIN Permissions p
WHERE r.Name = 'Student' 
AND ((p.Resource = 'courses' AND p.Action = 'read') 
     OR (p.Resource = 'orders' AND p.Action IN ('create', 'read')));

-- 9. Gán vai trò Super Admin cho admin user
INSERT INTO UserRoles (UserId, RoleId, AssignedAt, IsActive)
SELECT 
    u.Id as UserId,
    r.Id as RoleId,
    GETUTCDATE() as AssignedAt,
    1 as IsActive
FROM Users u
CROSS JOIN Roles r
WHERE u.Email = 'admin@coursemanager.com' 
AND r.Name = 'Super Admin';

-- 10. Gán vai trò User cho các user khác
INSERT INTO UserRoles (UserId, RoleId, AssignedAt, IsActive)
SELECT 
    u.Id as UserId,
    r.Id as RoleId,
    GETUTCDATE() as AssignedAt,
    1 as IsActive
FROM Users u
CROSS JOIN Roles r
WHERE u.Email IN ('john.doe@example.com', 'jane.smith@example.com')
AND r.Name = 'User';

-- 11. Tạo menu hệ thống
INSERT INTO Menus (Name, Description, Url, Icon, Color, ParentId, SortOrder, Permission, Component, Module, IsVisible, IsActive, CreatedAt)
VALUES 
-- Main Menus
('Dashboard', 'Trang chủ', '/dashboard', 'fas fa-tachometer-alt', '#007bff', NULL, 1, 'dashboard.read', 'Dashboard', 'Main', 1, 1, GETUTCDATE()),
('Course Management', 'Quản lý khóa học', '/courses', 'fas fa-book', '#28a745', NULL, 2, 'courses.read', 'CourseManagement', 'Course', 1, 1, GETUTCDATE()),
('Order Management', 'Quản lý đơn hàng', '/orders', 'fas fa-shopping-cart', '#ffc107', NULL, 3, 'orders.read', 'OrderManagement', 'Order', 1, 1, GETUTCDATE()),
('User Management', 'Quản lý người dùng', '/users', 'fas fa-users', '#6f42c1', NULL, 4, 'users.read', 'UserManagement', 'User', 1, 1, GETUTCDATE()),
('System Settings', 'Cài đặt hệ thống', '/settings', 'fas fa-cog', '#6c757d', NULL, 5, 'system.settings', 'SystemSettings', 'System', 1, 1, GETUTCDATE()),

-- Course Sub-menus
('All Courses', 'Tất cả khóa học', '/courses/list', 'fas fa-list', '#28a745', (SELECT Id FROM Menus WHERE Name = 'Course Management'), 1, 'courses.read', 'CourseList', 'Course', 1, 1, GETUTCDATE()),
('Create Course', 'Tạo khóa học', '/courses/create', 'fas fa-plus', '#28a745', (SELECT Id FROM Menus WHERE Name = 'Course Management'), 2, 'courses.create', 'CourseCreate', 'Course', 1, 1, GETUTCDATE()),
('Categories', 'Danh mục', '/courses/categories', 'fas fa-tags', '#28a745', (SELECT Id FROM Menus WHERE Name = 'Course Management'), 3, 'categories.read', 'CategoryList', 'Course', 1, 1, GETUTCDATE()),

-- Order Sub-menus
('All Orders', 'Tất cả đơn hàng', '/orders/list', 'fas fa-list', '#ffc107', (SELECT Id FROM Menus WHERE Name = 'Order Management'), 1, 'orders.read', 'OrderList', 'Order', 1, 1, GETUTCDATE()),
('Create Order', 'Tạo đơn hàng', '/orders/create', 'fas fa-plus', '#ffc107', (SELECT Id FROM Menus WHERE Name = 'Order Management'), 2, 'orders.create', 'OrderCreate', 'Order', 1, 1, GETUTCDATE()),
('Payments', 'Thanh toán', '/orders/payments', 'fas fa-credit-card', '#ffc107', (SELECT Id FROM Menus WHERE Name = 'Order Management'), 3, 'payments.read', 'PaymentList', 'Order', 1, 1, GETUTCDATE()),

-- User Sub-menus
('All Users', 'Tất cả người dùng', '/users/list', 'fas fa-list', '#6f42c1', (SELECT Id FROM Menus WHERE Name = 'User Management'), 1, 'users.read', 'UserList', 'User', 1, 1, GETUTCDATE()),
('Create User', 'Tạo người dùng', '/users/create', 'fas fa-plus', '#6f42c1', (SELECT Id FROM Menus WHERE Name = 'User Management'), 2, 'users.create', 'UserCreate', 'User', 1, 1, GETUTCDATE()),
('Roles', 'Vai trò', '/users/roles', 'fas fa-user-tag', '#6f42c1', (SELECT Id FROM Menus WHERE Name = 'User Management'), 3, 'roles.read', 'RoleList', 'User', 1, 1, GETUTCDATE()),
('Permissions', 'Quyền hạn', '/users/permissions', 'fas fa-key', '#6f42c1', (SELECT Id FROM Menus WHERE Name = 'User Management'), 4, 'permissions.read', 'PermissionList', 'User', 1, 1, GETUTCDATE()),

-- System Sub-menus
('General Settings', 'Cài đặt chung', '/settings/general', 'fas fa-cog', '#6c757d', (SELECT Id FROM Menus WHERE Name = 'System Settings'), 1, 'system.settings', 'GeneralSettings', 'System', 1, 1, GETUTCDATE()),
('Menu Management', 'Quản lý menu', '/settings/menus', 'fas fa-bars', '#6c757d', (SELECT Id FROM Menus WHERE Name = 'System Settings'), 2, 'menus.read', 'MenuManagement', 'System', 1, 1, GETUTCDATE()),
('System Logs', 'Logs hệ thống', '/settings/logs', 'fas fa-file-alt', '#6c757d', (SELECT Id FROM Menus WHERE Name = 'System Settings'), 3, 'system.logs', 'SystemLogs', 'System', 1, 1, GETUTCDATE());

-- 12. Gán menu cho Super Admin (tất cả menu)
INSERT INTO MenuRoles (MenuId, RoleId, AssignedAt, IsActive)
SELECT 
    m.Id as MenuId,
    r.Id as RoleId,
    GETUTCDATE() as AssignedAt,
    1 as IsActive
FROM Menus m
CROSS JOIN Roles r
WHERE r.Name = 'Super Admin';

-- 13. Gán menu cho Admin (trừ system logs)
INSERT INTO MenuRoles (MenuId, RoleId, AssignedAt, IsActive)
SELECT 
    m.Id as MenuId,
    r.Id as RoleId,
    GETUTCDATE() as AssignedAt,
    1 as IsActive
FROM Menus m
CROSS JOIN Roles r
WHERE r.Name = 'Admin' 
AND m.Name != 'System Logs';

-- 14. Gán menu cho Manager (courses, orders)
INSERT INTO MenuRoles (MenuId, RoleId, AssignedAt, IsActive)
SELECT 
    m.Id as MenuId,
    r.Id as RoleId,
    GETUTCDATE() as AssignedAt,
    1 as IsActive
FROM Menus m
CROSS JOIN Roles r
WHERE r.Name = 'Manager' 
AND (m.Module IN ('Course', 'Order') OR m.ParentId IN (
    SELECT Id FROM Menus WHERE Module IN ('Course', 'Order')
));

-- 15. Gán menu cho Student (dashboard, courses read)
INSERT INTO MenuRoles (MenuId, RoleId, AssignedAt, IsActive)
SELECT 
    m.Id as MenuId,
    r.Id as RoleId,
    GETUTCDATE() as AssignedAt,
    1 as IsActive
FROM Menus m
CROSS JOIN Roles r
WHERE r.Name = 'Student' 
AND (m.Name IN ('Dashboard', 'All Courses') OR m.ParentId IN (
    SELECT Id FROM Menus WHERE Name = 'Course Management'
));

-- 16. Tạo danh mục mẫu
INSERT INTO Categories (Name, Description, IsActive, CreatedAt, UpdatedAt)
VALUES 
('Programming', 'Lập trình và phát triển phần mềm', 1, GETUTCDATE(), GETUTCDATE()),
('Web Development', 'Phát triển web và ứng dụng web', 1, GETUTCDATE(), GETUTCDATE()),
('Mobile Development', 'Phát triển ứng dụng di động', 1, GETUTCDATE(), GETUTCDATE()),
('Data Science', 'Khoa học dữ liệu và phân tích', 1, GETUTCDATE(), GETUTCDATE()),
('Design', 'Thiết kế UI/UX và đồ họa', 1, GETUTCDATE(), GETUTCDATE()),
('Business', 'Kinh doanh và quản lý', 1, GETUTCDATE(), GETUTCDATE());

-- 17. Tạo khóa học mẫu
INSERT INTO Courses (Title, Description, Instructor, Category, Level, Price, Duration, ImageUrl, IsActive, CreatedAt, UpdatedAt)
VALUES 
('C# Programming Fundamentals', 'Học lập trình C# từ cơ bản đến nâng cao', 'John Doe', 'Programming', 'Beginner', 299000, 40, '/images/csharp-fundamentals.jpg', 1, GETUTCDATE(), GETUTCDATE()),
('ASP.NET Core Web API', 'Xây dựng Web API với ASP.NET Core', 'Jane Smith', 'Web Development', 'Intermediate', 499000, 60, '/images/aspnet-core.jpg', 1, GETUTCDATE(), GETUTCDATE()),
('React.js Complete Guide', 'Học React.js từ A đến Z', 'Mike Johnson', 'Web Development', 'Intermediate', 399000, 50, '/images/react-js.jpg', 1, GETUTCDATE(), GETUTCDATE()),
('Python for Data Science', 'Python cho khoa học dữ liệu', 'Sarah Wilson', 'Data Science', 'Beginner', 599000, 80, '/images/python-data-science.jpg', 1, GETUTCDATE(), GETUTCDATE()),
('UI/UX Design Principles', 'Nguyên lý thiết kế UI/UX', 'David Brown', 'Design', 'Beginner', 349000, 30, '/images/ui-ux-design.jpg', 1, GETUTCDATE(), GETUTCDATE());

-- 18. Tạo đơn hàng mẫu
INSERT INTO Orders (OrderNumber, UserId, TotalAmount, Status, PaymentMethod, PaymentStatus, CreatedAt, UpdatedAt)
VALUES 
('ORD-2024-001', (SELECT Id FROM Users WHERE Email = 'john.doe@example.com'), 299000, 'Completed', 'MoMo', 'Completed', GETUTCDATE(), GETUTCDATE()),
('ORD-2024-002', (SELECT Id FROM Users WHERE Email = 'jane.smith@example.com'), 499000, 'Completed', 'Banking', 'Completed', GETUTCDATE(), GETUTCDATE()),
('ORD-2024-003', (SELECT Id FROM Users WHERE Email = 'john.doe@example.com'), 399000, 'Pending', 'COD', 'Pending', GETUTCDATE(), GETUTCDATE());

-- 19. Tạo Order Items mẫu
INSERT INTO OrderItems (OrderId, CourseId, Quantity, Price, CreatedAt)
VALUES 
((SELECT Id FROM Orders WHERE OrderNumber = 'ORD-2024-001'), (SELECT Id FROM Courses WHERE Title = 'C# Programming Fundamentals'), 1, 299000, GETUTCDATE()),
((SELECT Id FROM Orders WHERE OrderNumber = 'ORD-2024-002'), (SELECT Id FROM Courses WHERE Title = 'ASP.NET Core Web API'), 1, 499000, GETUTCDATE()),
((SELECT Id FROM Orders WHERE OrderNumber = 'ORD-2024-003'), (SELECT Id FROM Courses WHERE Title = 'React.js Complete Guide'), 1, 399000, GETUTCDATE());

PRINT 'Seed data đã được tạo thành công!';
PRINT 'Tài khoản Admin: admin@coursemanager.com / Admin123!';
PRINT 'Tài khoản User: john.doe@example.com / Admin123!';
PRINT 'Tài khoản User: jane.smith@example.com / Admin123!';
