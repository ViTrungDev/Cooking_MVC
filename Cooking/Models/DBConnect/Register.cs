using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cooking.Models.DBConnect
{
    [Table("Register")]  // Chỉ định tên bảng là "Register"
    public class Register
    {
        [Key]
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsAdmin { get; set; }

        public override string ToString()
        {
            return $"ID: {Id}, UserName: {UserName}, Email: {Email}, IsAdmin: {IsAdmin}";
        }
    }
}
