using Cooking.Models.DBConnect;
using Cooking.Models.DOTs;
using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace Cooking.Helpers
{
    public class Path_image_conten
    {
        private readonly CookingDBConnect _dbcontext;
        private readonly IConfiguration _configuration;

        public Path_image_conten(CookingDBConnect dbcontext, IConfiguration configuration)
        {
            _dbcontext = dbcontext;
            _configuration = configuration;
        }

        public async Task<IActionResult> CreatePath([FromForm] BlogModelDTO request)
        {
            // Tạo tên file
            string imageName = $"{Guid.NewGuid()}{Path.GetExtension(request.image.FileName)}";
            string contentName = $"{Guid.NewGuid()}{Path.GetExtension(request.content.FileName)}";

            // Tạo đường dẫn thư mục
            string imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
            string contentPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "content");

            // Tạo thư mục nếu chưa có
            if (!Directory.Exists(imagePath))
                Directory.CreateDirectory(imagePath);

            if (!Directory.Exists(contentPath))
                Directory.CreateDirectory(contentPath);

            // Lưu file ảnh
            string imageFullPath = Path.Combine(imagePath, imageName);
            using (var stream = new FileStream(imageFullPath, FileMode.Create))
            {
                await request.image.CopyToAsync(stream);
            }

            // Lưu file nội dung
            string contentFullPath = Path.Combine(contentPath, contentName);
            using (var stream = new FileStream(contentFullPath, FileMode.Create))
            {
                await request.content.CopyToAsync(stream);
            }

            // Trả kết quả về client
            return new JsonResult(new
            {
                Image = imageName,
                Content = contentName
            });
        }
    }
}
