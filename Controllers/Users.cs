using CAFE_MENU.Models;
using CAFE_MENU.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CAFE_MENU.Controllers
{
    public class UsersController : Controller
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        // Kullanıcı Listesi
        public async Task<IActionResult> Index()
        {
            var users = await _context.Users.ToListAsync();
            return View(users);
        }

        // Yeni Kullanıcı Ekleme Sayfasını Göster
        [HttpGet]
        public IActionResult AddUser()
        {
            return View();
        }

        // Yeni Kullanıcı Ekleme İşlemi
        [HttpPost]
        public async Task<IActionResult> AddUser(UserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (string.IsNullOrWhiteSpace(model.Password))
            {
                ModelState.AddModelError("Password", "Şifre boş olamaz.");
                return View(model);
            }

            // Salt oluştur
            var salt = GenerateSalt();
            var hashedPassword = HashPasswordWithSHA256(model.Password, salt);

            var newUser = new User
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                UserName = model.UserName,
                HashPassword = hashedPassword,
                SaltPassword = salt
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        // Kullanıcı Güncelleme Sayfasını Göster
        [HttpGet]
        public async Task<IActionResult> UpdateUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var userViewModel = new UserViewModel
            {
                UserId = user.UserId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                UserName = user.UserName
            };

            return View(userViewModel);
        }

        // Kullanıcı Güncelleme İşlemi
        [HttpPost]
        public async Task<IActionResult> UpdateUser(UserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _context.Users.FindAsync(model.UserId);
            if (user == null)
            {
                return NotFound();
            }

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.UserName = model.UserName;

            // Şifre değiştirildiyse, hashle ve kaydet
            if (!string.IsNullOrEmpty(model.Password))
            {
                user.SaltPassword = GenerateSalt();
                user.HashPassword = HashPasswordWithSHA256(model.Password, user.SaltPassword);
            }

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        // Rastgele Salt Üretme Metodu
        private static byte[] GenerateSalt()
        {
            byte[] salt = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            return salt;
        }

        // SHA256 + Salt ile şifre hashleme
        private static byte[] HashPasswordWithSHA256(string password, byte[] salt)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
                byte[] combinedBytes = passwordBytes.Concat(salt).ToArray();
                return sha256.ComputeHash(combinedBytes);
            }
        }
    }
}
