using CAFE_MENU.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CAFE_MENU.Controllers
{
    public class CategoryController : Controller
    {
        private readonly AppDbContext _context;
        private const int PageSize = 10;
        public CategoryController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            var totalCategories = await _context.Categories
                .Where(c => c.IsDeleted == false || c.IsDeleted == null)
                .CountAsync();

            var categories = await _context.Categories
                .Include(c => c.ParentCategory)
                .Where(c => c.IsDeleted == false || c.IsDeleted == null)
                .OrderBy(c => c.CategoryId)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalCategories / PageSize);

            return View(categories);
        }

        [HttpGet]
        public IActionResult AddCategory()
        {
            ViewBag.Categories = _context.Categories
                .Where(c => c.IsDeleted == false || c.IsDeleted == null)
                .ToList();

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddCategory(Category model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            model.CreatedDate = DateTime.Now;
            model.IsDeleted = false;

            _context.Categories.Add(model);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> UpdateCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            ViewBag.Categories = _context.Categories
                .Where(c => c.IsDeleted == false || c.IsDeleted == null && c.CategoryId != id) // Kendisi hariç diğer kategoriler
                .ToList();

            return View(category);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateCategory(Category model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var category = await _context.Categories.FindAsync(model.CategoryId);
            if (category == null)
            {
                return NotFound();
            }

            category.CategoryName = model.CategoryName;
            category.ParentCategoryId = model.ParentCategoryId;

            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            category.IsDeleted = true;
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    }
}
