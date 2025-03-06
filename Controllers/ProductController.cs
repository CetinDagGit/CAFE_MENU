using CAFE_MENU.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CAFE_MENU.Controllers
{
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;

        public ProductController(AppDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            var products = await _context.Products
                .Include(p => p.Category) 
                .Where(p => p.IsDeleted == false || p.IsDeleted == null) 
                .ToListAsync();

            return View(products);
        }
        [HttpGet]
        public IActionResult AddProduct()
        {
            GetDrpDown();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddProduct(Product model)
        {
            if (!ModelState.IsValid)
            {
                GetDrpDown();
                return View(model);
            }

            model.CreatedDate = DateTime.Now;
            model.IsDeleted = false;

            _context.Products.Add(model);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> UpdateProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProduct(Product model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var product = await _context.Products.FindAsync(model.ProductId);
            if (product == null)
            {
                return NotFound();
            }

            // Güncellenen alanları atama
            product.ProductName = model.ProductName;
            product.CategoryId = model.CategoryId;
            product.Price = model.Price;
            product.ImagePath = model.ImagePath;

            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            product.IsDeleted = true; // Veriyi silmek yerine işaretliyoruz
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

    }
}
