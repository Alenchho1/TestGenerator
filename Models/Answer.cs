using System.ComponentModel.DataAnnotations;

namespace TestGenerator.Models
{
    public class Answer
    {
        public int Id { get; set; }

        [Required]
        public string Content { get; set; }

        public bool IsCorrect { get; set; }

        public int QuestionId { get; set; }
        public Question Question { get; set; }
    }
} 