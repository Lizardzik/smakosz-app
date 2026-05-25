using Microsoft.AspNetCore.Mvc;

namespace SmakoszApp.Controllers
{
    public class RecipeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
