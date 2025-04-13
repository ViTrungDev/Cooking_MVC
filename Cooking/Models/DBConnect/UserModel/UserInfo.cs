using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Cooking.Models.DBConnect.UserModel
{
    public class UserInfo
    {
        [Key]
        [ForeignKey("Register")] // vừa là khóa chính vừa là khóa ngoại
        public string? UserId { get; set; }

        public string? Avatar { get; set; } = "/images/default-avatar.png";
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? Email { get; set; }

        public string? UserName { get; set; }
        [JsonIgnore]
        public Register Register { get; set; } = null!;
    }
}
