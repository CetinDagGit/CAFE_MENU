using CAFE_MENU.Models;
using CAFE_MENU.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace CAFE_MENU.Controllers
{
    public class AdminController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public AdminController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        // Copilot: This method renders the main dashboard page
        public IActionResult Index()
        {
            var model = new DashboardViewModel();

            using (var context = new AppDbContext())
            {
                // Copilot: Get the count of products by category
                model.CategoryCount = context.Products
                    .GroupBy(p => p.Category.CategoryName)  // Group products by their category names
                    .ToDictionary(g => g.Key, g => g.Count());  // Convert the result to a dictionary of category name and product count

                // Copilot: Get today's total revenue from the Orders table
                var today = DateTime.Now.Date;  // Get today's date (without time component)
                model.DailyTotalRevenue = context.Orders
                    .Where(o => o.OrderDate.Date == today)  // Filter orders by today's date
                    .Sum(o => o.TotalPrice);  // Sum the total price of today's orders
            }

            // Copilot: Return the view with the model data
            return View(model);
        }

        // Copilot: This is a new API endpoint that fetches today's total revenue
        [HttpGet]
        public IActionResult GetDailyTotalRevenue()
        {
            decimal dailyTotalRevenue;
            using (var context = new AppDbContext())
            {
                // Copilot: Calculate today's total revenue from the Orders table
                var today = DateTime.Now.Date;  // Get today's date (without time component)
                dailyTotalRevenue = context.Orders
                    .Where(o => o.OrderDate.Date == today)  // Filter orders by today's date
                    .Sum(o => o.TotalPrice);  // Sum the total price of today's orders
            }

            // Copilot: Return the calculated daily total revenue as JSON
            return Json(new { dailyTotalRevenue = dailyTotalRevenue });
        }
    }
}
