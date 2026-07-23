using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmakoszApp.Data;
using SmakoszApp.Models;
using SmakoszApp.Services;
using System.Security.Claims;

namespace SmakoszApp.Controllers
{
    public class RecipeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly MealApiService _mealApiService;

        public RecipeController(ApplicationDbContext context, MealApiService mealApiService)
        {
            _context = context;
            _mealApiService = mealApiService;
        }

        [HttpPost]
        public async Task<IActionResult> ToggleFavorite(string recipeId, string title, string thumb, string category, string area)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString)) return Unauthorized();

            int userId = int.Parse(userIdString);

            var favorite = await _context.Favorites
                .FirstOrDefaultAsync(f => f.UserId == userId && f.ExternalMealId == recipeId);

            bool isFavorite;
            if (favorite != null)
            {
                _context.Favorites.Remove(favorite);
                isFavorite = false;
            }
            else
            {
                _context.Favorites.Add(new Favorite
                {
                    UserId = userId,
                    ExternalMealId = recipeId,
                    MealName = title ?? "",
                    MealThumb = thumb ?? "",
                    Category = category ?? "",
                    Area = area ?? ""
                });
                isFavorite = true;
            }

            await _context.SaveChangesAsync();
            return Json(new { isFavorite = isFavorite });
        }

        [HttpGet]
        public async Task<IActionResult> Favorites()
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString)) return RedirectToAction("Index", "Account");

            int userId = int.Parse(userIdString);

            var favoriteMeals = await _context.Favorites
                .Where(f => f.UserId == userId)
                .OrderByDescending(f => f.AddedAt)
                .ToListAsync();

            return View(favoriteMeals);
        }

        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var meal = await _mealApiService.GetMealByIdAsync(id);
            if (meal == null) return NotFound();

            var ratings = await _context.Ratings
                .Where(r => r.ExternalMealId == id)
                .ToListAsync();

            var comments = await _context.Comments
                .Include(c => c.User)
                .Where(c => c.ExternalMealId == id)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            ViewBag.Comments = comments;

            int totalVotes = ratings.Count;
            double avgScore = totalVotes > 0 ? Math.Round(ratings.Average(r => r.Score), 1) : 0.0;

            var starBreakdown = new Dictionary<int, int>
            {
                { 5, ratings.Count(r => r.Score == 5) },
                { 4, ratings.Count(r => r.Score == 4) },
                { 3, ratings.Count(r => r.Score == 3) },
                { 2, ratings.Count(r => r.Score == 2) },
                { 1, ratings.Count(r => r.Score == 1) }
            };

            bool isFavorite = false;
            if (User.Identity?.IsAuthenticated == true)
            {
                var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userIdString) && int.TryParse(userIdString, out int userId))
                {
                    isFavorite = await _context.Favorites.AnyAsync(f => f.UserId == userId && f.ExternalMealId == id);
                }
            }

            ViewBag.AverageScore = avgScore;
            ViewBag.TotalVotes = totalVotes;
            ViewBag.StarBreakdown = starBreakdown;
            ViewBag.IsFavorite = isFavorite;

            return View(meal);
        }

        [HttpPost]
        public async Task<IActionResult> RateRecipe(string recipeId, int score)
        {
            if (score < 1 || score > 5) return BadRequest("Score must be between 1 and 5.");

            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int? userId = !string.IsNullOrEmpty(userIdString) ? int.Parse(userIdString) : null;
            string userIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";

            Rating? existingRating = null;

            if (userId.HasValue)
            {
                existingRating = await _context.Ratings
                    .FirstOrDefaultAsync(r => r.ExternalMealId == recipeId && r.UserId == userId.Value);
            }
            else
            {
                existingRating = await _context.Ratings
                    .FirstOrDefaultAsync(r => r.ExternalMealId == recipeId && r.UserId == null && r.UserIp == userIp);
            }

            bool isNewRating = (existingRating == null);

            if (existingRating != null)
            {
                existingRating.Score = score;
                existingRating.CreatedAt = DateTime.Now;
            }
            else
            {
                _context.Ratings.Add(new Rating
                {
                    ExternalMealId = recipeId,
                    Score = score,
                    UserId = userId,
                    UserIp = userIp
                });
            }

            if (userId.HasValue && isNewRating)
            {
                var user = await _context.Users.FindAsync(userId.Value);
                if (user != null)
                {
                    user.ReputationPoints += 1;
                }
            }

            await _context.SaveChangesAsync();

            var ratingsForMeal = await _context.Ratings
                .Where(r => r.ExternalMealId == recipeId)
                .ToListAsync();

            double avgScore = ratingsForMeal.Average(r => r.Score);
            int votesCount = ratingsForMeal.Count;

            return Json(new
            {
                success = true,
                average = Math.Round(avgScore, 1),
                votes = votesCount,
                userScore = score
            });
        }

        [HttpPost]
        public async Task<IActionResult> AddComment(string recipeId, string text)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString)) return Unauthorized();

            if (string.IsNullOrWhiteSpace(text) || text.Length < 3)
            {
                return BadRequest("Comment is too short.");
            }

            int userId = int.Parse(userIdString);

            var comment = new Comment
            {
                ExternalMealId = recipeId,
                UserId = userId,
                Text = text.Trim(),
                CreatedAt = DateTime.Now
            };

            _context.Comments.Add(comment);

            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                user.ReputationPoints += 10;
            }

            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                author = user?.Login ?? "User",
                text = comment.Text,
                date = comment.CreatedAt.ToString("MMM dd, yyyy HH:mm"),
                commentId = comment.Id
            });
        }

        [HttpPost]
        public async Task<IActionResult> LikeComment(int commentId)
        {
            var comment = await _context.Comments
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == commentId);

            if (comment == null) return NotFound();

            comment.LikesCount++;

            if (comment.User != null)
            {
                comment.User.ReputationPoints += 5;
            }

            await _context.SaveChangesAsync();

            return Json(new { success = true, likes = comment.LikesCount });
        }
    }
}