using Cooking.Models.DBConnect;
using Cooking.Models.DOTs.Products;
using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace Cooking.Controllers.Products
{
    [Authorize] // Yêu cầu đã đăng nhập mới được dùng
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly CookingDBConnect _dbcontext;
        private readonly IConfiguration _configuration;
        public ProductController(CookingDBConnect _dbcontext, IConfiguration _configuration)
        {
            this._dbcontext = _dbcontext;
            this._configuration = _configuration;
        }
        // API Create: /api/product/create
        [Authorize(Policy = "AdminOnly")]
        [HttpPost("create")]
        public async Task<IActionResult> CreateProduct([FromForm] CookingproductInputDto request)
        {
            var isAdmin = User.Claims.FirstOrDefault(c => c.Type == "IsAdmin")?.Value;

            if (isAdmin != "True")
                return Forbid("Bạn không đủ quyền để thực hiện chức năng này!");
            // Sinh ID ngẫu nhiên 6 chữ số
            int newId = GenerateUniqueId();

            // Xử lý lưu ảnh vào wwwroot/images (tạo thư mục nếu chưa có)
            string imageName = $"{Guid.NewGuid()}{Path.GetExtension(request.image.FileName)}";
            string imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");

            if (!Directory.Exists(imagePath))
            {
                Directory.CreateDirectory(imagePath);
            }

            string fullPath = Path.Combine(imagePath, imageName);
            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await request.image.CopyToAsync(stream);
            }

            // Tạo đối tượng sản phẩm
            var newProduct = new Cookingproduct
            {
                id = newId,
                name = request.name,
                image = $"/images/{imageName}", // Lưu đường dẫn tương đối để hiển thị
                price = request.price,
                size = request.size,
                quantity = request.quantity
            };

            _dbcontext.Cookingproducts.Add(newProduct);
            await _dbcontext.SaveChangesAsync();

            return Ok(new { message = "Tạo sản phẩm thành công!", newProduct });
        }
        private int GenerateUniqueId()
        {
            var random = new Random();

            while (true)
            {
                int newId = random.Next(100000, 1000000); // Random 6 chữ số
                if (!_dbcontext.Cookingproducts.Any(p => p.id == newId))
                {
                    return newId;
                }
            }
        }
        // Get ALl product: /api/product/getall
        [Authorize(Policy = "AdminOrUser")]
        [HttpGet("getall")]
        public async Task<IActionResult> Getall()
        {
            
            var products = await _dbcontext.Cookingproducts.ToListAsync();
            return Ok(new
            {
                message = "Lấy danh sách sản phẩm thành công!",
                products = products.Select(p => new CookingproductDto
                {
                    id = p.id,
                    name = p.name,
                    image = p.image,
                    price = p.price,
                    size = p.size,
                    quantity = p.quantity
                }).ToList()
            });
        }
        // Get api product{id}: /api/product/getbyid
        [Authorize(Policy = "AdminOrUser")]
        [HttpGet("getbyid/{id}")]
        public async Task<IActionResult> GetbyId(int id)
        {
          
            var product = await _dbcontext.Cookingproducts.FirstOrDefaultAsync(p => p.id == id);
            if (product == null)
            {
                return NotFound(new { message = "Sản phẩm không tồn tại!" });
            }
            if (product != null)
            {
                var productDto = new CookingproductDto
                {
                    id = product.id,
                    name = product.name,
                    image = product.image,
                    price = product.price,
                    size = product.size,
                    quantity = product.quantity
                };
                return Ok(new { message = "Lấy sản phẩm thành công!", product = productDto });
            }
            else
            {
                return NotFound(new { message = "Sản phẩm không tồn tại!" });
            }
        }
        // API Update product: /api/product/update/{id}
        [Authorize(Policy = "AdminOnly")]
        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromForm] CookingproductInputDto request)
        {
            var isAdmin = User.Claims.FirstOrDefault(c => c.Type == "IsAdmin")?.Value;

            if (isAdmin != "True")
                return Forbid("Bạn không đủ quyền để thực hiện chức năng này!");
            var product = await _dbcontext.Cookingproducts.FindAsync(id);
            if (product == null)
                return NotFound("Không tìm thấy sản phẩm.");

            product.name = request.name;
            product.price = request.price;
            product.size = request.size;
            product.quantity = request.quantity;

            if (request.image != null)
            {
                // Xóa ảnh cũ nếu có
                if (!string.IsNullOrEmpty(product.image))
                {
                    var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", product.image.TrimStart('/'));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                // Lưu ảnh mới vào wwwroot/images/
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(request.image.FileName);
                var imageFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");

                if (!Directory.Exists(imageFolder))
                {
                    Directory.CreateDirectory(imageFolder);
                }

                var filePath = Path.Combine(imageFolder, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await request.image.CopyToAsync(stream);
                }

                // Lưu đường dẫn tương đối
                product.image = $"/images/{fileName}";
            }

            await _dbcontext.SaveChangesAsync();

            return Ok("Cập nhật sản phẩm thành công.");
        }

        // API Delete product: /api/product/delete/{id}
        [Authorize(Policy = "AdminOnly")]
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var isAdmin = User.Claims.FirstOrDefault(c => c.Type == "IsAdmin")?.Value;

            if (isAdmin != "True")
                return Forbid("Bạn không đủ quyền để thực hiện chức năng này!");
            var product = await _dbcontext.Cookingproducts.FindAsync(id);
            if (product == null)
                return NotFound("Không tìm thấy sản phẩm để xoá!");

            _dbcontext.Cookingproducts.Remove(product);
            await _dbcontext.SaveChangesAsync();
            return Ok("Xoá sản phẩm thành công!");
        }

    }
}
