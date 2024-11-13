using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VstepPractice.API.Common.Enums;
using VstepPractice.API.Data.Seeding.Abstracts;
using VstepPractice.API.Models.Entities;

namespace VstepPractice.API.Data.Seeding.Seeders;

public class ExamSeeder : BaseSeeder
{
    private readonly ApplicationDbContext context;
    private readonly UserManager<User> _userManager;

    public ExamSeeder(
        ApplicationDbContext context,
        UserManager<User> userManager,
    ILogger<ExamSeeder> logger)
        : base(context, logger)
    {
        this.context = context;
        _userManager = userManager;
    }

    public override async Task SeedAsync()
    {
        try
        {
            if (await context.Exams.AnyAsync())
                return;

            // Create sample exam
            var exam = new Exam
            {
                Title = "VSTEP B2 Sample Test",
                Description = "Practice test for VSTEP B2 certification",
                CreatedById = 1 // Ensure this admin user exists
            };

            context.Exams.Add(exam);
            await context.SaveChangesAsync();

            // Create Listening Section
            var listeningSection = new Section
            {
                ExamId = exam.Id,
                Type = SectionType.Listening,
                Title = "Listening Comprehension",
                Instructions = "In this section, you will hear several different types of recordings...",
                OrderNum = 1
            };

            context.Sections.Add(listeningSection);
            await context.SaveChangesAsync();

            // Create Listening Parts
            var part1 = new SectionPart
            {
                SectionId = listeningSection.Id,
                PartNumber = 1,
                Title = "Short Announcements",
                Instructions = "In this part, you will hear EIGHT short announcements or instructions...",
                OrderNum = 1
            };

            var part2 = new SectionPart
            {
                SectionId = listeningSection.Id,
                PartNumber = 2,
                Title = "Conversations",
                Instructions = "In this part, you will hear THREE conversations...",
                OrderNum = 2
            };

            context.SectionParts.AddRange(part1, part2);
            await context.SaveChangesAsync();

            // Add sample passage for Part 1
            var announcement1 = new Passage
            {
                SectionId = listeningSection.Id,
                PartId = part1.Id,
                Title = "Airport Announcement",
                AudioUrl = "announcements/airport.mp3",
                OrderNum = 1
            };

            context.Passages.Add(announcement1);
            await context.SaveChangesAsync();

            // Add questions for the announcement
            var question1 = new Question
            {
                SectionId = listeningSection.Id,
                PartId = part1.Id,
                PassageId = announcement1.Id,
                QuestionText = "What is the flight number mentioned in the announcement?",
                OrderNum = 1,
                Points = 1
            };

            context.Questions.Add(question1);
            await context.SaveChangesAsync();

            // Add options for the question
            var options = new[]
            {
            new QuestionOption { QuestionId = question1.Id, OptionText = "VN 123", IsCorrect = true },
            new QuestionOption { QuestionId = question1.Id, OptionText = "VN 124", IsCorrect = false },
            new QuestionOption { QuestionId = question1.Id, OptionText = "VN 125", IsCorrect = false },
            new QuestionOption { QuestionId = question1.Id, OptionText = "VN 126", IsCorrect = false }
        };

            context.QuestionOptions.AddRange(options);
            await context.SaveChangesAsync();
            _logger.LogInformation("Seeded sample VSTEP B2 exam");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding exam data");
            throw;
        }
    }
}