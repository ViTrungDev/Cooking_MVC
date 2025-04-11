using System.ComponentModel.DataAnnotations.Schema;

namespace Cooking.Models.DBConnect.Blogs
{
    public class BlogModel
    {

        public Guid Id { get; set; }
        public string Title { get; set; }
        [Column("image_path")]
        public string ImagePath { get; set; }
        [Column("content_path")]
        public string ContentPath { get; set; }
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        // Khóa ngoại đến Register (Admin tạo blog)
        [Column("created_by_id")]
        public string? CreatedById { get; set; }
        public Register CreatedBy { get; set; }
    }
}
