using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers
{
    [Authorize(Roles = "Cashier")]
    public class CashierHomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
