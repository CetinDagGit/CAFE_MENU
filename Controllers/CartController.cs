using Microsoft.AspNetCore.Mvc;

namespace CAFE_MENU.Controllers
{
    public class CartController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
