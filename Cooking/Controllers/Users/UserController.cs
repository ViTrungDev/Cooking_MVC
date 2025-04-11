using Cooking.Models.DBConnect;
using Cooking.Models.DOTs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cooking.Controllers.UserController.cs
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UserController:ControllerBase
    {
        private readonly CookingDBConnect _dbcontext;

        public UserController(CookingDBConnect dbcontext)
        {
            _dbcontext = dbcontext;
        }

        // GET: api/user/all
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
                    Address = u.Address ?? ""
                })
                .ToListAsync();

            return Ok(users);
        }


        // GET: api/user/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(string id)
        {
            var user = await _dbcontext.UserInfo.FindAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "Không tìm thấy người dùng!" });
            }
            return Ok(user);
        }

        // PUT: api/user/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UserModelDTO request)
        {
            var user = await _dbcontext.UserInfo.FindAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "Không tìm thấy người dùng!" });
            }

            // Cập nhật thông tin
            user.UserName = request.UserName ?? user.UserName;
            user.Avatar = request.Avatar ?? user.Avatar;
            user.Phone = request.Phone ?? user.Phone;
            user.Address = request.Address ?? user.Address;

            await _dbcontext.SaveChangesAsync();
            return Ok(new { message = "Cập nhật thành công!", user });
        }

        // DELETE: api/user/{id}
        [Authorize(Policy ="AdminOnly")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _dbcontext.UserInfo.FindAsync(id);
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
