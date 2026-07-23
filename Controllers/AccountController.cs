using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SmakoszApp.Data;
using SmakoszApp.Models;
using System.Security.Claims;

namespace SmakoszApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly PasswordHasher<User> _passwordHasher;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<User>();
        }

        // Wyświetlanie widoku logowania i rejestracji
        [HttpGet]
        public IActionResult Index()
        {
            return View(new AccountViewModel());
        }

        // Obsługa rejestracji nowego użytkownika i haszowanie hasła
        [HttpPost]
        public async Task<IActionResult> Register(AccountViewModel model)
        {
            if (_context.Users.Any(u => u.Login == model.Login || u.Email == model.Email))
            {
                ModelState.AddModelError("", "Użytkownik o takim loginie lub e-mailu już istnieje.");
                return View("Index", model);
            }

            var user = new User
            {
                Login = model.Login,
                Email = model.Email,
                ReputationPoints = 0
            };

            user.Password = _passwordHasher.HashPassword(user, model.Password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Konto zostało pomyślnie utworzone. Możesz się teraz zalogować.";

            return RedirectToAction("Index");
        }

        // Obsługa weryfikacji danych i logowania użytkownika do sesji ciasteczek
        [HttpPost]
        public async Task<IActionResult> Login(AccountViewModel model)
        {
            var user = _context.Users.FirstOrDefault(u => u.Login == model.Login);

            if (user == null || _passwordHasher.VerifyHashedPassword(user, user.Password, model.Password) == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError("", "Nieprawidłowy login lub hasło.");
                return View("Index", model);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Login)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            return RedirectToAction("Index", "Home");
        }

        // Wylogowanie zalogowanego użytkownika i powrót do ekranu logowania
        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Index", "Account");
        }

        // Wylogowanie ze wszystkich schematów, wyczyszczenie sesji i przekierowanie na stronę główną jako gość
        public async Task<IActionResult> Guest()
        {
            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);

            return RedirectToAction("Index", "Home");
        }
    }
}