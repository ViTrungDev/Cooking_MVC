using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

using Cooking.Models;
using Cooking.Models.DOTs;
using Cooking.Models.DBConnect;
using Cooking.Helpers;
using Cooking.Models.DBConnect.UserModel;

namespace Cooking.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthenticationController : ControllerBase
    {
        private readonly CookingDBConnect _dbcontext;
        private readonly IConfiguration _configuration;

        public AuthenticationController(CookingDBConnect dbcontext, IConfiguration configuration)
        {
            _dbcontext = dbcontext;
            _configuration = configuration;
        }

        // API Register: /api/authentication/register
        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegisterDto request)
        {
            var existingUser = await _dbcontext.Registers.FirstOrDefaultAsync(user => user.Email == request.Email);
            if (existingUser != null)
            {
                return BadRequest(new { message = "Tài khoản đã tồn tại!" });
            }

            PasswordHelper.CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);

            // Tạo trước 1 ID dùng chung
            var userId = IdGenerator.GenerateId();

            var newUser = new Register
            {
                Id = userId,
                UserName = request.Username,
                Email = request.Email,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                CreatedAt = DateTime.Now,
                IsAdmin = false
            };

            var userInfo = new UserInfo
            {
                UserId = userId,
                UserName = newUser.UserName,
                Avatar = "/images/default-avatar.png",
                Email = newUser.Email,
                Phone = null,
                Address = null
            };

            newUser.UserInfo = userInfo;

            _dbcontext.Registers.Add(newUser);
            await _dbcontext.SaveChangesAsync();

            return Ok(new { message = "Đăng ký thành công!", newUser });
        }

        // API Login: /api/authentication/login
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDTO request)
        {
            var user = await _dbcontext.Registers
                .Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null || !PasswordHelper.VerifyPasswordHash(request.Password, user.PasswordHash, user.PasswordSalt))
                return BadRequest(new { message = "Email hoặc mật khẩu không chính xác!" });

            // Tạo accessToken và refreshToken
            var accessToken = CreateAccessToken(user);
            var refreshToken = CreateRefreshToken();

            // Thêm refresh token mới và xóa cái hết hạn
            user.RefreshTokens.Add(refreshToken);
            user.RefreshTokens.RemoveAll(t => t.Expires < DateTime.UtcNow || t.IsRevoked);

            await _dbcontext.SaveChangesAsync();
            Console.WriteLine($"[LOGIN] {DateTime.Now} - Tài khoản '{user.Email}' đã đăng nhập. Quyền: {(user.IsAdmin ? "Admin" : "Người dùng")}");

            // Lưu refreshToken vào cookie
            Response.Cookies.Append("refreshToken", refreshToken.Token, new CookieOptions
            {
                HttpOnly = true,  // Cookie chỉ có thể truy cập từ server, không thể truy cập qua JavaScript
                Secure = false,   // Đảm bảo Secure=true nếu bạn sử dụng HTTPS
                SameSite = SameSiteMode.Strict,  // Cấm gửi cookie từ các domain khác
                MaxAge = TimeSpan.FromDays(7)  // Thời gian sống của cookie (7 ngày)
            });

            return Ok(new
            {
                message = "Đăng nhập thành công!",
                accessToken,
                refreshToken = refreshToken.Token, // Trả về refresh token trong response body (nếu cần)
                userId = user.Id,
                userName = user.UserName,
                isAdmin = user.IsAdmin
            });
        }

        // API Refresh Token: /api/authentication/refresh-token
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken()
        {
            // Lấy refreshToken từ cookie
            var refreshToken = Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(refreshToken))
                return Unauthorized(new { message = "Không tìm thấy refresh token trong cookie!" });

            var user = await _dbcontext.Registers
                .Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(u => u.RefreshTokens.Any(t => t.Token == refreshToken));

            if (user == null)
                return Unauthorized(new { message = "Token không hợp lệ!" });

            var storedToken = user.RefreshTokens.FirstOrDefault(t => t.Token == refreshToken);
            if (storedToken == null || storedToken.Expires < DateTime.UtcNow || storedToken.IsRevoked)
                return Unauthorized(new { message = "Token không hợp lệ hoặc đã hết hạn!" });

            storedToken.IsRevoked = true;
            storedToken.RevokedAt = DateTime.UtcNow;

            var newRefreshToken = CreateRefreshToken();
            user.RefreshTokens.Add(newRefreshToken);

            await _dbcontext.SaveChangesAsync();

            // Lưu refreshToken mới vào cookie
            Response.Cookies.Append("refreshToken", newRefreshToken.Token, new CookieOptions
            {
                HttpOnly = true,
                Secure = false,  // Đảm bảo Secure=true nếu bạn sử dụng HTTPS
                SameSite = SameSiteMode.Strict,
                MaxAge = TimeSpan.FromDays(7)
            });

            return Ok(new
            {
                accessToken = CreateAccessToken(user),
                refreshToken = newRefreshToken.Token
            });
        }

        // API Logout: /api/authentication/logout
        [HttpPost("logout")]
        public async Task<IActionResult> Logout(string userId)
        {
            var user = await _dbcontext.Registers
                .Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return BadRequest(new { message = "Người dùng không tồn tại!" });

            // Xóa refreshToken trong cookie
            Response.Cookies.Delete("refreshToken");

            return Ok(new { message = "Đăng xuất thành công!" });
        }

        // Tạo Access Token
        private string CreateAccessToken(Register user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.Email),
                new Claim("Role", user.IsAdmin ? "Admin" : "User"),
                new Claim("UserId", user.Id.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT_SECRET_KEY"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT_ISSUER"],
                audience: _configuration["JWT_AUDIENCE"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(30),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // Tạo Refresh Token
        private RefreshToken CreateRefreshToken()
        {
            return new RefreshToken
            {
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                Expires = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow
            };
        }
    }
}
