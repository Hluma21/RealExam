using Microsoft.AspNetCore.Mvc;

namespace Greenfield.Controllers
{
    public class LoyaltyController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
