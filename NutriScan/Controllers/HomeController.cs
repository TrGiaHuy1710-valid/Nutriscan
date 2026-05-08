using Microsoft.AspNetCore.Mvc;

namespace NutriScan.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Nutrition()
        {
            return View();
        }

        public IActionResult AI()
        {
            return View();
        }

        public IActionResult Workout()
        {
            return View();
        }

        public IActionResult QRScan()
        {
            return Content("Trang quét QR");
        }
    }
}