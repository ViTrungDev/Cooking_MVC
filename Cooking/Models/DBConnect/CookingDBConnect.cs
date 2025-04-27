using Cooking.Models.DBConnect.Blogs;
using Cooking.Models.DBConnect.Evaluate;
using Cooking.Models.DBConnect.Order;
using Cooking.Models.DBConnect.UserModel;
using Microsoft.EntityFrameworkCore;

namespace Cooking.Models.DBConnect
{
    public class CookingDBConnect : DbContext
    {
        public CookingDBConnect(DbContextOptions<CookingDBConnect> options) : base(options)
        {
        }

        public DbSet<Register> Registers { get; set; } // Đối tượng mẫu
        public DbSet<Cookingproduct> Cookingproducts { get; set; }
        public DbSet<OrderModel> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<BlogModel> Blogs { get; set; }
        public DbSet<UserInfo> UserInfo { get; set; }
        public DbSet<EvaluateModule> Evaluate { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Quan hệ giữa Register và UserInfo
            modelBuilder.Entity<Register>()
                .HasOne(r => r.UserInfo)  // Register có 1 UserInfo
                .WithOne(ui => ui.Register)  // UserInfo có 1 Register
                .HasForeignKey<UserInfo>(ui => ui.UserId); // UserInfo.UserId là khóa ngoại trỏ đến Register.Id

            // Cấu hình Cascade Delete cho quan hệ giữa Cookingproduct và OrderDetail
            modelBuilder.Entity<OrderDetail>()
                .HasOne(od => od.Product)  // OrderDetail có 1 Cookingproduct
                .WithMany(p => p.OrderDetails)  // Cookingproduct có nhiều OrderDetails
                .HasForeignKey(od => od.product_id)  // Sửa lại từ ProductId thành product_id (trong OrderDetail, tên trường là product_id)
                .OnDelete(DeleteBehavior.Cascade);  // Nếu xóa Cookingproduct thì xóa luôn các OrderDetail liên quan

            // Cấu hình Cascade Delete cho quan hệ giữa OrderModel và OrderDetail
            modelBuilder.Entity<OrderDetail>()
                .HasOne(od => od.Order)  // OrderDetail có 1 Order
                .WithMany(o => o.OrderDetails)  // Order có nhiều OrderDetails
                .HasForeignKey(od => od.order_id)  // OrderDetail.order_id là khóa ngoại trỏ đến Order.Id
                .OnDelete(DeleteBehavior.Cascade);  // Nếu xóa Order thì xóa luôn các OrderDetail liên quan

            // Nếu có thêm các cấu hình khác cho các bảng khác, bạn có thể tiếp tục thêm ở đây

            base.OnModelCreating(modelBuilder);
        }
    }
}
