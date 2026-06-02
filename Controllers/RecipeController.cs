using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmakoszApp.Data;
using SmakoszApp.Models;
using System.Security.Claims;

namespace SmakoszApp.Controllers
{
    public class RecipeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RecipeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Wyświetlanie formularza dodawania przepisu
        [HttpGet]
        public IActionResult Add()
        {
            return View();
        }

        // Zapisywanie nowego przepisu i zdjęcia w bazie
        [HttpPost]
        public async Task<IActionResult> Add(Recipe model, List<string> IngredientsList, IFormFile ImageFile)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdString))
            {
                return RedirectToAction("Index", "Account");
            }

            model.UserId = int.Parse(userIdString);

            if (ImageFile != null && ImageFile.Length > 0)
            {
                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/recipes");

                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
                var filePath = Path.Combine(folderPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(stream);
                }

                model.ImagePath = "/images/recipes/" + fileName;
            }

            foreach (var item in IngredientsList)
            {
                if (!string.IsNullOrWhiteSpace(item))
                {
                    model.Ingredients.Add(new Ingredient { Name = item });
                }
            }

            _context.Recipes.Add(model);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Przepis został dodany pomyślnie!";
            return RedirectToAction("Index", "Home");
        }

        // Wyświetlanie listy przepisów przypisanych do konta
        [HttpGet]
        public async Task<IActionResult> MyRecipes()
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdString))
            {
                return RedirectToAction("Index", "Account");
            }

            int userId = int.Parse(userIdString);

            var recipes = await _context.Recipes
                .Where(r => r.UserId == userId)
                .ToListAsync();

            return View(recipes);
        }

        // Usuwanie przepisu oraz przypisanego zdjęcia z serwera
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString)) return RedirectToAction("Index", "Account");

            int userId = int.Parse(userIdString);

            var recipe = await _context.Recipes.FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

            if (recipe == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(recipe.ImagePath))
            {
                var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", recipe.ImagePath.TrimStart('/'));
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }

            _context.Recipes.Remove(recipe);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Przepis został usunięty.";
            return RedirectToAction("MyRecipes");
        }

        // Pobieranie danych przepisu do formularza edycji
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString)) return RedirectToAction("Index", "Account");

            int userId = int.Parse(userIdString);

            var recipe = await _context.Recipes
                .Include(r => r.Ingredients)
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

            if (recipe == null) return NotFound();

            return View(recipe);
        }

        // Zapisywanie wprowadzonych zmian i ewentualna podmiana zdjęcia
        [HttpPost]
        public async Task<IActionResult> Edit(int id, Recipe updatedRecipe, List<string> IngredientsList, IFormFile? ImageFile)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString)) return RedirectToAction("Index", "Account");

            int userId = int.Parse(userIdString);

            var recipe = await _context.Recipes
                .Include(r => r.Ingredients)
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

            if (recipe == null) return NotFound();

            recipe.Title = updatedRecipe.Title;
            recipe.Instructions = updatedRecipe.Instructions;
            recipe.PrepTimeMinutes = updatedRecipe.PrepTimeMinutes;
            recipe.Calories = updatedRecipe.Calories;
            recipe.MainCategory = updatedRecipe.MainCategory;
            recipe.SubCategory = updatedRecipe.SubCategory;
            recipe.DietType = updatedRecipe.DietType;
            recipe.CuisineType = updatedRecipe.CuisineType;

            if (ImageFile != null && ImageFile.Length > 0)
            {
                if (!string.IsNullOrEmpty(recipe.ImagePath))
                {
                    var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", recipe.ImagePath.TrimStart('/'));
                    if (System.IO.File.Exists(oldImagePath)) System.IO.File.Delete(oldImagePath);
                }

                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/recipes");
                if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
                var filePath = Path.Combine(folderPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(stream);
                }

                recipe.ImagePath = "/images/recipes/" + fileName;
            }

            _context.Ingredients.RemoveRange(recipe.Ingredients);
            foreach (var item in IngredientsList)
            {
                if (!string.IsNullOrWhiteSpace(item))
                {
                    recipe.Ingredients.Add(new Ingredient { Name = item });
                }
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Przepis zaktualizowany pomyślnie.";
            return RedirectToAction("Index", "Home");
        }

        // Dodawanie i usuwanie przepisu z listy ulubionych
        [HttpPost]
        public async Task<IActionResult> ToggleFavorite(int recipeId)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString)) return Unauthorized();

            int userId = int.Parse(userIdString);

            var favorite = await _context.Favorites
                .FirstOrDefaultAsync(f => f.UserId == userId && f.RecipeId == recipeId);

            bool isFavorite;
            if (favorite != null)
            {
                _context.Favorites.Remove(favorite);
                isFavorite = false;
            }
            else
            {
                _context.Favorites.Add(new Favorite { UserId = userId, RecipeId = recipeId });
                isFavorite = true;
            }

            await _context.SaveChangesAsync();
            return Json(new { isFavorite = isFavorite });
        }

        // Wyświetlanie listy ulubionych potraw
        [HttpGet]
        public async Task<IActionResult> Favorites()
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString)) return RedirectToAction("Index", "Account");

            int userId = int.Parse(userIdString);

            var favoriteRecipes = await _context.Favorites
                .Where(f => f.UserId == userId)
                .Select(f => f.Recipe)
                .ToListAsync();

            return View(favoriteRecipes);
        }

        // Wyświetlanie szczegółów wybranej potrawy
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var recipe = await _context.Recipes
                .Include(r => r.Ingredients)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (recipe == null)
            {
                return NotFound();
            }

            return View(recipe);
        }
    }
}