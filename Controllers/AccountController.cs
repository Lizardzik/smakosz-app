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

        [HttpGet]
        public IActionResult Index()
        {
            return View(new AccountViewModel());
        }

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
                Email = model.Email
            };

            user.Password = _passwordHasher.HashPassword(user, model.Password);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Konto zostało pomyślnie utworzone. Możesz się teraz zalogować.";

            return RedirectToAction("Index");
        }

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

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }
    }
}