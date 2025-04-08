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

    }
}
