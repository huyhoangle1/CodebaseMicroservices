# Course Manager Microservices Architecture

Đây là kiến trúc microservices thực sự cho ứng dụng Course Manager với các service độc lập.

## 🏗️ Kiến trúc Microservices

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   API Gateway   │    │  Course Service │    │  Order Service  │
│   (Port 5000)   │    │   (Port 5001)   │    │   (Port 5002)   │
└─────────────────┘    └─────────────────┘    └─────────────────┘
         │                       │                       │
         └───────────────────────┼───────────────────────┘
                                 │
                    ┌─────────────────┐
                    │  Auth Service   │
                    │   (Port 5003)   │
                    └─────────────────┘
                                 │
                    ┌─────────────────┐
                    │   SQL Server    │
                    │   (Port 1433)   │
                    └─────────────────┘
```

## 🚀 Các Microservices

### 1. **API Gateway** (Port 5000)
- **Chức năng**: Entry point cho tất cả requests
- **Công nghệ**: Ocelot Gateway
- **Tính năng**:
  - Routing requests đến các microservices
  - Load balancing
  - Rate limiting
  - Circuit breaker
  - Service discovery với Consul

### 2. **Course Service** (Port 5001)
- **Chức năng**: Quản lý khóa học và danh mục
- **Endpoints**:
  - `GET /api/courses` - Lấy danh sách khóa học
  - `GET /api/courses/{id}` - Lấy khóa học theo ID
  - `POST /api/courses` - Tạo khóa học mới
  - `PUT /api/courses/{id}` - Cập nhật khóa học
  - `DELETE /api/courses/{id}` - Xóa khóa học
  - `GET /api/categories` - Lấy danh sách danh mục

### 3. **Order Service** (Port 5002)
- **Chức năng**: Quản lý đơn hàng và thanh toán
- **Endpoints**:
  - `GET /api/orders` - Lấy danh sách đơn hàng
  - `POST /api/orders` - Tạo đơn hàng mới
  - `PUT /api/orders/{id}/status` - Cập nhật trạng thái đơn hàng
  - `POST /api/payments/momo/create` - Tạo MoMo payment
  - `POST /api/payments/momo/callback` - MoMo callback

### 4. **Auth Service** (Port 5003)
- **Chức năng**: Xác thực và phân quyền
- **Endpoints**:
  - `POST /api/auth/login` - Đăng nhập
  - `POST /api/auth/register` - Đăng ký
  - `POST /api/auth/refresh` - Refresh token
  - `GET /api/users` - Lấy danh sách users
  - `PUT /api/users/{id}` - Cập nhật user

## 🛠️ Công nghệ sử dụng

### Core Technologies
- **.NET 8** - Framework chính
- **Entity Framework Core** - ORM
- **SQL Server** - Database
- **AutoMapper** - Object mapping
- **FluentValidation** - Input validation
- **Serilog** - Logging

### Microservices Technologies
- **Ocelot** - API Gateway
- **Consul** - Service Discovery
- **MassTransit + RabbitMQ** - Message Queue
- **Polly** - Circuit Breaker & Retry
- **Docker** - Containerization

## 🚀 Cách chạy

### 1. Chạy với Docker Compose (Khuyến nghị)

```bash
cd Microservices
docker-compose up -d
```

### 2. Chạy từng service riêng lẻ

```bash
# Terminal 1 - API Gateway
cd Microservices/ApiGateway
dotnet run

# Terminal 2 - Course Service
cd Microservices/CourseService
dotnet run

# Terminal 3 - Order Service
cd Microservices/OrderService
dotnet run

# Terminal 4 - Auth Service
cd Microservices/AuthService
dotnet run
```

## 📊 Service Discovery

Sử dụng **Consul** để service discovery:
- Consul UI: http://localhost:8500
- Các service tự động đăng ký và health check
- API Gateway tự động route dựa trên service discovery

## 🔄 Message Queue

Sử dụng **RabbitMQ** cho communication giữa services:
- RabbitMQ Management UI: http://localhost:15672
- Username: admin, Password: admin
- Order Service gửi events khi có đơn hàng mới

## 🗄️ Database

Mỗi service có thể có database riêng hoặc shared:
- **Course Service**: CourseManagerDB (Courses, Categories)
- **Order Service**: CourseManagerDB (Orders, OrderItems)
- **Auth Service**: CourseManagerDB (Users, RefreshTokens)

## 🔐 Authentication Flow

1. Client gửi login request đến Auth Service
2. Auth Service tạo JWT token
3. Client gửi requests với JWT token đến API Gateway
4. API Gateway validate token và route đến service tương ứng

## 📈 Monitoring & Health Checks

- Mỗi service có endpoint `/health`
- Consul monitor health của các services
- Serilog logging cho tất cả services
- Circuit breaker pattern cho resilience

## 🔧 Configuration

### Environment Variables
```bash
# Database
ConnectionStrings__DefaultConnection=Server=sqlserver;Database=CourseManagerDB;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=true

# JWT
JwtSettings__SecretKey=YourSuperSecretKeyThatIsAtLeast32CharactersLong!
JwtSettings__Issuer=CourseManagerAPI
JwtSettings__Audience=CourseManagerUsers

# RabbitMQ
ConnectionStrings__RabbitMQ=amqp://admin:admin@rabbitmq:5672

# Consul
Consul__Host=localhost
Consul__Port=8500
```

## 🚀 Deployment

### Docker
```bash
# Build images
docker-compose build

# Run services
docker-compose up -d

# Scale services
docker-compose up -d --scale course-service=3
```

### Kubernetes (Optional)
```bash
kubectl apply -f k8s/
```

## 🔍 API Testing

### Swagger UI
- API Gateway: http://localhost:5000/swagger
- Course Service: http://localhost:5001/swagger
- Order Service: http://localhost:5002/swagger
- Auth Service: http://localhost:5003/swagger

### Health Checks
- API Gateway: http://localhost:5000/health
- Course Service: http://localhost:5001/health
- Order Service: http://localhost:5002/health
- Auth Service: http://localhost:5003/health

## 📝 Benefits của Microservices Architecture

1. **Scalability**: Scale từng service độc lập
2. **Technology Diversity**: Mỗi service có thể dùng tech stack khác nhau
3. **Fault Isolation**: Lỗi ở 1 service không ảnh hưởng service khác
4. **Team Independence**: Teams có thể develop độc lập
5. **Deployment Independence**: Deploy từng service riêng biệt
6. **Resilience**: Circuit breaker, retry patterns
7. **Service Discovery**: Tự động discover và route services

## 🎯 So sánh với Monolithic

| Aspect | Monolithic | Microservices |
|--------|------------|---------------|
| **Deployment** | Deploy toàn bộ app | Deploy từng service |
| **Scaling** | Scale toàn bộ app | Scale service cần thiết |
| **Technology** | 1 tech stack | Nhiều tech stack |
| **Database** | 1 database | Database per service |
| **Complexity** | Đơn giản hơn | Phức tạp hơn |
| **Performance** | Nhanh hơn (no network) | Chậm hơn (network calls) |
| **Testing** | Dễ test | Khó test integration |
