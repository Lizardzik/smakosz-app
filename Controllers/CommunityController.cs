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

        // GET: /Community/Index (Lista wszystkich użytkowników z wyszukiwarką i paginacją)
        [HttpGet]
        public async Task<IActionResult> Index(string? search, int page = 1)
        {
            int pageSize = 12;

            var query = _context.Users.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(u => u.Login.Contains(search));
            }

            int totalUsers = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalUsers / (double)pageSize);

            page = Math.Max(1, Math.Min(page, Math.Max(1, totalPages)));

            var users = await query
                .OrderByDescending(u => u.ReputationPoints)
                .ThenBy(u => u.Login)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentSearch = search ?? string.Empty;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalUsers = totalUsers;

            return View(users);
        }

        // GET: /Community/Leaderboard (Wyświetlanie rankingu społeczności)
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