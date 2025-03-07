using CAFE_MENU.Models;
using CAFE_MENU.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace CAFE_MENU.Controllers
{
	public class AdminController : Controller
	{
		private readonly ILogger<HomeController> _logger;

		public AdminController(ILogger<HomeController> logger)
		{
			_logger = logger;
		}
        public IActionResult Index()
        {
            // Copilot: Create a new instance of the DashboardViewModel
            var model = new DashboardViewModel();

            // Copilot: Create a new instance of the ApplicationDbContext
            using (var context = new AppDbContext())
            {
                // Copilot: Get the count of products in each category
                model.CategoryCount = context.Products
                    .GroupBy(p => p.Category.CategoryName)  // Copilot: Group products by category name
                    .ToDictionary(g => g.Key, g => g.Count());  // Copilot: Convert the grouped results into a dictionary
            }

            // Copilot: Return the view with the model
            return View(model);
        }

    }
}
