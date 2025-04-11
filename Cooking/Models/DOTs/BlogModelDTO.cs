namespace Cooking.Models.DOTs
{
    public class BlogModelDTO
    {
        public string? title { get; set; }
        public IFormFile? image { get; set; }
        public IFormFile? content { get; set; }
    }
}
