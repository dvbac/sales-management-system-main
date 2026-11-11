namespace SalesManagement.Web.Models
{
    public class HomeProductViewModel
    {
        public IEnumerable<ProductViewModel> Products { get; set; } = new List<ProductViewModel>();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public string? SearchString { get; set; }
        public string? SortOrder { get; set; }
        public int? CategoryId { get; set; }
        public IEnumerable<CategoryViewModel> Categories { get; set; } = new List<CategoryViewModel>();
    }
}
