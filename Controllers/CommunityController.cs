using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmakoszApp.Data;

namespace SmakoszApp.Controllers
{
    public class CommunityController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CommunityController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Wyświetlanie rankingu społeczności (Leaderboard)
        [HttpGet]
        public async Task<IActionResult> Leaderboard()
        {
            var topUsers = await _context.Users
                .OrderByDescending(u => u.ReputationPoints)
                .Take(10)
                .ToListAsync();

            return View(topUsers);
        }
    }
}