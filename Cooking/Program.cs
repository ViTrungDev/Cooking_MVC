using Cooking.Models.DBConnect;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Load biến môi trường từ file .env
DotNetEnv.Env.Load();

string secretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY");
string issuer = Environment.GetEnvironmentVariable("JWT_ISSUER");
string audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE");


// Cấu hình CORS cho phép frontend truy cập
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Lấy chuỗi kết nối
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                     ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Cấu hình EF Core với MySQL
builder.Services.AddDbContext<CookingDBConnect>((serviceProvider, options) =>
{
    var logger = serviceProvider.GetRequiredService<ILogger<CookingDBConnect>>();

    logger.LogInformation("Đang kết nối tới MySQL...");

    options.UseMySql(connectionString,
     new MySqlServerVersion(new Version(8, 0, 30)))
     .EnableSensitiveDataLogging()
     .LogTo(Console.WriteLine, LogLevel.Information);
});

// Cấu hình JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
        };
    });
// phân quyền cho các API
builder.Services.AddAuthorization(options =>
        {
         options.AddPolicy("AdminOnly", policy =>
         policy.RequireClaim("Role", "Admin"));

          options.AddPolicy("UserOnly", policy =>
                policy.RequireClaim("Role", "User"));

          options.AddPolicy("AdminOrUser", policy =>
                policy.RequireClaim("Role", "Admin", "User"));
        }
      );


// Thêm controller
builder.Services.AddControllers();
builder.Configuration.AddEnvironmentVariables(); // thêm biến môi trường từ file .env

var app = builder.Build();

// Test kết nối DB khi chạy app
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<CookingDBConnect>();

    try
    {
        var userCount = context.Registers.Count();
        Console.WriteLine("=============================== Kết nối Database thành công ==================================");
        Console.WriteLine($"Số lượng người dùng: {userCount}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Lỗi kết nối DB: {ex.Message}");
    }
}

// Middleware
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

// Chỉ map các controller API — KHÔNG map route mặc định MVC
app.MapControllers();

app.Run();
