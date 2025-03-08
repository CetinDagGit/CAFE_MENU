using Microsoft.AspNetCore.Mvc;
using CAFE_MENU.Extensions;
using CAFE_MENU.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Newtonsoft.Json;

public class CartController : Controller
{
    private readonly AppDbContext _context;

    public CartController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("Index")]
    public IActionResult Index()
    {
        var cart = HttpContext.Session.GetObject<List<int>>("Cart") ?? new List<int>();  // Cart tipi int olarak kaldı
        var products = _context.Products.Where(p => cart.Contains((int)p.ProductId)).ToList();  // ProductId'yi int'e dönüştürme
        var totalPrice = products.Sum(p => p.Price);

        ViewBag.TotalPrice = totalPrice;  
        return View(products);  
    }

    [HttpGet("Remove/{id}")]
    public IActionResult Remove(int id)
    {
        var cart = HttpContext.Session.GetObject<List<int>>("Cart") ?? new List<int>();

        if (cart.Contains(id))
        {
            cart.Remove(id);
            HttpContext.Session.SetObject("Cart", cart);
        }

        return RedirectToAction("Index");
    }

    [HttpPost("Checkout")]
    public IActionResult Checkout()
    {
        var cart = HttpContext.Session.GetObject<List<int>>("Cart") ?? new List<int>();
        var products = _context.Products.Where(p => cart.Contains((int)p.ProductId)).ToList();
        var totalPrice = products.Sum(p => p.Price);

        if (products.Any())
        {
            var cartItemsJson = JsonConvert.SerializeObject(products.Select(p => new { p.ProductId, p.ProductName, p.Price }).ToList());

            var userName = HttpContext.User.Identity.Name ?? "Guest";  

            var order = new Order
            {
                UserName = userName,
                OrderDate = DateTime.Now,
                CartItems = cartItemsJson,
                TotalPrice = totalPrice
            };

            _context.Orders.Add(order);
            _context.SaveChanges();

            HttpContext.Session.SetObject("Cart", new List<int>());

            return RedirectToAction("Index", "Home");  
        }

        return RedirectToAction("Index"); 
    }

}
