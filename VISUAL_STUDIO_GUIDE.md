# Hướng dẫn mở project trong Visual Studio

## 🚀 Cách mở project

### 1. Mở Solution File
- Mở Visual Studio 2022
- File → Open → Project/Solution
- Chọn file `CourseManager.sln` trong thư mục gốc
- Click Open

### 2. Cấu trúc Solution
```
CourseManager.sln
├── CourseManager.API (Monolithic API - Port 5000)
├── CourseManager.Shared (Shared Library)
├── Microservices/
│   ├── CourseService.API (Port 5001)
│   ├── OrderService.API (Port 5002)
│   ├── AuthService.API (Port 5003)
│   └── ApiGateway (Port 5000)
```

## 🏗️ Chạy Project

### Option 1: Chạy Monolithic API
1. Right-click vào `CourseManager.API`
2. Set as Startup Project
3. F5 hoặc Ctrl+F5 để chạy
4. Truy cập: http://localhost:5000

### Option 2: Chạy Microservices
1. **Chạy từng service riêng lẻ:**
   - Right-click vào `CourseService.API` → Set as Startup Project → F5
   - Right-click vào `OrderService.API` → Set as Startup Project → F5
   - Right-click vào `AuthService.API` → Set as Startup Project → F5
   - Right-click vào `ApiGateway` → Set as Startup Project → F5

2. **Chạy multiple projects:**
   - Right-click vào Solution → Properties
   - Multiple startup projects
   - Set Action = Start cho các services cần thiết

### Option 3: Chạy với Docker
```bash
cd Microservices
docker-compose up -d
```

## 🔧 Cấu hình Database

### 1. SQL Server LocalDB
- Mở SQL Server Management Studio
- Connect to: `(localdb)\mssqllocaldb`
- Tạo database: `CourseManagerDB`

### 2. Connection String
Cập nhật trong `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=CourseManagerDB;Trusted_Connection=true;MultipleActiveResultSets=true;TrustServerCertificate=true"
  }
}
```

### 3. Entity Framework Migration
```bash
# Trong Package Manager Console
Add-Migration InitialCreate
Update-Database
```

## 📊 API Endpoints

### Monolithic API (Port 5000)
- Swagger UI: http://localhost:5000/swagger
- Health Check: http://localhost:5000/health

### Microservices
- **Course Service**: http://localhost:5001/swagger
- **Order Service**: http://localhost:5002/swagger  
- **Auth Service**: http://localhost:5003/swagger
- **API Gateway**: http://localhost:5000/swagger

## 🔐 Authentication

### 1. Register User
```bash
POST http://localhost:5000/api/auth/register
{
  "firstName": "John",
  "lastName": "Doe",
  "email": "john@example.com",
  "password": "password123",
  "confirmPassword": "password123"
}
```

### 2. Login
```bash
POST http://localhost:5000/api/auth/login
{
  "email": "john@example.com",
  "password": "password123"
}
```

### 3. Use Token
Thêm header: `Authorization: Bearer {token}`

## 🐛 Debugging

### 1. Breakpoints
- Set breakpoints trong controllers hoặc services
- F5 để debug
- Step through code với F10, F11

### 2. Logging
- Sử dụng Serilog
- Logs được ghi vào Console và File
- File logs: `logs/coursemanager-.log`

### 3. Exception Handling
- Middleware xử lý lỗi toàn cục
- Logs chi tiết trong Console
- Response format chuẩn

## 📁 Project Structure

```
CourseManager.API/
├── Controllers/          # API Controllers
├── Data/                # DbContext
├── DTOs/                # Data Transfer Objects
├── Middleware/          # Custom Middleware
├── Models/              # Entity Models
├── Profiles/            # AutoMapper Profiles
├── Repositories/        # Data Access Layer
├── Services/            # Business Logic
├── Microservices/       # Microservices Architecture
│   ├── CourseService/
│   ├── OrderService/
│   ├── AuthService/
│   └── ApiGateway/
└── Shared/              # Shared Library
    └── CourseManager.Shared/
```

## 🚀 Deployment

### 1. Publish
- Right-click project → Publish
- Chọn target (IIS, Azure, Docker, etc.)
- Configure settings
- Publish

### 2. Docker
```bash
# Build image
docker build -t coursemanager-api .

# Run container
docker run -p 5000:5000 coursemanager-api
```

## 🔍 Troubleshooting

### 1. Build Errors
- Clean Solution: Build → Clean Solution
- Rebuild Solution: Build → Rebuild Solution
- Restore NuGet Packages: Right-click Solution → Restore NuGet Packages

### 2. Runtime Errors
- Check connection string
- Verify database exists
- Check port conflicts
- Review logs

### 3. Database Issues
- Ensure SQL Server is running
- Check connection string
- Run migrations: `Update-Database`

## 📚 Additional Resources

- [.NET 8 Documentation](https://docs.microsoft.com/en-us/dotnet/)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)
- [AutoMapper](https://docs.automapper.org/)
- [Serilog](https://serilog.net/)
- [Ocelot Gateway](https://ocelot.readthedocs.io/)

## 🎯 Next Steps

1. **Mở Visual Studio**
2. **Load Solution** `CourseManager.sln`
3. **Set Startup Project** (CourseManager.API hoặc ApiGateway)
4. **Configure Database** connection string
5. **Run Project** (F5)
6. **Test APIs** via Swagger UI
7. **Explore Code** và customize theo nhu cầu
