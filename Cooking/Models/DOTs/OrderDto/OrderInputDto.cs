using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace Cooking.Models.DOTs.OrderDto
{
    // cliendt gửi yêu cầu tạo đơn hàng
    public class OrderInputDto
    {
        [Required]
        public string customer_name { get; set; }

        public string phone { get; set; }

        public string address { get; set; }

        [Required]
        public List<OrderDetailInputDto> details { get; set; }
    }

    public class OrderDetailInputDto
    {
        [Required]
        public int product_id { get; set; }

        [Required]
        public int size { get; set; }

        [Required]
        public int quantity { get; set; }

        [Required]
        public decimal price { get; set; }
    }
}

