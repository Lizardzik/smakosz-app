using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmakoszApp.Data;
using SmakoszApp.Models;

namespace SmakoszApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        // Wyœwietlanie strony g³ównej i listy wszystkich potraw
        public async Task<IActionResult> Index()
        {
            var recipes = await _context.Recipes.ToListAsync();

            var userIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userIdString))
            {
                int userId = int.Parse(userIdString);
                ViewBag.FavoriteIds = await _context.Favorites
                    .Where(f => f.UserId == userId)
                    .Select(f => f.RecipeId)
                    .ToListAsync();
            }
            else
            {
                ViewBag.FavoriteIds = new List<int>();
            }

            return View(recipes);
        }

        // Wyœwietlanie strony kontaktowej
        public IActionResult Contact()
        {
            return View();
        }

        // Wyœwietlanie galerii zdjêæ
        public async Task<IActionResult> Gallery()
        {
            var recipes = await _context.Recipes.ToListAsync();
            return View(recipes);
        }

        // Obs³uga b³êdów aplikacji
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}