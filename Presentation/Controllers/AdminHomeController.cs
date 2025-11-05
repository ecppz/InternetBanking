using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers
{
    public class AdminHomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
