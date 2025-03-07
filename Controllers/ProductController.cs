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
        public async Task<IActionResult> Index(int page = 1)
        {
            int pageSize = 10; 
            var query = _context.Products
                                 .Include(p => p.Category)
                                 .Where(p => p.IsDeleted == false || p.IsDeleted == null);

            var totalItems = await query.CountAsync();

            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var products = await query
                .Skip((page - 1) * pageSize)  
                .Take(pageSize)  
                .ToListAsync();

            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"] = totalPages;

            return View(products);
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
                {
                    Directory.CreateDirectory(uploadPath);
                }

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

            return RedirectToAction("Index");
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

            // Eğer kullanıcı yeni bir resim yüklediyse
            if (imageFile != null && imageFile.Length > 0)
            {
                // Resmi kaydetme klasörü
                string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "ProductImages");

                // Eğer klasör yoksa oluştur
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                // Benzersiz dosya adı oluştur
                string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                string filePath = Path.Combine(uploadPath, uniqueFileName);

                // Yeni resmi belirtilen klasöre kaydet
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                // Eğer eski bir resim varsa, onu sil
                if (!string.IsNullOrEmpty(product.ImagePath))
                {
                    string oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", product.ImagePath.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                // Yeni resim yolunu kaydet
                product.ImagePath = "/ProductImages/" + uniqueFileName;
            }

            // Diğer güncellenen alanları atama
            product.ProductName = model.ProductName;
            product.CategoryId = model.CategoryId;
            product.Price = model.Price;

            await _context.SaveChangesAsync();
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
