using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TestGenerator.Data;
using TestGenerator.Models;
using TestGenerator.Services;
using TestGenerator.ViewModels;

namespace TestGenerator.Controllers
{
    [Authorize]
    public class TestController : BaseController
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly TestEvaluationService _evaluationService;

        public TestController(
            ApplicationDbContext context, 
            UserManager<ApplicationUser> userManager,
            TestEvaluationService evaluationService) 
            : base(context)
        {
            _userManager = userManager;
            _evaluationService = evaluationService;
        }

        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var tests = await _context.Tests
                .Include(t => t.TestQuestions)
                .Where(t => t.CreatorId == currentUser.Id)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            return View(tests);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var test = await _context.Tests
                .Include(t => t.TestQuestions)
                    .ThenInclude(tq => tq.Question)
                        .ThenInclude(q => q.Category)
                .Include(t => t.TestQuestions)
                    .ThenInclude(tq => tq.Question)
                        .ThenInclude(q => q.PossibleAnswers)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (test == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (test.CreatorId != currentUser.Id)
            {
                return Forbid();
            }

            return View(test);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = await _context.Categories
                .Select(c => new
                {
                    c.Id,
                    c.Name,
                    QuestionCount = c.Questions.Count
                })
                .ToListAsync();

            var model = new TestGenerationViewModel
            {
                TimeLimit = 60,
                DifficultyLevel = 3,
                NumberOfQuestions = 10
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TestGenerationViewModel model)
        {
            if (ModelState.IsValid)
            {
                var currentUser = await _userManager.GetUserAsync(User);
                var questions = await GetQuestionsForTest(model);

                if (!questions.Any())
                {
                    SetAlert("No questions found matching the specified criteria.", "warning");
                    ViewBag.Categories = await _context.Categories
                        .Select(c => new { c.Id, c.Name, QuestionCount = c.Questions.Count })
                        .ToListAsync();
                    return View(model);
                }

                var test = new Test
                {
                    Title = model.Title,
                    Description = model.Description,
                    CreatedAt = DateTime.Now,
                    TimeLimit = model.TimeLimit,
                    DifficultyLevel = model.DifficultyLevel,
                    CreatorId = currentUser.Id,
                    TotalPoints = questions.Sum(q => q.Points),
                    TestQuestions = questions.Select((q, index) => new TestQuestion
                    {
                        Question = q,
                        OrderNumber = index + 1
                    }).ToList()
                };

                _context.Add(test);
                await _context.SaveChangesAsync();

                SetAlert("Test created successfully", "success");
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Categories = await _context.Categories
                .Select(c => new { c.Id, c.Name, QuestionCount = c.Questions.Count })
                .ToListAsync();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var test = await _context.Tests
                .Include(t => t.TestQuestions)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (test == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (test.CreatorId != currentUser.Id)
            {
                return Forbid();
            }

            var testResults = await _context.TestResults
                .Include(tr => tr.AnswerResults)
                .Where(tr => tr.TestId == id)
                .ToListAsync();

            foreach (var result in testResults)
            {
                _context.TestAnswerResults.RemoveRange(result.AnswerResults);
                _context.TestResults.Remove(result);
            }

            _context.TestQuestions.RemoveRange(test.TestQuestions);
            _context.Tests.Remove(test);
            await _context.SaveChangesAsync();

            SetAlert("Test deleted successfully", "success");
            return RedirectToAction(nameof(Index));
        }

        private async Task<List<Question>> GetQuestionsForTest(TestGenerationViewModel model)
        {
            var query = _context.Questions
                .Include(q => q.Category)
                .Include(q => q.PossibleAnswers)
                .AsQueryable();

            if (model.CategoryIds != null && model.CategoryIds.Any())
            {
                query = query.Where(q => model.CategoryIds.Contains(q.CategoryId));
            }

            if (model.DifficultyLevel > 0)
            {
                query = query.Where(q => q.DifficultyLevel == model.DifficultyLevel);
            }

            var availableQuestions = await query.ToListAsync();
            return availableQuestions
                .OrderBy(x => Guid.NewGuid())
                .Take(model.NumberOfQuestions)
                .ToList();
        }

        public async Task<IActionResult> Take(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var test = await _context.Tests
                .Include(t => t.TestQuestions)
                    .ThenInclude(tq => tq.Question)
                        .ThenInclude(q => q.PossibleAnswers)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (test == null)
            {
                return NotFound();
            }

            var viewModel = new TestSubmissionViewModel
            {
                TestId = test.Id,
                Title = test.Title,
                TimeLimit = test.TimeLimit,
                StartTime = DateTime.Now,
                Questions = test.TestQuestions
                    .OrderBy(tq => tq.OrderNumber)
                    .Select(tq => new QuestionSubmissionViewModel
                    {
                        QuestionId = tq.Question.Id,
                        Content = tq.Question.Content,
                        Type = tq.Question.Type.ToString(),
                        ImagePath = tq.Question.ImagePath,
                        PossibleAnswers = tq.Question.Type == QuestionType.MultipleChoice
                            ? tq.Question.PossibleAnswers.Select(a => new AnswerSubmissionViewModel
                            {
                                Id = a.Id,
                                Content = a.Content
                            }).ToList()
                            : null
                    }).ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(TestSubmissionViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    // Reload test data for the view
                    var testData = await _context.Tests
                        .Include(t => t.TestQuestions)
                            .ThenInclude(tq => tq.Question)
                                .ThenInclude(q => q.PossibleAnswers)
                        .FirstOrDefaultAsync(t => t.Id == model.TestId);

                    if (testData != null)
                    {
                        // Update the model with the reloaded data while preserving submitted answers
                        foreach (var question in model.Questions)
                        {
                            var testQuestion = testData.TestQuestions
                                .FirstOrDefault(tq => tq.Question.Id == question.QuestionId);
                            if (testQuestion != null)
                            {
                                question.Content = testQuestion.Question.Content;
                                question.Type = testQuestion.Question.Type.ToString();
                                question.ImagePath = testQuestion.Question.ImagePath;
                                question.PossibleAnswers = testQuestion.Question.Type == QuestionType.MultipleChoice
                                    ? testQuestion.Question.PossibleAnswers.Select(a => new AnswerSubmissionViewModel
                                    {
                                        Id = a.Id,
                                        Content = a.Content
                                    }).ToList()
                                    : null;
                            }
                        }
                    }

                    SetAlert("Моля, попълнете всички задължителни полета.", "danger");
                    return View("Take", model);
                }

                var test = await _context.Tests
                    .Include(t => t.TestQuestions)
                        .ThenInclude(tq => tq.Question)
                            .ThenInclude(q => q.PossibleAnswers)
                    .Include(t => t.TestQuestions)
                        .ThenInclude(tq => tq.Question)
                            .ThenInclude(q => q.Category)
                    .FirstOrDefaultAsync(t => t.Id == model.TestId);

                if (test == null)
                {
                    SetAlert("Тестът не беше намерен.", "danger");
                    return RedirectToAction(nameof(Index));
                }

                var currentUser = await _userManager.GetUserAsync(User);
                var submittedAnswers = model.Questions
                    .Select(q => (q.QuestionId, q.SubmittedAnswer))
                    .ToList();

                var testResult = _evaluationService.EvaluateTest(test, submittedAnswers, model.StartTime);
                testResult.UserId = currentUser.Id;
                testResult.EndTime = DateTime.Now;

                _context.TestResults.Add(testResult);
                await _context.SaveChangesAsync();

                SetAlert("Тестът беше успешно предаден.", "success");
                return RedirectToAction(nameof(Result), new { id = testResult.Id });
            }
            catch (Exception ex)
            {
                SetAlert($"Възникна грешка при предаването на теста: {ex.Message}", "danger");
                return View("Take", model);
            }
        }

        public async Task<IActionResult> Result(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var testResult = await _context.TestResults
                .Include(tr => tr.Test)
                .Include(tr => tr.AnswerResults)
                    .ThenInclude(ar => ar.Question)
                        .ThenInclude(q => q.Category)
                .Include(tr => tr.AnswerResults)
                    .ThenInclude(ar => ar.Question)
                        .ThenInclude(q => q.PossibleAnswers)
                .FirstOrDefaultAsync(tr => tr.Id == id);

            if (testResult == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (testResult.UserId != currentUser.Id)
            {
                return Forbid();
            }

            return View(testResult);
        }
    }
} 