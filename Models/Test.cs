using System.ComponentModel.DataAnnotations;

namespace TestGenerator.Models
{
    public class Test
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        public string? Description { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        public int TimeLimit { get; set; }

        public int DifficultyLevel { get; set; }

        public int TotalPoints { get; set; }

        public string CreatorId { get; set; }
        public ApplicationUser Creator { get; set; }

        public ICollection<TestQuestion> TestQuestions { get; set; }
    }

    public class TestQuestion
    {
        public int TestId { get; set; }
        public Test Test { get; set; }

        public int QuestionId { get; set; }
        public Question Question { get; set; }

        public int OrderNumber { get; set; }
    }
} 