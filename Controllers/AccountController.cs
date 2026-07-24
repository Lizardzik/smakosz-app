using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SmakoszApp.Data;
using SmakoszApp.Models;
using SmakoszApp.Services;
using System.Security.Claims;

namespace SmakoszApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly PasswordHasher<User> _passwordHasher;
        private readonly IContentModerationService _moderationService;

        public AccountController(ApplicationDbContext context, IContentModerationService moderationService)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<User>();
            _moderationService = moderationService;
        }

        // Display login & registration view
        [HttpGet]
        public IActionResult Index()
        {
            return View(new AccountViewModel());
        }

        // Register new user & hash password
        [HttpPost]
        public async Task<IActionResult> Register(AccountViewModel model)
        {
            // AI MODERATION VALIDATION (Login)
            var (isAllowed, reason) = await _moderationService.ValidateTextAsync(model.Login);
            if (!isAllowed)
            {
                ModelState.AddModelError("", "The provided username contains inappropriate or prohibited words.");
                return View("Index", model);
            }

            if (_context.Users.Any(u => u.Login == model.Login || u.Email == model.Email))
            {
                ModelState.AddModelError("", "A user with this username or email already exists.");
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

            TempData["SuccessMessage"] = "Account created successfully. You can now log in.";

            return RedirectToAction("Index");
        }

        // Verify credentials & log user into cookie session
        [HttpPost]
        public async Task<IActionResult> Login(AccountViewModel model)
        {
            var user = _context.Users.FirstOrDefault(u => u.Login == model.Login);

            if (user == null || _passwordHasher.VerifyHashedPassword(user, user.Password, model.Password) == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError("", "Invalid username or password.");
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

        // Log out user and redirect to login screen
        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Index", "Account");
        }

        // Sign out from all schemes and return to home page as guest
        public async Task<IActionResult> Guest()
        {
            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);

            return RedirectToAction("Index", "Home");
        }

        // GET: Account/Settings
        [HttpGet]
        public async Task<IActionResult> Settings()
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString)) return RedirectToAction("Index", "Account");

            int userId = int.Parse(userIdString);
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            var model = new UserSettingsViewModel
            {
                Login = user.Login,
                Email = user.Email,
                CurrentProfilePicture = user.ProfilePicturePath,
                ThemePreference = user.ThemePreference ?? "light"
            };

            return View(model);
        }

        // POST: Account/Settings
        [HttpPost]
        public async Task<IActionResult> Settings(UserSettingsViewModel model, IFormFile? AvatarFile)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString)) return RedirectToAction("Index", "Account");

            int userId = int.Parse(userIdString);
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            // AI MODERATION VALIDATION (New Login)
            var (isAllowed, reason) = await _moderationService.ValidateTextAsync(model.Login);
            if (!isAllowed)
            {
                ModelState.AddModelError("", "The new username contains inappropriate or prohibited words.");
                model.CurrentProfilePicture = user.ProfilePicturePath;
                return View(model);
            }

            // Check if username or email is already taken by someone else
            if (_context.Users.Any(u => u.Id != userId && (u.Login == model.Login || u.Email == model.Email)))
            {
                ModelState.AddModelError("", "The provided username or email is already taken by another user.");
                model.CurrentProfilePicture = user.ProfilePicturePath;
                return View(model);
            }

            // 1. Update basic information
            user.Login = model.Login;
            user.Email = model.Email;
            user.ThemePreference = model.ThemePreference;

            // 2. Handle profile picture upload
            if (AvatarFile != null && AvatarFile.Length > 0)
            {
                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/avatars");
                if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                // Remove old avatar if it exists
                if (!string.IsNullOrEmpty(user.ProfilePicturePath))
                {
                    var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.ProfilePicturePath.TrimStart('/'));
                    if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                }

                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(AvatarFile.FileName)}";
                var filePath = Path.Combine(folderPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await AvatarFile.CopyToAsync(stream);
                }

                user.ProfilePicturePath = "/uploads/avatars/" + fileName;
            }

            // 3. Handle password change
            if (!string.IsNullOrEmpty(model.NewPassword))
            {
                if (string.IsNullOrEmpty(model.CurrentPassword))
                {
                    ModelState.AddModelError("", "You must provide your current password to set a new one.");
                    model.CurrentProfilePicture = user.ProfilePicturePath;
                    return View(model);
                }

                var verifyResult = _passwordHasher.VerifyHashedPassword(user, user.Password, model.CurrentPassword);
                if (verifyResult == PasswordVerificationResult.Failed)
                {
                    ModelState.AddModelError("", "The current password is incorrect.");
                    model.CurrentProfilePicture = user.ProfilePicturePath;
                    return View(model);
                }

                user.Password = _passwordHasher.HashPassword(user, model.NewPassword);
            }

            await _context.SaveChangesAsync();

            // Refresh identity claims in cookie (in case username changed)
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Login)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

            TempData["SuccessMessage"] = "Settings updated successfully!";
            return RedirectToAction("Settings");
        }
    }
}