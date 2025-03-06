using CAFE_MENU.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CAFE_MENU.Controllers
{
    public class LoginController : Controller
    {
        private readonly AppDbContext _context;

        public LoginController(AppDbContext context)
        {
            _context = context;
        }

        // Login Sayfasını Göster
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        // Kullanıcı giriş işlemi
        [HttpPost]
        public async Task<IActionResult> Index(LoginRequest model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Kullanıcıyı veritabanından çek
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserName == model.UserName);

            if (user == null)
            {
                ModelState.AddModelError("", "Geçersiz kullanıcı adı veya şifre.");
                return View(model);
            }

            // Kullanıcının salt değerini al
            var salt = user.SaltPassword;

            // Kullanıcının hash ve salt değerlerini al
            var hashedPasswordFromUser = HashPasswordWithSHA256(model.Password, salt);

            // Hash değerlerini logla
            ViewData["ComputedHash"] = BitConverter.ToString(hashedPasswordFromUser);
            ViewData["DBHash"] = BitConverter.ToString(user.HashPassword);

            // Eğer hashler uyuşmazsa hata mesajı göster
            if (!hashedPasswordFromUser.SequenceEqual(user.HashPassword))
            {
                ViewData["HashComparisonResult"] = "❌ Hashler uyuşmuyor! Giriş başarısız.";
                ModelState.AddModelError("", "Geçersiz kullanıcı adı veya şifre.");
                return View(model);
            }

            ViewData["HashComparisonResult"] = "✅ Hashler eşleşti! Giriş başarılı.";

            //// Kullanıcı doğrulandı, oturumu başlat
            //HttpContext.Session.SetString("UserId", user.UserId.ToString());
            //HttpContext.Session.SetString("UserName", user.UserName);
            if (user.UserName == "admin")
                return RedirectToAction("Index", "Admin");
            else
                return RedirectToAction("Index", "Home");
        }

        // Kullanıcı çıkış işlemi
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Login");
        }

        // SHA256 + Salt ile şifre hashleme
        private static byte[] HashPasswordWithSHA256(string password, byte[] salt)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                // Şifre ve Salt birleştiriliyor
                byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
                byte[] combinedBytes = passwordBytes.Concat(salt).ToArray();

                // Hash hesaplanıyor
                return sha256.ComputeHash(combinedBytes);
            }
        }
    }
}
