using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TestGenerator.Data;
using TestGenerator.Models;
using Microsoft.Extensions.Logging;

namespace TestGenerator.Controllers
{
    [Authorize]
    public class CategoryController : BaseController
    {
        private readonly ILogger<CategoryController> _logger;

        public CategoryController(ApplicationDbContext context, ILogger<CategoryController> logger) 
            : base(context)
        {
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _context.Categories
                .Include(c => c.Questions)
                .ToListAsync();
            return View(categories);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description")] Category category)
        {
            _logger.LogInformation("Create action started");
            try
            {
                _logger.LogInformation("Received category data - Name: {Name}, Description: {Description}", 
                    category.Name, category.Description);

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state when creating category");
                    foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                    {
                        _logger.LogWarning("Validation error: {Error}", error.ErrorMessage);
                    }
                    return View(category);
                }

                _logger.LogInformation("Model state is valid, adding category to database");
                _context.Add(category);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Category created successfully with ID: {Id}", category.Id);
                SetAlert("Category created successfully", "success");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating category");
                SetAlert("Error creating category. Please try again.", "danger");
                return View(category);
            }
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description")] Category category)
        {
            if (id != category.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                    SetAlert("Category updated successfully", "success");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAjax([FromBody] Category category)
        {
            if (ModelState.IsValid)
            {
                _context.Add(category);
                await _context.SaveChangesAsync();
                return Json(new { id = category.Id, name = category.Name });
            }
            return BadRequest(ModelState);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _context.Categories
                .Include(c => c.Questions)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
            {
                return NotFound();
            }

            try
            {
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
                SetAlert("Категорията беше изтрита успешно", "success");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting category");
                SetAlert("Грешка при изтриване на категорията. Уверете се, че няма тестове, използващи тази категория.", "danger");
            }

            return RedirectToAction(nameof(Index));
        }

        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.Id == id);
        }
    }
} 