using System.ComponentModel.DataAnnotations;

namespace TestGenerator.Models
{
    public class Category
    {
        public Category()
        {
            Questions = new List<Question>();
        }

        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string? Description { get; set; }

        public ICollection<Question>? Questions { get; set; }
    }
} 