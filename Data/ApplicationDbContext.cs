using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TestGenerator.Models;

namespace TestGenerator.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Answer> Answers { get; set; }
        public DbSet<Test> Tests { get; set; }
        public DbSet<TestQuestion> TestQuestions { get; set; }
        public DbSet<TestResult> TestResults { get; set; }
        public DbSet<TestAnswerResult> TestAnswerResults { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<TestQuestion>()
                .HasKey(tq => new { tq.TestId, tq.QuestionId });

            builder.Entity<TestQuestion>()
                .HasOne(tq => tq.Test)
                .WithMany(t => t.TestQuestions)
                .HasForeignKey(tq => tq.TestId);

            builder.Entity<TestQuestion>()
                .HasOne(tq => tq.Question)
                .WithMany()
                .HasForeignKey(tq => tq.QuestionId);

            builder.Entity<TestResult>()
                .HasOne(tr => tr.Test)
                .WithMany()
                .HasForeignKey(tr => tr.TestId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<TestResult>()
                .HasOne(tr => tr.User)
                .WithMany()
                .HasForeignKey(tr => tr.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<TestAnswerResult>()
                .HasOne(tar => tar.TestResult)
                .WithMany(tr => tr.AnswerResults)
                .HasForeignKey(tar => tar.TestResultId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<TestAnswerResult>()
                .HasOne(tar => tar.Question)
                .WithMany()
                .HasForeignKey(tar => tar.QuestionId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure decimal precision for PercentageScore and Grade
            builder.Entity<TestResult>()
                .Property(tr => tr.PercentageScore)
                .HasPrecision(5, 2);

            builder.Entity<TestResult>()
                .Property(tr => tr.Grade)
                .HasPrecision(3, 2);

            builder.Entity<TestAnswerResult>()
                .Property(tar => tar.KeywordMatchPercentage)
                .HasPrecision(5, 2);

            // Seed initial data
            SeedInitialData(builder);
        }

        private void SeedInitialData(ModelBuilder builder)
        {
            // Seed Categories
            builder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "C# Basics", Description = "Basic C# programming concepts" },
                new Category { Id = 2, Name = "ASP.NET Core", Description = "Web development with ASP.NET Core" },
                new Category { Id = 3, Name = "Entity Framework", Description = "Database operations with Entity Framework Core" },
                new Category { Id = 4, Name = "JavaScript", Description = "JavaScript programming fundamentals" }
            );

            // Seed Questions
            builder.Entity<Question>().HasData(
                new Question
                {
                    Id = 1,
                    Content = "What is a namespace in C#?",
                    Type = QuestionType.MultipleChoice,
                    DifficultyLevel = 1,
                    Points = 10,
                    CategoryId = 1,
                    Keywords = "namespace,organization,scope"
                },
                new Question
                {
                    Id = 2,
                    Content = "Explain what dependency injection is in ASP.NET Core.",
                    Type = QuestionType.OpenEnded,
                    DifficultyLevel = 3,
                    Points = 20,
                    CategoryId = 2,
                    Keywords = "DI,IoC,services",
                    CorrectAnswer = "Dependency Injection is a design pattern and a mechanism in ASP.NET Core that enables loose coupling between components by injecting dependencies through constructors or properties."
                },
                new Question
                {
                    Id = 3,
                    Content = "What is the difference between First() and FirstOrDefault() in LINQ?",
                    Type = QuestionType.MultipleChoice,
                    DifficultyLevel = 2,
                    Points = 15,
                    CategoryId = 3,
                    Keywords = "LINQ,collections,queries"
                }
            );

            // Seed Answers for multiple choice questions
            builder.Entity<Answer>().HasData(
                // Answers for Question 1
                new Answer { Id = 1, QuestionId = 1, Content = "A way to organize code into logical groups", IsCorrect = true },
                new Answer { Id = 2, QuestionId = 1, Content = "A type of variable", IsCorrect = false },
                new Answer { Id = 3, QuestionId = 1, Content = "A method declaration", IsCorrect = false },
                new Answer { Id = 4, QuestionId = 1, Content = "A class constructor", IsCorrect = false },

                // Answers for Question 3
                new Answer { Id = 5, QuestionId = 3, Content = "First() throws an exception if no elements found, FirstOrDefault() returns default value", IsCorrect = true },
                new Answer { Id = 6, QuestionId = 3, Content = "They are exactly the same", IsCorrect = false },
                new Answer { Id = 7, QuestionId = 3, Content = "FirstOrDefault() is faster than First()", IsCorrect = false },
                new Answer { Id = 8, QuestionId = 3, Content = "First() returns null, FirstOrDefault() throws an exception", IsCorrect = false }
            );
        }
    }
}
