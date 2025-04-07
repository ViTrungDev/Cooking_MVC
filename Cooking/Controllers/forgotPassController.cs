using Cooking.Models.DBConnect;
using Cooking.Models.DOTs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using System.Net;
using DotNetEnv;

using Cooking.Helpers;


namespace Cooking.Controllers
{
    [ApiController]
    [Route("api/[Controller]")]
    public class ForgotPassController : ControllerBase
    {
        private readonly CookingDBConnect _dbcontext;
        private readonly IConfiguration _configuration;

        public ForgotPassController(CookingDBConnect dbconnect, IConfiguration configuration)
        {
            _configuration = configuration;
            _dbcontext = dbconnect;
        }

        // API forgot-password: /api/forgotpass/forgot-password
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDto request)
        {
            var user = await _dbcontext.Registers.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null)
            {
                return BadRequest(new { message = "Email không tồn tại" });
            }

            var otp = new Random().Next(100000, 999999).ToString();
            user.ResetToken = otp;
            user.ResetTokenExpires = DateTime.UtcNow.AddMinutes(10); // Otp hết hạn trong 10 phút
            await _dbcontext.SaveChangesAsync();

            // Gửi email xác nhận
            try
            {
                await SendEmailAsync(user.Email, "Mã xác nhận đặt lại mật khẩu", $"Mã OTP của bạn là {otp}");
                return Ok(new { message = "Mã OTP đã được gửi đến email của bạn" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Có lỗi xảy ra khi gửi email", error = ex.Message });
            }
        }
        // API comfirm password: /api/forgotpass/comfirm-password
        [HttpPost("comfirm-password")]
        public async Task<IActionResult> ComfirmPassword(ResetPasswordDto request)
        {
            var user = await _dbcontext.Registers.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null)
            {
                return BadRequest(new { message = "Email không tồn tại" });
            }
            if (user.ResetToken != request.Otp || user.ResetTokenExpires < DateTime.UtcNow) 
            {
                return BadRequest(new { message = "Mã OTP không hợp lệ hoặc đã hết hạn" });
            }
            PasswordHelper.CreatePasswordHash(request.NewPassword, out byte[] passwordHash, out byte[] passwordSalt);
            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;
            user.ResetToken = null; // Xóa mã OTP sau khi đặt lại mật khẩu
            user.ResetTokenExpires = null; // Xóa thời gian hết hạn
            await _dbcontext.SaveChangesAsync();
            return Ok(new { message = "Mật khẩu đã được đặt lại thành công" });
        }

        private async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var senderEmail = Env.GetString("EMAIL");
            var senderPassword = Env.GetString("PASS_EMAIL");

            try
            {
                using (var client = new SmtpClient("smtp.gmail.com"))
                {
                    client.Port = 587;  // Cổng 587 cho STARTTLS
                    client.Credentials = new NetworkCredential(senderEmail, senderPassword); // Mật khẩu ứng dụng
                    client.EnableSsl = true;  // Bật SSL (phù hợp với cổng 587)

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(senderEmail),
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = true
                    };

                    mailMessage.To.Add(toEmail);

                    await client.SendMailAsync(mailMessage);
                    Console.WriteLine("Email đã được gửi thành công!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi gửi email: {ex.Message}");
                throw new Exception($"Lỗi khi gửi email: {ex.Message}");
            }
        }

    }
}