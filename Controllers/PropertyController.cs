using CAFE_MENU.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace CAFE_MENU.Controllers
{
    public class PropertyController : Controller
    {
        private readonly AppDbContext _context;

        public PropertyController(AppDbContext context)
        {
            _context = context;
        }

        // 📌 Tüm özellikleri listeleme
        public async Task<IActionResult> Index()
        {
            var properties = await _context.Properties.ToListAsync();
            return View(properties);
        }

        [HttpGet]
        public IActionResult AddProperty()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddProperty(Property model)
        {
            if (ModelState.IsValid)
            {
                _context.Properties.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> UpdateProperty(int id)
        {
            var property = await _context.Properties.FindAsync(id);
            if (property == null) return NotFound();
            return View(property);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProperty(Property model)
        {
            if (ModelState.IsValid)
            {
                _context.Properties.Update(model);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteProperty(int id)
        {
            var property = await _context.Properties.FindAsync(id);
            if (property == null) return NotFound();

            _context.Properties.Remove(property);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
