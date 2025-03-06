using CAFE_MENU.Models;
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
			return View();
		}
	}
}
