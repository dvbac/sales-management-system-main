namespace SalesManagement.Web.Models
{
    public class CategoryViewModel
    {
        public int CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? DisplayOrder { get; set; }
    }
}
