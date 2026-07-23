using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmakoszApp.Data;
using SmakoszApp.Models;
using SmakoszApp.Services;
using System.Diagnostics;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;

namespace SmakoszApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly MealApiService _mealApiService;

        public HomeController(
            ILogger<HomeController> logger,
            ApplicationDbContext context,
            MealApiService mealApiService,
            IConfiguration configuration)
        {
            _logger = logger;
            _context = context;
            _mealApiService = mealApiService;
            _configuration = configuration;
        }

        // Odpowiada za wyœwietlanie strony g³ównej, wyszukiwanie, filtrowanie, paginacjê, pobieranie ulubionych oraz przeliczanie ocen przepisów
        public async Task<IActionResult> Index(string search = "", string category = "", string cuisine = "", int page = 1)
        {
            int pageSize = 12;
            List<ExternalMeal> allMeals;

            if (!string.IsNullOrWhiteSpace(search))
            {
                allMeals = await _mealApiService.SearchMealsAsync(search);
            }
            else
            {
                allMeals = await _mealApiService.GetCatalogMealsAsync();
            }

            if (!string.IsNullOrWhiteSpace(category))
            {
                allMeals = allMeals.Where(m => (m.StrCategory ?? "").Equals(category, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (!string.IsNullOrWhiteSpace(cuisine))
            {
                allMeals = allMeals.Where(m => (m.StrArea ?? "").Equals(cuisine, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            int totalItems = allMeals.Count;
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            totalPages = Math.Max(1, totalPages);
            page = Math.Clamp(page, 1, totalPages);

            var pagedMeals = allMeals
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Pobranie listy ID ulubionych przepisów dla zalogowanego u¿ytkownika
            List<string> userFavoriteIds = new List<string>();
            if (User.Identity?.IsAuthenticated == true)
            {
                var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userIdString) && int.TryParse(userIdString, out int userId))
                {
                    userFavoriteIds = await _context.Favorites
                        .Where(f => f.UserId == userId)
                        .Select(f => f.ExternalMealId)
                        .ToListAsync();
                }
            }

            // Pobranie i obliczenie œredniej ocen oraz liczby g³osów dla wyœwietlanych przepisów
            var mealIds = pagedMeals.Select(m => m.IdMeal).ToList();
            var ratingsData = await _context.Ratings
                .Where(r => mealIds.Contains(r.ExternalMealId))
                .GroupBy(r => r.ExternalMealId)
                .Select(g => new RecipeRatingDto
                {
                    ExternalMealId = g.Key,
                    AverageScore = Math.Round(g.Average(r => r.Score), 1),
                    VotesCount = g.Count()
                })
                .ToDictionaryAsync(r => r.ExternalMealId);

            ViewBag.SearchQuery = search;
            ViewBag.SelectedCategory = category;
            ViewBag.SelectedCuisine = cuisine;
            ViewBag.TotalItems = totalItems;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.UserFavoriteIds = userFavoriteIds;
            ViewBag.RecipeRatings = ratingsData;

            // Jeœli zapytanie przysz³o przez AJAX (np. przy paginacji czy filtrach), zwracamy sam czêœciowy widok
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_RecipesListPartial", pagedMeals);
            }

            return View(pagedMeals);
        }

        // Odpowiada za wyœwietlanie formularza kontaktowego
        public IActionResult Contact()
        {
            return View();
        }

        // Odpowiada za wysy³kê e-maili z formularza kontaktowego
        [HttpPost]
        public IActionResult SendContactEmail(string SenderName, string SenderEmail, string Message)
        {
            var lastSentString = HttpContext.Session.GetString("LastEmailSentDate");

            if (!string.IsNullOrEmpty(lastSentString))
            {
                if (DateTime.TryParse(lastSentString, out DateTime lastSentDate))
                {
                    if ((DateTime.Now - lastSentDate).TotalHours < 24)
                    {
                        TempData["ErrorMessage"] = "You can only send one message every 24 hours.";
                        return RedirectToAction("Contact");
                    }
                }
            }

            try
            {
                var smtpServer = _configuration["EmailSettings:SmtpServer"] ?? "smtp.gmail.com";
                var port = int.Parse(_configuration["EmailSettings:Port"] ?? "587");
                var senderEmail = _configuration["EmailSettings:SenderEmail"] ?? "smakoszsite@gmail.com";
                var appPassword = _configuration["EmailSettings:AppPassword"];

                var smtpClient = new SmtpClient(smtpServer)
                {
                    Port = port,
                    Credentials = new NetworkCredential(senderEmail, appPassword),
                    EnableSsl = true,
                };

                string htmlBody = $@"
        <!DOCTYPE html>
        <html>
        <head>
            <meta charset='utf-8'>
            <style>
                body {{
                    font-family: 'Montserrat', Arial, sans-serif;
                    background-color: #FAF6F0;
                    margin: 0;
                    padding: 30px;
                    color: #2D3142;
                }}
                .email-card {{
                    max-width: 600px;
                    margin: 0 auto;
                    background-color: #ffffff;
                    border: 2px solid #2D3142;
                    border-radius: 16px;
                    padding: 30px;
                    box-shadow: 0 4px 15px rgba(0,0,0,0.05);
                }}
                .email-header {{
                    text-align: center;
                    border-bottom: 2px solid #2D3142;
                    padding-bottom: 15px;
                    margin-bottom: 25px;
                }}
                .brand-title {{
                    font-size: 28px;
                    font-weight: bold;
                    letter-spacing: 1px;
                    color: #2D3142;
                    margin: 0;
                }}
                .info-row {{
                    margin-bottom: 12px;
                    font-size: 15px;
                }}
                .info-label {{
                    font-weight: bold;
                    color: #2D3142;
                }}
                .message-box {{
                    background-color: #FAF6F0;
                    border-left: 4px solid #2D3142;
                    padding: 15px 20px;
                    margin-top: 20px;
                    border-radius: 4px;
                    font-size: 15px;
                    line-height: 1.6;
                    white-space: pre-wrap;
                }}
                .email-footer {{
                    margin-top: 30px;
                    text-align: center;
                    font-size: 12px;
                    color: #777777;
                }}
            </style>
        </head>
        <body>
            <div class='email-card'>
                <div class='email-header'>
                    <h1 class='brand-title'>smakosz</h1>
                    <span style='font-size: 13px; text-transform: uppercase; letter-spacing: 2px;'>New Contact Form Message</span>
                </div>
                
                <div class='info-row'>
                    <span class='info-label'>From:</span> {SenderName}
                </div>
                <div class='info-row'>
                    <span class='info-label'>Email Address:</span> <a href='mailto:{SenderEmail}' style='color: #2D3142;'>{SenderEmail}</a>
                </div>
                
                <div class='message-box'>
                    {Message}
                </div>

                <div class='email-footer'>
                    This message was sent via the Smakosz contact form.
                </div>
            </div>
        </body>
        </html>";

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(senderEmail, "Smakosz Contact Form"),
                    Subject = $"[Smakosz] New Message from {SenderName}",
                    Body = htmlBody,
                    IsBodyHtml = true
                };

                mailMessage.ReplyToList.Add(new MailAddress(SenderEmail));
                mailMessage.To.Add(senderEmail);

                smtpClient.Send(mailMessage);

                HttpContext.Session.SetString("LastEmailSentDate", DateTime.Now.ToString("o"));

                TempData["SuccessMessage"] = "Your message has been sent successfully!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending contact form email");
                TempData["ErrorMessage"] = "There was a problem sending your message. Please try again later.";
            }

            return RedirectToAction("Contact");
        }
    }
}