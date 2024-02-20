using Microsoft.AspNetCore.Identity;

namespace TestGenerator.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public ICollection<Test> CreatedTests { get; set; }
    }
} 