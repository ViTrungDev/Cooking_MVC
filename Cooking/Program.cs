using Cooking.Models.DBConnect;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// Lấy chuỗi kết nối từ appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                     ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Cấu hình DbContext và ghi log SQL ra console
builder.Services.AddDbContext<CookingDBConnect>((serviceProvider, options) =>
{
    var logger = serviceProvider.GetRequiredService<ILogger<CookingDBConnect>>();

    logger.LogInformation("Dang ket noi toi  MySQL...");

    options.UseMySql(connectionString,
     new MySqlServerVersion(new Version(8, 0, 30)))  // Định nghĩa phiên bản MySQL server bạn đang sử dụng
     .EnableSensitiveDataLogging() // Log dữ liệu nhạy cảm (khi debug)
     .LogTo(Console.WriteLine, LogLevel.Information);  // Log các câu SQL ra console
});

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Tạo scope để khởi tạo DbContext
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<CookingDBConnect>();

    try
    {
        // Truy vấn thử bảng Registers để khởi tạo và in thông tin ra console
        var userCount = context.Registers.Count();
        Console.WriteLine($"So luong nguoi dung Registers: {userCount}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Loi truy van database: {ex.Message}");
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();

// Map route mặc định
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.Run();
