using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Serilog;
using CourseManager.Shared.Data;
using CourseManager.Shared.Services;
using CourseManager.Shared.Repositories;
using CourseManager.Shared.Controllers;
using CourseManager.Shared.Middleware;
using CourseManager.Shared.Profiles;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Auth Service API", Version = "v1" });
});

// Database
builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
        };
    });

builder.Services.AddAuthorization();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Services
builder.Services.AddScoped<IIdentityService, IdentityService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

// AutoMapper
builder.Services.AddAutoMapper(typeof(UserProfile));

// FluentValidation
builder.Services.AddFluentValidationAutoValidation();

// Service Discovery
builder.Services.AddConsul(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Middleware
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseMiddleware<RateLimitingMiddleware>();

app.UseHttpsRedirection();
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => new { Status = "Healthy", Service = "AuthService", Timestamp = DateTime.UtcNow });

// Service Discovery
app.UseConsul();

// Seed database
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
    try
    {
        context.Database.EnsureCreated();
        Log.Information("Auth Service database initialized successfully");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "An error occurred while initializing the Auth Service database");
    }
}

try
{
    Log.Information("Starting Auth Service on port 5003");
    app.Run("http://localhost:5003");
}
catch (Exception ex)
{
    Log.Fatal(ex, "Auth Service terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
