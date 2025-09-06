# Course Manager API

Backend .NET 8 Web API cho ứng dụng quản lý khóa học với microservice architecture phát triển Lê Huy Hoàng.

## Tính năng chính

- **Authentication & Authorization**: JWT-based authentication với refresh token
- **Course Management**: CRUD operations cho khóa học
- **Category Management**: Quản lý danh mục khóa học
- **Order Management**: Xử lý đơn hàng và thanh toán
- **Payment Integration**: Tích hợp MoMo payment gateway
- **API Gateway**: Routing và load balancing
- **Error Handling**: Middleware xử lý lỗi toàn cục
- **Rate Limiting**: Giới hạn số lượng request
- **AutoMapper**: Mapping tự động giữa models và DTOs
- **SQL Server**: Database với Entity Framework Core

## Cấu trúc Project

```
CourseManager.API/
├── Controllers/          # API Controllers
├── Data/                # DbContext và database configuration
├── DTOs/                # Data Transfer Objects
├── Middleware/          # Custom middleware
├── Models/              # Entity models
│   └── Auth/           # Authentication models
├── Profiles/            # AutoMapper profiles
├── Services/            # Business logic services
├── appsettings.json     # Configuration
└── Program.cs           # Application startup
```

## Cài đặt và chạy

### Yêu cầu
- .NET 8 SDK
- SQL Server hoặc SQL Server LocalDB
- Visual Studio 2022 hoặc VS Code

### Cài đặt

1. Clone repository
```bash
git clone <repository-url>
cd CourseManager.API
```

2. Cài đặt dependencies
```bash
dotnet restore
```

3. Cấu hình database
- Cập nhật connection string trong `appsettings.json`
- Chạy migration để tạo database

4. Chạy ứng dụng
```bash
dotnet run
```

### Database Migration

```bash
# Tạo migration
dotnet ef migrations add InitialCreate

# Cập nhật database
dotnet ef database update
```

## API Endpoints

### Authentication
- `POST /api/auth/login` - Đăng nhập
- `POST /api/auth/register` - Đăng ký
- `POST /api/auth/refresh` - Refresh token
- `POST /api/auth/revoke` - Thu hồi token

### Courses
- `GET /api/courses` - Lấy tất cả khóa học
- `GET /api/courses/{id}` - Lấy khóa học theo ID
- `POST /api/courses` - Tạo khóa học mới (Admin)
- `PUT /api/courses/{id}` - Cập nhật khóa học (Admin)
- `DELETE /api/courses/{id}` - Xóa khóa học (Admin)
- `GET /api/courses/category/{category}` - Lấy khóa học theo danh mục
- `GET /api/courses/search?q={query}` - Tìm kiếm khóa học

### Categories
- `GET /api/categories` - Lấy tất cả danh mục
- `GET /api/categories/{id}` - Lấy danh mục theo ID
- `POST /api/categories` - Tạo danh mục mới (Admin)
- `PUT /api/categories/{id}` - Cập nhật danh mục (Admin)
- `DELETE /api/categories/{id}` - Xóa danh mục (Admin)

### Orders
- `GET /api/orders` - Lấy tất cả đơn hàng (Admin)
- `GET /api/orders/{id}` - Lấy đơn hàng theo ID
- `GET /api/orders/user/{userId}` - Lấy đơn hàng của user
- `POST /api/orders` - Tạo đơn hàng mới
- `PUT /api/orders/{id}/status` - Cập nhật trạng thái đơn hàng
- `PUT /api/orders/{id}/payment` - Cập nhật trạng thái thanh toán

### Payments
- `POST /api/payments/momo/create` - Tạo MoMo payment
- `POST /api/payments/momo/callback` - MoMo payment callback
- `GET /api/payments/methods` - Lấy danh sách phương thức thanh toán

## Cấu hình

### JWT Settings
```json
{
  "JwtSettings": {
    "SecretKey": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
    "Issuer": "CourseManagerAPI",
    "Audience": "CourseManagerUsers",
    "ExpiryInMinutes": 60
  }
}
```

### MoMo Payment
```json
{
  "MoMoPayment": {
    "PartnerCode": "MOMO_PARTNER_CODE",
    "AccessKey": "MOMO_ACCESS_KEY",
    "SecretKey": "MOMO_SECRET_KEY",
    "Endpoint": "https://test-payment.momo.vn/v2/gateway/api/create"
  }
}
```

### Rate Limiting
```json
{
  "RateLimiting": {
    "MaxRequests": 100,
    "TimeWindowMinutes": 1
  }
}
```

## Middleware

### Error Handling Middleware
Xử lý lỗi toàn cục và trả về response chuẩn.

### API Gateway Middleware
Routing requests đến các microservices tương ứng.

### Rate Limiting Middleware
Giới hạn số lượng requests từ mỗi client.

## Services

### BaseService
Service cơ sở cung cấp các phương thức CRUD chung:
- `GetAllAsync()`
- `GetByIdAsync(int id)`
- `CreateAsync(T entity)`
- `UpdateAsync(int id, T entity)`
- `DeleteAsync(int id)`
- `SoftDeleteAsync(int id)`
- `ExistsAsync(int id)`
- `CountAsync()`
- `GetPagedAsync(int pageNumber, int pageSize)`

### IdentityService
Quản lý authentication và authorization:
- Login/Register
- JWT token generation
- Refresh token management
- Password management
- User profile management

## AutoMapper Profiles

- `CourseProfile`: Mapping cho Course entities
- `CategoryProfile`: Mapping cho Category entities
- `OrderProfile`: Mapping cho Order entities
- `UserProfile`: Mapping cho User entities

## Logging

Sử dụng Serilog cho logging:
- Console logging
- File logging (daily rotation)
- Structured logging với properties

## Security

- JWT Authentication
- Password hashing với BCrypt
- CORS configuration
- Rate limiting
- Input validation với FluentValidation

## Development

### Chạy tests
```bash
dotnet test
```

### Build production
```bash
dotnet publish -c Release -o ./publish
```

### Docker (optional)
```bash
docker build -t coursemanager-api .
docker run -p 5000:5000 coursemanager-api
```

## License

MIT License
"# CodeBaseMicrosofrServices" 
