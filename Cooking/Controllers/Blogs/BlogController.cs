using Cooking.Helpers;
using Cooking.Models.DBConnect;
using Cooking.Models.DBConnect.Blogs;
using Cooking.Models.DOTs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Claims;


namespace Cooking.Controllers.Blogs
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class BlogController : ControllerBase
    {
        private readonly CookingDBConnect _dbcontext;
        private readonly IConfiguration _configuration;

        public BlogController(CookingDBConnect dbcontext, IConfiguration configuration)
        {
            _dbcontext = dbcontext;
            _configuration = configuration;
        }

        // API POST create blog: /api/blog/create
        [Authorize(Policy = "AdminOnly")]
        [HttpPost("create")]
        public async Task<IActionResult> CreateBlog([FromForm] BlogModelDTO request)
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

            // Tạo model và lưu vào database
            var blog = new BlogModel()
            {
                Id = Guid.NewGuid(),
                Title = request.title,
                ImagePath = $"/images/{imageName}",
                ContentPath = $"/content/{contentName}",
                CreatedAt = DateTime.Now,
                CreatedById = userId
            };

            await _dbcontext.Blogs.AddAsync(blog);
            await _dbcontext.SaveChangesAsync();

            return Ok(new
            {
                message = "Tạo blog thành công!",
                data = new
                {
                    blog.Id,
                    blog.Title,
                    blog.ImagePath,
                    blog.ContentPath,
                    blog.CreatedAt
                }
            });
        }
        // Get all blog: /api/blog/getAll
        [AllowAnonymous]
        [HttpGet("getAll")]
        public async Task<IActionResult> getAll()
        {
            var listBlogs = await _dbcontext.Blogs.ToListAsync();
            return Ok(listBlogs);
        }
        // Get blog by id: /api/blog/getBy/{id}
        [AllowAnonymous]
        [HttpGet("get/{id}")]
        public async Task<IActionResult> getBlog(Guid id)
        {
            var FindBlog = await _dbcontext.Blogs.FirstOrDefaultAsync(blog => blog.Id == id);
            if (FindBlog == null)
            {
                return BadRequest("Không tìm thấy Blog");
            }
            return Ok(FindBlog);
        }
        // Update blog {id}:/api/blog/update/{id}
        [Authorize(Policy = "AdminOnly")]
        [Route("update/{id}")]
        [HttpPut]
        public async Task<IActionResult> updateBlog(Guid id, [FromForm] BlogModelDTO request)
        {
            var FindBlog = await _dbcontext.Blogs.FirstOrDefaultAsync(blog => blog.Id == id);
            if (FindBlog == null)
            {
                return BadRequest("Không tìm thấy blog");
            }

            var userId = User.Claims.FirstOrDefault(typeUser => typeUser.Type == "UserId")?.Value;
            if (userId == null)
            {
                return BadRequest("Không tìm thấy người dùng");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Cập nhật tiêu đề nếu có
            if (!string.IsNullOrEmpty(request.title))
            {
                FindBlog.Title = request.title;
            }

            // Đường dẫn thư mục
            string rootPath = Directory.GetCurrentDirectory();
            string imageDir = Path.Combine(rootPath, "wwwroot", "images");
            string contentDir = Path.Combine(rootPath, "wwwroot", "content");

            // Tạo thư mục nếu chưa có
            if (!Directory.Exists(imageDir)) Directory.CreateDirectory(imageDir);
            if (!Directory.Exists(contentDir)) Directory.CreateDirectory(contentDir);

            // Xử lý ảnh nếu có
            if (request.image != null)
            {
                // Xoá ảnh cũ
                if (!string.IsNullOrEmpty(FindBlog.ImagePath))
                {
                    string oldImagePath = Path.Combine(rootPath, "wwwroot", FindBlog.ImagePath.TrimStart('/'));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                // Lưu ảnh mới
                string imageName = $"{Guid.NewGuid()}{Path.GetExtension(request.image.FileName)}";
                string imageFullPath = Path.Combine(imageDir, imageName);
                using (var stream = new FileStream(imageFullPath, FileMode.Create))
                {
                    await request.image.CopyToAsync(stream);
                }
                FindBlog.ImagePath = $"/images/{imageName}";
            }

            // Xử lý nội dung nếu có
            if (request.content != null)
            {
                // Xoá nội dung cũ
                if (!string.IsNullOrEmpty(FindBlog.ContentPath))
                {
                    string oldContentPath = Path.Combine(rootPath, "wwwroot", FindBlog.ContentPath.TrimStart('/'));
                    if (System.IO.File.Exists(oldContentPath))
                    {
                        System.IO.File.Delete(oldContentPath);
                    }
                }

                // Lưu nội dung mới
                string contentName = $"{Guid.NewGuid()}{Path.GetExtension(request.content.FileName)}";
                string contentFullPath = Path.Combine(contentDir, contentName);
                using (var stream = new FileStream(contentFullPath, FileMode.Create))
                {
                    await request.content.CopyToAsync(stream);
                }
                FindBlog.ContentPath = $"/content/{contentName}";
            }


            _dbcontext.Blogs.Update(FindBlog);
            await _dbcontext.SaveChangesAsync();

            return Ok(new
            {
                message = "Cập nhật blog thành công!",
                data = new
                {
                    FindBlog.Id,
                    FindBlog.Title,
                    FindBlog.ImagePath,
                    FindBlog.ContentPath,
                    FindBlog.CreatedAt,
                }
            });

        }
        //Delete: /api/blog/delete/{id}
        [Authorize(Policy = "AdminOnly")]
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var ID_blog = await _dbcontext.Blogs.FirstOrDefaultAsync(blogId => blogId.Id == id);
            if(ID_blog == null)
            {
                return BadRequest("Không tìm thấy Blog");
            }
            _dbcontext.Blogs.Remove(ID_blog);
            await _dbcontext.SaveChangesAsync();
            return Ok("Xóa blog thành công!");
        }
    }
}

