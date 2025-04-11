using System;
using System.Collections.Generic;
namespace Cooking.Models.DOTs.OrderDto
{
    public class OrderDto
    {
        public string id { get; set; }
        public string customer_name { get; set; }
        public string phone { get; set; }
        public string address { get; set; }
        public decimal total_amount { get; set; }
        public DateTime order_date { get; set; }
        public string status { get; set; }

        public List<OrderDetailDto> details { get; set; }
    }

    public class OrderDetailDto
    {
        public int product_id { get; set; }
        public string product_name { get; set; } // gợi ý thêm tên món ăn
        public string image { get; set; }        // gợi ý thêm ảnh món ăn
        public int size { get; set; }
        public int quantity { get; set; }
        public decimal price { get; set; }
    }
}
