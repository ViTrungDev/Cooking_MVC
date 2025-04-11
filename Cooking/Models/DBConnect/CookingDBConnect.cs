﻿using Cooking.Models.DBConnect.Blogs;
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


    }
}
