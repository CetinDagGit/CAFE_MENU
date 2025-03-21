﻿using CAFE_MENU.Models;
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

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Index(LoginRequest model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserName == model.UserName);

            if (user == null)
            {
                ModelState.AddModelError("", "Invalid username or password.");
                return View(model);
            }
            var salt = user.SaltPassword;

            var hashedPasswordFromUser = HashPasswordWithSHA256(model.Password, salt);

            //ViewData["ComputedHash"] = BitConverter.ToString(hashedPasswordFromUser);
            //ViewData["DBHash"] = BitConverter.ToString(user.HashPassword);

            if (!hashedPasswordFromUser.SequenceEqual(user.HashPassword))
            {
                ModelState.AddModelError("", "Invalid username or password.");
                return View(model);
            }

            HttpContext.Session.SetString("UserId", user.UserId.ToString());
            HttpContext.Session.SetString("UserName", user.UserName);
            if (user.UserName == "admin")
                return RedirectToAction("Index", "Admin");
            else
                return RedirectToAction("Index", "Home");
        }

        public IActionResult Logout()
        {
            //HttpContext.Session.Clear();
            return RedirectToAction("Index", "Login");
        }

        // SHA256 + Salt hashing
        private static byte[] HashPasswordWithSHA256(string password, byte[] salt)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                // Combine pasw and Salt 
                byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
                byte[] combinedBytes = passwordBytes.Concat(salt).ToArray();

                return sha256.ComputeHash(combinedBytes);
            }
        }
    }
}
