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
            modelBuilder.Entity<Register>()
                .HasOne(r => r.UserInfo)  // Register có 1 UserInfo
                .WithOne(ui => ui.Register)  // UserInfo có 1 Register
                .HasForeignKey<UserInfo>(ui => ui.UserId); // UserInfo.UserId là khóa ngoại trỏ đến Register.Id

            base.OnModelCreating(modelBuilder);
        }
    }
}
