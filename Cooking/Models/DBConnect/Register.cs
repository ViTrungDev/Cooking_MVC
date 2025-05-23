﻿using Cooking.Models.DBConnect.Order;
using Cooking.Models.DBConnect.UserModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cooking.Models.DBConnect
{
    [Table("Register")]
    public class Register
    {
        [Key]
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsAdmin { get; set; } = false;
        public string? ResetToken { get; set; } // Mã OTP
        public DateTime? ResetTokenExpires { get; set; } // Thời gian hết hạn OTP

        // Danh sách RefreshTokens của người dùng (One-to-Many)
        public List<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
        public ICollection<OrderModel> Orders { get; set; } = new List<OrderModel>();
        public UserInfo? UserInfo { get; set; }

        public override string ToString()
        {
            return $"ID: {Id}, UserName: {UserName}, Email: {Email}, IsAdmin: {IsAdmin}";
        }
    }
}
