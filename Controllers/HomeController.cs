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
using System.Net.Http;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CAFE_MENU.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IDatabase _cache;
        private readonly HttpClient _httpClient;
        public HomeController(AppDbContext context, IConnectionMultiplexer redis, HttpClient httpClient)
        {
            _context = context;
            _cache = redis.GetDatabase();
            _httpClient = httpClient;  
        }

        public async Task<IActionResult> Index(int page = 1, string searchTerm = "", int? categoryId = null)
        {
            GetDrpDown(); // Kategori dropdown'unu doldur

            string cacheKey = $"home_products_page_{page}_search_{searchTerm}_category_{categoryId}";

            // Redis'ten veriyi al
            var cachedData = await _cache.StringGetAsync(cacheKey);
            List<ProductDTO> productsList;

            // Ürünleri filtrele
            var query = _context.Products
                                .Include(p => p.Category)
                                .Where(p => (p.IsDeleted == false || p.IsDeleted == null) &&
                                            (string.IsNullOrEmpty(searchTerm) || p.ProductName.Contains(searchTerm)) &&
                                            (!categoryId.HasValue || p.CategoryId == categoryId)) // Kategori filtresi eklendi
                                .Select(p => new ProductDTO
                                {
                                    ProductId = p.ProductId,
                                    ProductName = p.ProductName,
                                    CategoryName = p.Category.CategoryName,
                                    Price = p.Price,
                                    ImagePath = p.ImagePath
                                });

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / 10.0);

            if (!cachedData.IsNullOrEmpty)
            {
                productsList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ProductDTO>>(cachedData);
            }
            else
            {
                productsList = await query.Skip((page - 1) * 10)
                                          .Take(10)
                                          .ToListAsync();

                // Redis'e kaydet (30 dakika)
                await _cache.StringSetAsync(cacheKey, Newtonsoft.Json.JsonConvert.SerializeObject(productsList), TimeSpan.FromMinutes(30));
            }

            // Döviz kurlarýný al
            var exchangeRates = await GetExchangeRates();

            ViewBag.UsdRate = exchangeRates.ContainsKey("USD") ? exchangeRates["USD"] : 1;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.SearchTerm = searchTerm;
            ViewBag.SelectedCategoryId = categoryId; // Seçili kategori bilgisini ViewBag'e ekle

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
        private async Task<Dictionary<string, decimal>> GetExchangeRates()
        {
            string url = "https://www.tcmb.gov.tr/kurlar/today.xml";
            Dictionary<string, decimal> exchangeRates = new Dictionary<string, decimal>();

            try
            {
                var response = await _httpClient.GetStringAsync(url);
                XDocument xml = XDocument.Parse(response);

                var currencies = xml.Descendants("Currency")
                                    .Where(x => x.Attribute("Kod") != null)
                                    .Select(x => new
                                    {
                                        Code = x.Attribute("Kod").Value,
                                        Rate = (decimal?)x.Element("ForexSelling") ?? 0
                                    });

                foreach (var currency in currencies)
                {
                    exchangeRates[currency.Code] = currency.Rate;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Döviz kuru alýnýrken hata oluþtu: " + ex.Message);
            }

            return exchangeRates;
        }

        public IActionResult GetDrpDown()
        {
            var categories = _context.Categories
                .Where(c => c.IsDeleted == false) // Silinmemiþ kategorileri al
                .Select(c => new SelectListItem
                {
                    Text = c.CategoryName,
                    Value = c.CategoryId.ToString()
                })
                .ToList();

            ViewBag.CategoryList = categories;
            return View();
        }


    }
}
