using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cooking.Models.DBConnect
{
    [Table("RefreshToken")]
    public class RefreshToken
    {
        [Key]
        public int Id { get; set; }
        public string Token { get; set; }
        public string UserId { get; set; }  // Khóa ngoại liên kết người dùng
        public DateTime Expires { get; set; }
        public bool IsRevoked { get; set; }    // Đánh dấu token đã bị thu hồi hay chưa
        public DateTime CreatedAt { get; set; }
        public DateTime? RevokedAt { get; set; }

        // Thuộc tính điều hướng để liên kết với bảng Register
        [ForeignKey("UserId")]
        public Register User { get; set; }
    }
}
