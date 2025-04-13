using Cooking.Helpers;
using Cooking.Models.DBConnect;
using Cooking.Models.DBConnect.Evaluate;
using Cooking.Models.DOTs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Cooking.Controllers.Evaluate
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class EvaluateController : ControllerBase
    {
        private readonly CookingDBConnect _dbcontext;

        public EvaluateController(CookingDBConnect dbcontext)
        {
            _dbcontext = dbcontext;
        }

        // Lấy tất cả đánh giá theo ProductId
        [HttpGet("product/{productId}")]
        public async Task<IActionResult> GetEvaluatesByProductId(int productId)
        {
            var evaluates = await _dbcontext.Evaluate
                .Include(e => e.User)
                .Where(e => e.ProductId == productId) 
                .Select(e => new
                {
                    e.EvaluateId,
                    e.UserId,
                    e.ProductId,
                    e.Comment,
                    Avatar = e.User.Avatar ?? "/images/default-avatar.png",
                    UserName = e.User.UserName
                })
                .ToListAsync();

            return Ok(evaluates);
        }


        // Lấy đánh giá theo ID
        [HttpGet("evaluate/{id}")]
        public async Task<IActionResult> GetEvaluateById(string id)
        {
            var evaluate = await _dbcontext.Evaluate
                .Include(e => e.User)
                .FirstOrDefaultAsync(e => e.EvaluateId == id);

            if (evaluate == null)
            {
                return NotFound(new { message = "Không tìm thấy đánh giá!" });
            }

            return Ok(new
            {
                evaluate.EvaluateId,
                evaluate.UserId,
                evaluate.ProductId,
                evaluate.Comment,
                Avatar = evaluate.User.Avatar ?? "/images/default-avatar.png",
                UserName = evaluate.User.UserName
            });
        }

        // Tạo đánh giá (Chỉ khi đã mua sản phẩm)
        [HttpPost("evaluate")]
        public async Task<IActionResult> AddEvaluate([FromBody] EvaluateModuleDTO request)
        {
            // Kiểm tra xem người dùng đã mua sản phẩm chưa
            var hasPurchased = await _dbcontext.OrderDetails
                .Include(od => od.Order)
                .AnyAsync(od => od.product_id == request.ProductId && od.Order.user_id == request.UserId);

            if (!hasPurchased)
            {
                return BadRequest(new { message = "Bạn phải mua sản phẩm trước khi đánh giá." });
            }

            // Tạo id cho Evaluate mới
            var evaluateId = Cooking.Helpers.IdGenerator.GenerateId();

            var evaluate = new EvaluateModule
            {
                EvaluateId = evaluateId,
                UserId = request.UserId,
                ProductId = request.ProductId,
                Comment = request.Comment
            };

            _dbcontext.Evaluate.Add(evaluate);
            await _dbcontext.SaveChangesAsync();

            return Ok(new { message = "Đánh giá đã được thêm thành công!", evaluateId });
        }

        // Xóa đánh giá (Chỉ Admin)
        [HttpDelete("evaluate/{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> DeleteEvaluate(string id)
        {
            var evaluate = await _dbcontext.Evaluate.FindAsync(id);
            if (evaluate == null)
            {
                return NotFound(new { message = "Không tìm thấy đánh giá!" });
            }

            _dbcontext.Evaluate.Remove(evaluate);
            await _dbcontext.SaveChangesAsync();

            return Ok(new { message = "Xóa đánh giá thành công!" });
        }

        // Sửa đánh giá (Chỉ Admin)
        [HttpPut("evaluate/{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> UpdateEvaluate(string id, [FromBody] EvaluateModuleDTO request)
        {
            var evaluate = await _dbcontext.Evaluate.FindAsync(id);
            if (evaluate == null)
            {
                return NotFound(new { message = "Không tìm thấy đánh giá!" });
            }

            // Cập nhật Comment nếu có thay đổi
            if (!string.IsNullOrEmpty(request.Comment))
            {
                evaluate.Comment = request.Comment;
            }

            // Cập nhật ProductId nếu có thay đổi
            if (request.ProductId > 0) // Kiểm tra ProductId hợp lệ (lớn hơn 0)
            {
                evaluate.ProductId = request.ProductId; // Cập nhật ProductId
            }

            await _dbcontext.SaveChangesAsync();

            return Ok(new { message = "Cập nhật đánh giá thành công!", evaluate });
        }

    }
}
