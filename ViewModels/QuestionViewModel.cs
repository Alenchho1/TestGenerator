using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using TestGenerator.Models;

namespace TestGenerator.ViewModels
{
    public class QuestionViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Question content is required")]
        public string Content { get; set; }

        [Required(ErrorMessage = "Question type is required")]
        public QuestionType Type { get; set; }

        [Range(1, 5, ErrorMessage = "Difficulty level must be between 1 and 5")]
        public int DifficultyLevel { get; set; }

        [Required(ErrorMessage = "Category is required")]
        public int CategoryId { get; set; }

        [Range(1, 100, ErrorMessage = "Points must be between 1 and 100")]
        public int Points { get; set; }

        public string? Keywords { get; set; }

        public string? CorrectAnswer { get; set; }

        public IFormFile? Image { get; set; }

        public string? ExistingImagePath { get; set; }

        public List<AnswerViewModel> Answers { get; set; } = new List<AnswerViewModel>();
    }

    public class AnswerViewModel
    {
        public string Content { get; set; }
        public bool IsCorrect { get; set; }
    }
} 