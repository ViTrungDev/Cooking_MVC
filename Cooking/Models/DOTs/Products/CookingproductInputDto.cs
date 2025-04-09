namespace Cooking.Models.DOTs.Products
{
    public class CookingproductInputDto
    {
        public string name { get; set; } = string.Empty;
        public IFormFile? image { get; set; }
        public double price { get; set; }
        public override string ToString()
        {
            return $"Name: {name}, Image: {image}, Price: {price}";
        }

    }
}
