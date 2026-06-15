using System.Diagnostics;
using System.Net;
using System.Net.Mail;
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

        // Wysy³anie Emaili z blokad¹ raz na dobê w sesji
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
                        TempData["ErrorMessage"] = "Mo¿esz wys³aæ tylko jedn¹ wiadomoœæ na 24 godziny z tego komputera.";
                        return RedirectToAction("Contact");
                    }
                }
            }
            try
            {
                var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential("przepisy.smakosz@gmail.com", "XXXXXXXXXXXXXXXXXXXXXXX"),
                    EnableSsl = true,
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress("przepisy.smakosz@gmail.com"),
                    Subject = "Wiadomoœæ z formularza od " + SenderName,
                    Body = "Wiadomoœæ od: " + SenderName + " (" + SenderEmail + ")\n\nTreœæ:\n" + Message,
                    IsBodyHtml = false,
                };

                mailMessage.ReplyToList.Add(new MailAddress(SenderEmail));
                mailMessage.To.Add("przepisy.smakosz@gmail.com");

                smtpClient.Send(mailMessage);

                HttpContext.Session.SetString("LastEmailSentDate", DateTime.Now.ToString("o"));

                TempData["SuccessMessage"] = "Wiadomoœæ zosta³a wys³ana pomyœlnie.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "B³¹d wysy³ania wiadomoœci z formularza");
                TempData["ErrorMessage"] = "Wyst¹pi³ problem podczas wysy³ania wiadomoœci.";
            }

            return RedirectToAction("Contact");
        }

        // Obs³uga b³êdów aplikacji
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}