using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VstepPractice.API.Common.Enums;
using VstepPractice.API.Data.Seeding.Abstracts;
using VstepPractice.API.Models.Entities;

namespace VstepPractice.API.Data.Seeding.Seeders;

public class ExamSeeder : BaseSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;

    public ExamSeeder(
        ApplicationDbContext context,
        UserManager<User> userManager,
        ILogger<ExamSeeder> logger)
        : base(context, logger)
    {
        _context = context;
        _userManager = userManager;
    }

    public override async Task SeedAsync()
    {
        try
        {
            if (await _context.Exams.AnyAsync())
                return;

            // Find admin user (assuming it exists from UserSeeder)
            var admin = await _userManager.FindByEmailAsync("admin@vstep.com");
            if (admin == null)
            {
                _logger.LogError("Admin user not found for exam seeding");
                return;
            }

            // Create sample exam
            var exam = new Exam
            {
                Title = "VSTEP B2 Sample Test (With Writing)",
                Description = "Practice test for VSTEP B2 certification",
                CreatedById = admin.Id
            };

            _context.Exams.Add(exam);
            await _context.SaveChangesAsync();

            // Create Writing Section
            var writingSection = new Section
            {
                ExamId = exam.Id,
                Type = SectionType.Writing,
                Title = "Writing Section",
                Instructions = """
                    This section tests your ability to write clear, detailed text on a variety of topics.
                    You will have 60 minutes to complete two writing tasks.
                    Task 1 requires you to write at least 150 words.
                    Task 2 requires you to write at least 250 words.
                    """,
                OrderNum = 1
            };

            _context.Sections.Add(writingSection);
            await _context.SaveChangesAsync();

            // Create Writing Parts
            var part1 = new SectionPart
            {
                SectionId = writingSection.Id,
                PartNumber = 1,
                Title = "Task 1 - Letter Writing",
                Instructions = "You should spend about 20 minutes on this task. Write at least 150 words.",
                OrderNum = 1
            };

            var part2 = new SectionPart
            {
                SectionId = writingSection.Id,
                PartNumber = 2,
                Title = "Task 2 - Essay Writing",
                Instructions = "You should spend about 40 minutes on this task. Write at least 250 words.",
                OrderNum = 2
            };

            _context.SectionParts.AddRange(part1, part2);
            await _context.SaveChangesAsync();

            // Add sample passage/prompt for Part 1 (Letter Writing)
            var letterPrompt = new Passage
            {
                SectionId = writingSection.Id,
                PartId = part1.Id,
                Title = "Formal Letter Task",
                Content = """
                    You recently visited a local museum and were disappointed with your experience.
                    Write a letter to the museum director. In your letter:
                    - Explain when you visited the museum
                    - Describe what disappointed you
                    - Suggest what improvements could be made
                    """,
                OrderNum = 1
            };

            _context.Passages.Add(letterPrompt);
            await _context.SaveChangesAsync();

            // Add question for letter task
            var letterQuestion = new Question
            {
                SectionId = writingSection.Id,
                PartId = part1.Id,
                PassageId = letterPrompt.Id,
                QuestionText = """
                    Write a formal letter to the museum director addressing the points mentioned above.
                    Write at least 150 words.
                    You do NOT need to write any addresses.
                    Begin your letter with "Dear Sir/Madam,"
                    """,
                Points = 20,
                OrderNum = 1
            };

            _context.Questions.Add(letterQuestion);
            await _context.SaveChangesAsync();

            // Add sample passage/prompt for Part 2 (Essay Writing)
            var essayPrompt = new Passage
            {
                SectionId = writingSection.Id,
                PartId = part2.Id,
                Title = "Essay Task",
                Content = """
                    Some people believe that students should be required to learn a foreign language in school,
                    while others believe it should be optional.
                    Discuss both views and give your opinion.
                    """,
                OrderNum = 1
            };

            _context.Passages.Add(essayPrompt);
            await _context.SaveChangesAsync();

            // Add question for essay task
            var essayQuestion = new Question
            {
                SectionId = writingSection.Id,
                PartId = part2.Id,
                PassageId = essayPrompt.Id,
                QuestionText = """
                    Write about the following topic:
                    
                    Some people believe that students should be required to learn a foreign language in school,
                    while others believe it should be optional.
                    Discuss both views and give your opinion.

                    Write at least 250 words.
                    Give reasons for your answer and include any relevant examples from your own experience.
                    """,
                Points = 30,
                OrderNum = 2
            };

            _context.Questions.Add(essayQuestion);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully seeded exam with writing section");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding exam data");
            throw;
        }
    }
}