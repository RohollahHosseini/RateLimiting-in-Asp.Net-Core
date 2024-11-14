using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using RateLimiting_in_MVC.Models;
using System.Diagnostics;

namespace RateLimiting_in_MVC.Controllers
{
    //استفاده کرد Atribute باید از این RateLimiting برای استفاده از 
    [EnableRateLimiting("MyRateFixed")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        [EnableRateLimiting("MyRateFixed")]
        public IActionResult Index()
        {
            return View();
        }
        
        //استفاده کنیم Attribute استفاده نکینم باید از این RateLimit اگر بخواهیم از
        [DisableRateLimiting]
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
