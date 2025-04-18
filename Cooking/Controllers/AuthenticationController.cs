﻿using Microsoft.AspNetCore.Mvc;
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

            var accessToken = CreateAccessToken(user);
            var refreshToken = CreateRefreshToken();

            user.RefreshTokens.Add(refreshToken);
            user.RefreshTokens.RemoveAll(t => t.Expires < DateTime.UtcNow || t.IsRevoked);

            await _dbcontext.SaveChangesAsync();

            return Ok(new
            {
                message = "Đăng nhập thành công!",
                accessToken,
                refreshToken = refreshToken.Token,
                userId = user.Id,
                userName = user.UserName
            });
        }

        // API Refresh Token: /api/authentication/refresh-token
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken(string refreshToken)
        {
            var user = await _dbcontext.Registers
                .Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(u => u.RefreshTokens.Any(t => t.Token == refreshToken));

            if (user == null) return Unauthorized(new { message = "Token không hợp lệ!" });

            var storedToken = user.RefreshTokens.FirstOrDefault(t => t.Token == refreshToken);
            if (storedToken == null || storedToken.Expires < DateTime.UtcNow || storedToken.IsRevoked)
                return Unauthorized(new { message = "Token không hợp lệ hoặc đã hết hạn!" });

            storedToken.IsRevoked = true;
            storedToken.RevokedAt = DateTime.UtcNow;

            var newRefreshToken = CreateRefreshToken();
            user.RefreshTokens.Add(newRefreshToken);

            await _dbcontext.SaveChangesAsync();

            return Ok(new
            {
                accessToken = CreateAccessToken(user),
                refreshToken = newRefreshToken.Token
            });
        }

        // API Logout: /api/authentication/logout
        [HttpPost("logout")]
        public async Task<IActionResult> Logout(string userId, string refreshToken)
        {
            var user = await _dbcontext.Registers
                .Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return BadRequest(new { message = "Người dùng không tồn tại!" });

            var token = user.RefreshTokens.FirstOrDefault(t => t.Token == refreshToken);
            if (token != null)
            {
                token.IsRevoked = true;
                token.RevokedAt = DateTime.UtcNow;
                await _dbcontext.SaveChangesAsync();
            }

            return Ok(new { message = "Đăng xuất thành công!" });
        }

        // Tạo Access Token
        private string CreateAccessToken(Register user)
        {
            var claims = new[]
            {
        new Claim(ClaimTypes.NameIdentifier, user.Id),
        new Claim(ClaimTypes.Name, user.UserName),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim("isAdmin", user.IsAdmin.ToString())
    };

            // Lấy các thông tin từ file .env thông qua IConfiguration
            var secretKey = _configuration["JWT_SECRET_KEY"];
            var issuer = _configuration["JWT_ISSUER"];
            var audience = _configuration["JWT_AUDIENCE"];

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(15),
                SigningCredentials = credentials,
                Issuer = issuer,
                Audience = audience
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
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
