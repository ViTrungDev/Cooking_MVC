using Cooking.Models.DBConnect;
using Cooking.Models.DBConnect.Order;
using Cooking.Models.DOTs.OrderDto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cooking.Controllers.Order
{
    [Authorize] // Yêu cầu đã đăng nhập mới được dùng
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly CookingDBConnect _dbcontext;
        private readonly IConfiguration _configuration;

        public OrderController(CookingDBConnect dbcontext, IConfiguration configuration)
        {
            _dbcontext = dbcontext;
            _configuration = configuration;
        }
        // API Create: /api/order/create
        [Authorize(Policy = "AdminOrUser")]
        [HttpPost("create")]
        public async Task<IActionResult> CreateOrder([FromBody] OrderInputDto request)
        {
            var userId = User.Claims.FirstOrDefault(typeUser => typeUser.Type == "UserId")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "Không tìm thấy thông tin người dùng." });
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Tạo ID ngẫu nhiên 8 chữ số
            string orderId = GenerateRandomId();

            // Tính tổng tiền
            decimal totalAmount = request.details.Sum(total => total.price * total.quantity);

            // Tạo đối tượng Order
            var order = new OrderModel
            {
                id = orderId,
                customer_name = request.customer_name,
                phone = request.phone,
                address = request.address,
                total_amount = totalAmount,
                order_date = DateTime.Now,
                status = OrderStatus.Pending.ToString(),
                user_id = userId,
            };

            // Thêm order vào database
            _dbcontext.Orders.Add(order);

            // Tạo danh sách chi tiết đơn hàng
            foreach (var item in request.details)
            {
                var detail = new OrderDetail
                {
                    order_id = orderId,
                    product_id = item.product_id,
                    size = item.size,
                    quantity = item.quantity,
                    price = item.price
                };
                _dbcontext.OrderDetails.Add(detail);
            }

            await _dbcontext.SaveChangesAsync();

            return Ok(new { message = "Đặt hàng thành công!", orderId = orderId });
        }

        private string GenerateRandomId()
        {
            Random random = new Random();
            return random.Next(10000000, 99999999).ToString();
        }
        // API Get: /api/order/getALL
        [Authorize(Roles = "Admin")] // Chỉ cho phép admin truy cập
        [HttpGet("getAll")]
        public async Task<IActionResult> GetAll()
        {
            var orders = await _dbcontext.Orders.ToListAsync();
            return Ok(orders);
        }
        // API GET: /api/order/getAllbyID
        [Authorize(Policy = "AdminOrUser")]
        [HttpGet("getAllbyId")]
        public async Task<IActionResult> GetAllByID([FromQuery] string? user_id)
        {
            var currentUserId = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            var role = User.Claims.FirstOrDefault(c => c.Type == "Role")?.Value;

            if (string.IsNullOrEmpty(currentUserId))
                return Unauthorized(new { message = "Không tìm thấy thông tin người dùng." });

            IQueryable<OrderModel> query = _dbcontext.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product);

            if (role == "Admin")
            {
                if (!string.IsNullOrEmpty(user_id))
                {
                    query = query.Where(o => o.user_id == user_id);
                }
            }
            else
            {
                query = query.Where(o => o.user_id == currentUserId);
            }

            var orders = await query.ToListAsync();

            if (orders == null || orders.Count == 0)
            {
                return NotFound(new { message = "Không tìm thấy đơn hàng nào." });
            }

            var result = orders.Select(order => new
            {
                order.id,
                order.customer_name,
                order.phone,
                order.address,
                order.total_amount,
                order.order_date,
                order.status,
                order.user_id,
                products = order.OrderDetails.Select(od => new
                {
                    od.product_id,
                    product_name = od.Product.name,
                    od.quantity,
                    od.price
                }).ToList()
            });

            return Ok(result);
        }
        // PUT /api/update/{id}
        [Authorize(Policy = "AdminOrUser")]
        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateOrder(string id, [FromBody] OrderUpdateDto updatedOrder)
        {
            var existingOrder = await _dbcontext.Orders.FirstOrDefaultAsync(order => order.id == id);
            if (existingOrder == null)
            {
                return NotFound(new { message = "Không tìm thấy đơn hàng." });
            }

            var currentUserId = User.Claims.FirstOrDefault(typeUser => typeUser.Type == "UserId")?.Value;
            var role = User.Claims.FirstOrDefault(typeUser => typeUser.Type == "Role")?.Value;

            if (role == "User")
            {
                if (existingOrder.user_id != currentUserId)
                    return Forbid("Bạn không có quyền cập nhật đơn hàng này.");

                if (!string.IsNullOrWhiteSpace(updatedOrder.customer_name))
                    existingOrder.customer_name = updatedOrder.customer_name;

                if (!string.IsNullOrWhiteSpace(updatedOrder.phone))
                    existingOrder.phone = updatedOrder.phone;

                if (!string.IsNullOrWhiteSpace(updatedOrder.address))
                    existingOrder.address = updatedOrder.address;

                if (updatedOrder.status == OrderStatus.Cancelled.ToString())
                    existingOrder.status = OrderStatus.Cancelled.ToString();
            }
            else if (role == "Admin")
            {
                if (!string.IsNullOrWhiteSpace(updatedOrder.customer_name))
                    existingOrder.customer_name = updatedOrder.customer_name;

                if (!string.IsNullOrWhiteSpace(updatedOrder.phone))
                    existingOrder.phone = updatedOrder.phone;

                if (!string.IsNullOrWhiteSpace(updatedOrder.address))
                    existingOrder.address = updatedOrder.address;

                if (!string.IsNullOrWhiteSpace(updatedOrder.status))
                    existingOrder.status = updatedOrder.status;
            }

            try
            {
                await _dbcontext.SaveChangesAsync();
                return Ok(new { message = "Cập nhật đơn hàng thành công.", order = existingOrder });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi cập nhật đơn hàng.", error = ex.Message });
            }
        }

    }
}