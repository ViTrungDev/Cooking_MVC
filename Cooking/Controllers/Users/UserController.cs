using Cooking.Models.DBConnect;
using Cooking.Models.DOTs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cooking.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly CookingDBConnect _dbcontext;

        public UserController(CookingDBConnect dbcontext)
        {
            _dbcontext = dbcontext;
        }

        // GET: /api/user/all (Chỉ Admin mới xem được tất cả user)
        [HttpGet("all")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _dbcontext.UserInfo
                .Select(u => new
                {
                    u.UserId,
                    u.UserName,
                    Avatar = u.Avatar ?? "/images/default-avatar.png",
                    Phone = u.Phone ?? "",
                    Address = u.Address ?? "",
                    Email = u.Email
                })
                .ToListAsync();

            return Ok(users);
        }

        // GET: api/user/{id} (Lấy thông tin người dùng theo ID)
        [HttpGet("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> GetUserById(string id)
        {
            var user = await _dbcontext.UserInfo
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null)
            {
                return NotFound(new { message = "Không tìm thấy người dùng!" });
            }

            var userDto = new UserModelDTO
            {
                UserId = user.UserId,
                UserName = user.UserName,
                Avatar = user.Avatar ?? "/images/default-avatar.png",
                Phone = user.Phone ?? "",
                Address = user.Address ?? ""
            };

            return Ok(userDto);
        }


        // PUT: api/user/{id} (Cập nhật thông tin người dùng theo ID)
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUserById(string id, [FromBody] UserModelDTO request)
        {
            var user = await _dbcontext.UserInfo
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null)
            {
                return NotFound(new { message = "Không tìm thấy người dùng!" });
            }

            user.UserName = request.UserName ?? user.UserName;
            user.Avatar = request.Avatar ?? user.Avatar;
            user.Phone = request.Phone ?? user.Phone;
            user.Address = request.Address ?? user.Address;

            await _dbcontext.SaveChangesAsync();
            return Ok(new { message = "Cập nhật thành công!", user });
        }


        // DELETE: api/user/{id} (Xóa người dùng theo ID)
        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> DeleteUserById(string id)
        {
            var user = await _dbcontext.UserInfo
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null)
            {
                return NotFound(new { message = "Không tìm thấy người dùng!" });
            }

            _dbcontext.UserInfo.Remove(user);
            await _dbcontext.SaveChangesAsync();

            return Ok(new { message = "Xóa người dùng thành công!" });
        }

    }
}
