using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Cooking.Models.DBConnect.Order
{
    public enum OrderStatus
    {
        Pending,     // Đơn hàng mới
        Processing,  // Đang xử lý
        Shipped,     // Đã giao hàng
        Delivered,   // Đã giao
        Cancelled    // Đã hủy
    }

    public class OrderModel
    {
        [Key]
        [StringLength(8)]
        public string id { get; set; }

        public string customer_name { get; set; }
        public string phone { get; set; }
        public string address { get; set; }
        public decimal total_amount { get; set; }
        public DateTime order_date { get; set; }

        // Trạng thái đơn hàng mặc định là "Pending"
        public string status { get; set; } = OrderStatus.Pending.ToString();

        public string user_id { get; set; }

        //  Navigation property để truy xuất danh sách chi tiết đơn hàng
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
    }
}
