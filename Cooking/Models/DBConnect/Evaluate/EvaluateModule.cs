using Cooking.Models.DBConnect.UserModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cooking.Models.DBConnect.Evaluate
{
    public class EvaluateModule
    {
        [Key]
        public string EvaluateId { get; set; }

        [Required]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual UserInfo User { get; set; }

        [Required]
        public int? ProductId { get; set; }

        [ForeignKey("ProductId")]
        public virtual Cookingproduct Product { get; set; }

        [Required]
        public string Comment { get; set; }
    }
}
