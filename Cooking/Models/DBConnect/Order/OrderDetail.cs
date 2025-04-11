using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cooking.Models.DBConnect.Order
{
    public class OrderDetail
    {
        [Key]
        public int id { get; set; }

        [Required]
        [StringLength(8)]
        public string order_id { get; set; }

        [ForeignKey("order_id")]
        public OrderModel Order { get; set; }

        public int product_id { get; set; }

        [ForeignKey("product_id")]
        public Cookingproduct Product { get; set; }

        public int size { get; set; }

        [Required]
        public int quantity { get; set; }

        [Required]
        public decimal price { get; set; }
    }
}
