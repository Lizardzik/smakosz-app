using Microsoft.AspNetCore.Mvc;

namespace SmakoszApp.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
