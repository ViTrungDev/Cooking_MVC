using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;
using Cooking.Models; // Sử dụng namespace Cooking.Models nhận dữ liệu từ database
using Cooking.Models.DOTs;
using Cooking.Models.DBConnect; // Sử dụng namespace Cooking.Models.DOTs nhận dữ liệu từ client

namespace Cooking.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthenticationController : ControllerBase
    {
        private readonly CookingDBConnect _dbcontext;

        public AuthenticationController(CookingDBConnect dbcontext)
        {
            _dbcontext = dbcontext;
        }

        // API Register: /api/authentication/register
        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegisterDto request)
        {
            var existingUser = await _dbcontext.Registers.FirstOrDefaultAsync(user => user.Email == request.Email);
            if (existingUser != null)
            {
                return BadRequest("Tài khoản đã tồn tại");
            }

            // Hash password
            CreatePassWordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);

            // Create new user
            var newUser = new Register
            {
                Id = Guid.NewGuid().ToString(),
                UserName = request.Username,
                Email = request.Email,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                CreatedAt = DateTime.Now,
                IsAdmin = false
            };

            _dbcontext.Registers.Add(newUser);
            await _dbcontext.SaveChangesAsync();
            return Ok(newUser); // Trả về thông tin user mới sau khi tạo thành công
        }

        // Triển khai hàm băm mật khẩu
        private void CreatePassWordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512()) // Sử dụng thuật toán HMACSHA512 để hash mật khẩu
            {
                passwordSalt = hmac.Key; // Lưu salt (khóa bí mật)
                passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password)); // Hash mật khẩu
            }
        }
    }
}
