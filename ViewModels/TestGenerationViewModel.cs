using System.ComponentModel.DataAnnotations;

namespace TestGenerator.ViewModels
{
    public class TestGenerationViewModel
    {
        [Required(ErrorMessage = "Title is required")]
        public string Title { get; set; }

        public string? Description { get; set; }

        [Required(ErrorMessage = "Number of questions is required")]
        [Range(1, 50, ErrorMessage = "Number of questions must be between 1 and 50")]
        public int NumberOfQuestions { get; set; }

        [Range(1, 5, ErrorMessage = "Difficulty level must be between 1 and 5")]
        public int DifficultyLevel { get; set; }

        [Range(5, 120, ErrorMessage = "Time limit must be between 5 and 120 minutes")]
        public int TimeLimit { get; set; }

        public List<int>? CategoryIds { get; set; }
    }
} 