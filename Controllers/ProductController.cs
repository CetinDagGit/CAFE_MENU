using CAFE_MENU.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using StackExchange.Redis;
using Newtonsoft.Json;
using CAFE_MENU.Models.DTOs;
using System.Net.Http;
using System.Xml.Linq;

namespace CAFE_MENU.Controllers
{
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IDatabase _cache;

        public ProductController(AppDbContext context, IConnectionMultiplexer redis)
        {
            _context = context;
            _cache = redis.GetDatabase();
        }
        public async Task<IActionResult> Index(int page = 1)
        {
            int pageSize = 10;
            string cacheKey = $"products_page_{page}";

            // Get data from Redis
            var cachedData = await _cache.StringGetAsync(cacheKey);
            if (!cachedData.IsNullOrEmpty)
            {
                var products = JsonConvert.DeserializeObject<List<ProductDTO>>(cachedData);
                ViewData["CurrentPage"] = page;
                ViewData["TotalPages"] = (int)Math.Ceiling((double)await _context.Products.CountAsync() / pageSize);

                // Get USD exchange rate
                decimal UsdRate = await GetExchangeRateAsync("USD");
                ViewData["UsdRate"] = UsdRate;

                return View(products);
            }

            var query = _context.Products
                                .Include(p => p.Category)
                                .Where(p => p.IsDeleted == false || p.IsDeleted == null)
                                .Select(p => new ProductDTO
                                {
                                    ProductId = p.ProductId,
                                    ProductName = p.ProductName,
                                    CategoryName = p.Category.CategoryName,
                                    Price = p.Price,
                                    ImagePath = p.ImagePath
                                });

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var productsList = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Save to Redis (30 minutes)
            await _cache.StringSetAsync(cacheKey, JsonConvert.SerializeObject(productsList), TimeSpan.FromMinutes(30));

            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"] = totalPages;

            // Get USD exchange rate
            decimal usdRate = await GetExchangeRateAsync("USD");
            ViewData["UsdRate"] = usdRate;

            return View(productsList);
        }


        [HttpGet]
        public IActionResult AddProduct()
        {
            GetDrpDown();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddProduct(Product model, IFormFile imageFile)
        {
            GetDrpDown();

            if (imageFile != null && imageFile.Length > 0)
            {
                string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "ProductImages");
                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                string filePath = Path.Combine(uploadPath, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                model.ImagePath = "/ProductImages/" + uniqueFileName;
            }

            model.CreatedDate = DateTime.Now;
            model.IsDeleted = false;

            _context.Products.Add(model);
            await _context.SaveChangesAsync();

            await ClearProductCache();

            return RedirectToAction("Index");
        }

        private async Task ClearProductCache()
        {
            for (int i = 1; i <= 100; i++)
            {
                string cacheKey = $"products_page_{i}";
                await _cache.KeyDeleteAsync(cacheKey);
            }
        }

        [HttpGet]
        public async Task<IActionResult> UpdateProduct(int id)
        {
            long longId = Convert.ToInt64(id); 
            var product = await _context.Products.FindAsync(longId); 
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProduct(Product model, IFormFile imageFile)
        {
            var product = await _context.Products.FindAsync(model.ProductId);
            if (product == null)
            {
                return NotFound();
            }

            if (imageFile != null && imageFile.Length > 0)
            {
                string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "ProductImages");

                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                string filePath = Path.Combine(uploadPath, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                if (!string.IsNullOrEmpty(product.ImagePath))
                {
                    string oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", product.ImagePath.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                product.ImagePath = "/ProductImages/" + uniqueFileName;
            }

            product.ProductName = model.ProductName;
            product.CategoryId = model.CategoryId;
            product.Price = model.Price;

            await _context.SaveChangesAsync();

            await ClearProductCache();

            return RedirectToAction("Index");
        }


        [HttpPost]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            long longId = Convert.ToInt64(id);
            var product = await _context.Products.FindAsync(longId);

            if (product == null)
            {
                return NotFound();
            }

            product.IsDeleted = true;
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        public IActionResult GetDrpDown()
        {
            List<SelectListItem> categories = _context.Categories
                .Where(c => c.IsDeleted == false || c.IsDeleted == null)
                .Select(c => new SelectListItem
                {
                    Value = c.CategoryId.ToString(),
                    Text = c.CategoryName
                })
                .ToList();

            ViewBag.Categories = categories;

            return View();
        }
        public async Task<decimal> GetExchangeRateAsync(string currencyCode)
        {
            string url = "https://www.tcmb.gov.tr/kurlar/today.xml";
            using (HttpClient client = new HttpClient())
            {
                var response = await client.GetStringAsync(url);
                XDocument xmlDoc = XDocument.Parse(response);

                var currency = xmlDoc.Descendants("Currency")
                                     .FirstOrDefault(x => (string)x.Attribute("CurrencyCode") == currencyCode);

                if (currency != null)
                {
                    decimal forexBuying = decimal.Parse(currency.Element("ForexBuying").Value.Replace('.', ','));
                    return forexBuying;
                }
                else
                {
                    throw new Exception("Currency not found.");
                }
            }
        }

    }
}
