namespace CAFE_MENU.Models.DTOs
{
    public class ProductDTO
    {
        public long ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public string ImagePath { get; set; }
        public string CategoryName { get; set; }
    }

}
