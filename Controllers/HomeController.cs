using CLDV7111_POE_PART_1_EventEase.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace CLDV7111_POE_PART_1_EventEase.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

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
