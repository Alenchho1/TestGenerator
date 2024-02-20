using System.ComponentModel.DataAnnotations;

namespace TestGenerator.ViewModels
{
    public class TestSubmissionViewModel
    {
        [Required]
        public int TestId { get; set; }
        
        public string Title { get; set; }
        
        public List<QuestionSubmissionViewModel> Questions { get; set; } = new List<QuestionSubmissionViewModel>();
        
        public DateTime StartTime { get; set; }
        
        public int TimeLimit { get; set; }
    }

    public class QuestionSubmissionViewModel
    {
        [Required]
        public int QuestionId { get; set; }
        
        public string Content { get; set; }
        
        [Required]
        public string Type { get; set; }
        
        public string? ImagePath { get; set; }
        
        public List<AnswerSubmissionViewModel>? PossibleAnswers { get; set; }
        
        [Required(ErrorMessage = "Отговорът е задължителен")]
        public string SubmittedAnswer { get; set; }
    }

    public class AnswerSubmissionViewModel
    {
        public int Id { get; set; }
        public string Content { get; set; }
    }
} 