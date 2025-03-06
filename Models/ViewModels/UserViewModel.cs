namespace CAFE_MENU.Models.ViewModels
{
    public class UserViewModel
    {
        public int? UserId { get; set; }  // Güncelleme için gerekebilir
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string? Password { get; set; }  // Sadece formdan veri almak için
    }
}
