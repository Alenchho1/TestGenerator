using System.ComponentModel.DataAnnotations;

namespace TestGenerator.Models
{
    public class Question
    {
        public int Id { get; set; }

        [Required]
        public string Content { get; set; }

        [Required]
        public QuestionType Type { get; set; }

        public int DifficultyLevel { get; set; }

        public string? CorrectAnswer { get; set; }

        public List<Answer>? PossibleAnswers { get; set; }

        public string? Keywords { get; set; }

        public string? ImagePath { get; set; }

        public int Points { get; set; }

        public int CategoryId { get; set; }
        public Category Category { get; set; }
    }

    public enum QuestionType
    {
        MultipleChoice,
        OpenEnded
    }
} 