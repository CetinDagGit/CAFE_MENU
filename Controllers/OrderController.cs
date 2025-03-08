using CAFE_MENU.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace CAFE_MENU.Controllers
{
    public class OrderController : Controller
    {
        private readonly AppDbContext _context;

        public OrderController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> OrderHistory()
        {
            var userName = HttpContext.Session.GetString("UserName");

            var orders = await _context.Orders
                .Where(o => o.UserName == userName)
                .OrderByDescending(o => o.OrderDate) 
                .ToListAsync();

            return View(orders);
        }
    }
}
