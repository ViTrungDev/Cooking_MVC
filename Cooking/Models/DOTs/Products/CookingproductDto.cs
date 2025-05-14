namespace Cooking.Models.DOTs.Products
{
    public class CookingproductDto
    {
        // return frontend
        public int id { get; set; }
        public string name { get; set; } = string.Empty;
        public string? image { get; set; }
        public double price { get; set; }
        public string? classify { get; set; }

    }
}
