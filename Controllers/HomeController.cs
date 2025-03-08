using CAFE_MENU.Models;
using CAFE_MENU.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using CAFE_MENU.Extensions;
namespace CAFE_MENU.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IDatabase _cache;

        public HomeController(AppDbContext context, IConnectionMultiplexer redis)
        {
            _context = context;
            _cache = redis.GetDatabase();
        }

        public async Task<IActionResult> Index()
        {
            string cacheKey = "home_products";

            // Get data via Redis
            var cachedData = await _cache.StringGetAsync(cacheKey);
            if (!cachedData.IsNullOrEmpty)
            {
                var products = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ProductDTO>>(cachedData);
                return View(products);
            }

            var productsList = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.IsDeleted == false || p.IsDeleted == null)
                .Select(p => new ProductDTO
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    CategoryName = p.Category.CategoryName,
                    Price = p.Price,
                    ImagePath = p.ImagePath
                })
                .ToListAsync();

            // Save to resdis (30 minutes)
            await _cache.StringSetAsync(cacheKey, Newtonsoft.Json.JsonConvert.SerializeObject(productsList), TimeSpan.FromMinutes(30));

            return View(productsList);
        }

        [HttpPost]
        public IActionResult AddToCart(int productId)
        {
            List<int> cart = HttpContext.Session.GetObject<List<int>>("Cart") ?? new List<int>();
            cart.Add(productId);
            HttpContext.Session.SetObject("Cart", cart);
            return RedirectToAction("Index");
        }

    }
}
