using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TestGenerator.Models
{
    public class TestResult
    {
        public int Id { get; set; }

        public int TestId { get; set; }
        public Test Test { get; set; }

        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public int Score { get; set; }
        public int MaxScore { get; set; }

        [Range(0, 100)]
        public decimal PercentageScore { get; set; }

        public bool IsCompleted { get; set; }

        public int CorrectAnswersCount { get; set; }
        public int TotalQuestionsCount { get; set; }
        public decimal Grade { get; set; }
        public string Feedback { get; set; }

        public virtual ICollection<TestAnswerResult> AnswerResults { get; set; } = new List<TestAnswerResult>();
    }

    public class TestAnswerResult
    {
        public int Id { get; set; }

        public int TestResultId { get; set; }
        public TestResult TestResult { get; set; }

        public int QuestionId { get; set; }
        public Question Question { get; set; }

        public string SubmittedAnswer { get; set; }
        public int? SelectedAnswerId { get; set; }

        public bool IsCorrect { get; set; }
        public int Points { get; set; }

        public decimal KeywordMatchPercentage { get; set; }
        public string FeedbackNotes { get; set; }
    }
} 