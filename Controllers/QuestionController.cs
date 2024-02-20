using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TestGenerator.Data;
using TestGenerator.Models;
using TestGenerator.ViewModels;

namespace TestGenerator.Controllers
{
    [Authorize]
    public class QuestionController : BaseController
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public QuestionController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment) : base(context)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index(int? categoryId)
        {
            var query = _context.Questions
                .Include(q => q.Category)
                .Include(q => q.PossibleAnswers)
                .AsQueryable();

            if (categoryId.HasValue)
            {
                query = query.Where(q => q.CategoryId == categoryId);
            }

            var questions = await query.ToListAsync();
            ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name", categoryId);
            ViewBag.CurrentCategoryId = categoryId;

            return View(questions);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name");
            return View(new QuestionViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(QuestionViewModel model)
        {
            if (ModelState.IsValid)
            {
                var question = new Question
                {
                    Content = model.Content,
                    Type = model.Type,
                    DifficultyLevel = model.DifficultyLevel,
                    CategoryId = model.CategoryId,
                    Points = model.Points,
                    Keywords = model.Keywords
                };

                // Handle image upload
                if (model.Image != null)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "question-images");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + model.Image.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.Image.CopyToAsync(fileStream);
                    }

                    question.ImagePath = "~/question-images/" + uniqueFileName;
                }

                if (model.Type == QuestionType.MultipleChoice)
                {
                    question.PossibleAnswers = model.Answers
                        .Where(a => !string.IsNullOrWhiteSpace(a.Content))
                        .Select(a => new Answer
                        {
                            Content = a.Content,
                            IsCorrect = a.IsCorrect
                        }).ToList();
                }
                else
                {
                    question.CorrectAnswer = model.CorrectAnswer;
                }

                _context.Add(question);
                await _context.SaveChangesAsync();
                SetAlert("Въпросът беше създаден успешно", "success");
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name", model.CategoryId);
            return View(model);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var question = await _context.Questions
                .Include(q => q.PossibleAnswers)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (question == null)
            {
                return NotFound();
            }

            var viewModel = new QuestionViewModel
            {
                Id = question.Id,
                Content = question.Content,
                Type = question.Type,
                DifficultyLevel = question.DifficultyLevel,
                CategoryId = question.CategoryId,
                Points = question.Points,
                Keywords = question.Keywords,
                CorrectAnswer = question.CorrectAnswer,
                ExistingImagePath = question.ImagePath,
                Answers = question.PossibleAnswers?.Select(a => new AnswerViewModel
                {
                    Content = a.Content,
                    IsCorrect = a.IsCorrect
                }).ToList() ?? new List<AnswerViewModel>()
            };

            ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name", question.CategoryId);
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, QuestionViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var question = await _context.Questions
                        .Include(q => q.PossibleAnswers)
                        .FirstOrDefaultAsync(q => q.Id == id);

                    if (question == null)
                    {
                        return NotFound();
                    }

                    question.Content = model.Content;
                    question.Type = model.Type;
                    question.DifficultyLevel = model.DifficultyLevel;
                    question.CategoryId = model.CategoryId;
                    question.Points = model.Points;
                    question.Keywords = model.Keywords;

                    // Handle image upload
                    if (model.Image != null)
                    {
                        string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "question-images");
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        // Delete old image if exists
                        if (!string.IsNullOrEmpty(question.ImagePath))
                        {
                            string oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, question.ImagePath.TrimStart('/'));
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }

                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + model.Image.FileName;
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await model.Image.CopyToAsync(fileStream);
                        }

                        question.ImagePath = "~/question-images/" + uniqueFileName;
                    }

                    if (model.Type == QuestionType.MultipleChoice)
                    {
                        // Remove existing answers
                        _context.RemoveRange(question.PossibleAnswers);

                        // Add new answers
                        question.PossibleAnswers = model.Answers
                            .Where(a => !string.IsNullOrWhiteSpace(a.Content))
                            .Select(a => new Answer
                            {
                                Content = a.Content,
                                IsCorrect = a.IsCorrect,
                                QuestionId = question.Id
                            }).ToList();
                        
                        question.CorrectAnswer = null;
                    }
                    else
                    {
                        question.CorrectAnswer = model.CorrectAnswer;
                        question.PossibleAnswers?.Clear();
                    }

                    await _context.SaveChangesAsync();
                    SetAlert("Question updated successfully", "success");
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!QuestionExists(model.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name", model.CategoryId);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var question = await _context.Questions
                .Include(q => q.PossibleAnswers)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (question == null)
            {
                return NotFound();
            }

            _context.Questions.Remove(question);
            await _context.SaveChangesAsync();
            SetAlert("Question deleted successfully", "success");

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> CreateAjax([FromBody] QuestionViewModel model)
        {
            if (ModelState.IsValid)
            {
                var question = new Question
                {
                    Content = model.Content,
                    Type = model.Type,
                    DifficultyLevel = model.DifficultyLevel,
                    CategoryId = model.CategoryId,
                    Points = model.Points,
                    Keywords = model.Keywords,
                    CorrectAnswer = model.CorrectAnswer
                };

                if (model.Type == QuestionType.MultipleChoice && model.Answers != null)
                {
                    question.PossibleAnswers = model.Answers
                        .Where(a => !string.IsNullOrWhiteSpace(a.Content))
                        .Select(a => new Answer
                        {
                            Content = a.Content,
                            IsCorrect = a.IsCorrect
                        }).ToList();
                }

                _context.Add(question);
                await _context.SaveChangesAsync();

                return Json(new
                {
                    id = question.Id,
                    content = question.Content,
                    type = question.Type.ToString(),
                    points = question.Points
                });
            }

            return BadRequest(ModelState);
        }

        private bool QuestionExists(int id)
        {
            return _context.Questions.Any(e => e.Id == id);
        }
    }
} 