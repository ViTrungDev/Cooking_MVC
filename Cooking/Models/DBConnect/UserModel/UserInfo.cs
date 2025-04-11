using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;


namespace Cooking.Models.DBConnect.UserModel
{
    public class UserInfo
    {
        [Key]
        [ForeignKey("Register")] // Khóa chính đồng thời là khóa ngoại
        public string? UserName { get; set; }
        public string? Avatar { get; set; } = "/images/default-avatar.png";
        public string? Phone { get; set; }
        public string? Address { get; set; }
        [JsonIgnore] // 👈 Ngăn JSON vòng lặp
        public string? UserId { get; set; }
        public Register Register { get; set; }
    }
}
