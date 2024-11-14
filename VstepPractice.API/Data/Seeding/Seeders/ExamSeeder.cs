using Microsoft.EntityFrameworkCore;
using VstepPractice.API.Common.Enums;
using VstepPractice.API.Data.Seeding.Abstracts;
using VstepPractice.API.Models.Entities;

namespace VstepPractice.API.Data.Seeding.Seeders;

public class ExamSeeder : BaseSeeder
{
    public ExamSeeder(ApplicationDbContext context, ILogger<ExamSeeder> logger)
        : base(context, logger)
    {
    }

    public override async Task SeedAsync()
    {
        try
        {
            if (await _context.Exams.AnyAsync())
            {
                _logger.LogInformation("Exams already seeded");
                return;
            }

            await _context.Database.BeginTransactionAsync();

            var admin = await _context.Users.FirstAsync(u => u.Email == "admin@vstep.com");

            // Create exam
            var exam = new Exam
            {
                Title = "VSTEP B2 Sample Test (With Writing)",
                Description = "A complete VSTEP B2 test including Listening, Reading and Writing sections",
                CreatedById = admin.Id
            };

            _context.Exams.Add(exam);
            await _context.SaveChangesAsync();

            // Listening Section
            var listeningSection = new Section
            {
                ExamId = exam.Id,
                Type = SectionType.Listening,
                Title = "Listening Comprehension",
                Instructions = "Listen to audio recordings and answer multiple choice questions.",
                OrderNum = 1
            };
            _context.Sections.Add(listeningSection);
            await _context.SaveChangesAsync();

            // Listening Part 1
            var listeningPart1 = new SectionPart
            {
                SectionId = listeningSection.Id,
                PartNumber = 1,
                Title = "Part 1: Short Conversations",
                Instructions = "Listen to 8 short conversations and choose the best answer.",
                OrderNum = 1
            };
            _context.SectionParts.Add(listeningPart1);
            await _context.SaveChangesAsync();

            // Listening Passage
            var listeningPassage = new Passage
            {
                SectionId = listeningSection.Id,
                PartId = listeningPart1.Id,
                Title = "Conversation 1",
                Content = "Audio URL for conversation 1",
                OrderNum = 1
            };
            _context.Passages.Add(listeningPassage);
            await _context.SaveChangesAsync();

            // Listening Question
            var listeningQuestion = new Question
            {
                SectionId = listeningSection.Id,
                PartId = listeningPart1.Id,
                PassageId = listeningPassage.Id,
                QuestionText = "What will the woman probably do?",
                OrderNum = 1,
                Points = 1
            };
            _context.Questions.Add(listeningQuestion);
            await _context.SaveChangesAsync();

            // Listening Options
            var listeningOptions = new[]
            {
                new QuestionOption { QuestionId = listeningQuestion.Id, OptionText = "Go to the beach", IsCorrect = false },
                new QuestionOption { QuestionId = listeningQuestion.Id, OptionText = "Stay home", IsCorrect = true },
                new QuestionOption { QuestionId = listeningQuestion.Id, OptionText = "Visit her friend", IsCorrect = false }
            };
            _context.QuestionOptions.AddRange(listeningOptions);
            await _context.SaveChangesAsync();

            // Reading Section
            var readingSection = new Section
            {
                ExamId = exam.Id,
                Type = SectionType.Reading,
                Title = "Reading Comprehension",
                Instructions = "Read the passages and answer the questions that follow.",
                OrderNum = 2
            };
            _context.Sections.Add(readingSection);
            await _context.SaveChangesAsync();

            // Reading Part 1
            var readingPart1 = new SectionPart
            {
                SectionId = readingSection.Id,
                PartNumber = 1,
                Title = "Part 1: Short Texts",
                Instructions = "Read the texts and answer the questions.",
                OrderNum = 1
            };
            _context.SectionParts.Add(readingPart1);
            await _context.SaveChangesAsync();

            // Reading Passage
            var readingPassage = new Passage
            {
                SectionId = readingSection.Id,
                PartId = readingPart1.Id,
                Title = "Text 1: Environmental Conservation",
                Content = @"Environmental conservation has become increasingly important in recent years. 
                          Climate change poses significant threats to our planet's ecosystems. 
                          Many species are at risk of extinction due to habitat loss and pollution. 
                          Scientists warn that immediate action is necessary to prevent irreversible damage.",
                OrderNum = 1
            };
            _context.Passages.Add(readingPassage);
            await _context.SaveChangesAsync();

            // Reading Question
            var readingQuestion = new Question
            {
                SectionId = readingSection.Id,
                PartId = readingPart1.Id,
                PassageId = readingPassage.Id,
                QuestionText = "What is the main idea of this passage?",
                OrderNum = 1,
                Points = 1
            };
            _context.Questions.Add(readingQuestion);
            await _context.SaveChangesAsync();

            // Reading Options
            var readingOptions = new[]
            {
                new QuestionOption { QuestionId = readingQuestion.Id, OptionText = "The importance of environmental conservation", IsCorrect = true },
                new QuestionOption { QuestionId = readingQuestion.Id, OptionText = "The history of climate change", IsCorrect = false },
                new QuestionOption { QuestionId = readingQuestion.Id, OptionText = "Methods of preventing pollution", IsCorrect = false }
            };
            _context.QuestionOptions.AddRange(readingOptions);
            await _context.SaveChangesAsync();

            // Writing Section
            var writingSection = new Section
            {
                ExamId = exam.Id,
                Type = SectionType.Writing,
                Title = "Writing",
                Instructions = "Complete both writing tasks. Pay attention to task requirements.",
                OrderNum = 3
            };
            _context.Sections.Add(writingSection);
            await _context.SaveChangesAsync();

            // Writing Task 1
            var writingPart1 = new SectionPart
            {
                SectionId = writingSection.Id,
                PartNumber = 1,
                Title = "Task 1",
                Instructions = "Write a formal letter (150-180 words)",
                OrderNum = 1
            };
            _context.SectionParts.Add(writingPart1);
            await _context.SaveChangesAsync();

            var writingPassage1 = new Passage
            {
                SectionId = writingSection.Id,
                PartId = writingPart1.Id,
                Title = "Formal Letter Task",
                Content = @"You recently visited a local museum and were disappointed with your experience.
                        Write a letter to the museum director. In your letter:
                        - Explain when you visited the museum
                        - Describe what disappointed you
                        - Suggest what improvements could be made",
                OrderNum = 1
            };
            _context.Passages.Add(writingPassage1);
            await _context.SaveChangesAsync();

            var writingQuestion1 = new Question
            {
                SectionId = writingSection.Id,
                PartId = writingPart1.Id,
                PassageId = writingPassage1.Id,
                QuestionText = @"Write a formal letter to the museum director addressing the points mentioned above.
                            Write at least 150 words.
                            You do NOT need to write any addresses.
                            Begin your letter with ""Dear Sir/Madam,""",
                OrderNum = 1,
                Points = 10
            };
            _context.Questions.Add(writingQuestion1);
            await _context.SaveChangesAsync();

            // Writing Task 2
            var writingPart2 = new SectionPart
            {
                SectionId = writingSection.Id,
                PartNumber = 2,
                Title = "Task 2",
                Instructions = "Write an essay (250-280 words)",
                OrderNum = 2
            };
            _context.SectionParts.Add(writingPart2);
            await _context.SaveChangesAsync();

            var writingPassage2 = new Passage
            {
                SectionId = writingSection.Id,
                PartId = writingPart2.Id,
                Title = "Essay Task",
                Content = @"Some people believe that students should be required to learn a foreign language in school,
                        while others believe it should be optional.
                        Discuss both views and give your opinion.",
                OrderNum = 1
            };
            _context.Passages.Add(writingPassage2);
            await _context.SaveChangesAsync();

            var writingQuestion2 = new Question
            {
                SectionId = writingSection.Id,
                PartId = writingPart2.Id,
                PassageId = writingPassage2.Id,
                QuestionText = @"Write about the following topic:

                            Some people believe that students should be required to learn a foreign language in school,
                            while others believe it should be optional.
                            Discuss both views and give your opinion.

                            Write at least 250 words.
                            Give reasons for your answer and include any relevant examples from your own experience.",
                OrderNum = 1,
                Points = 10
            };
            _context.Questions.Add(writingQuestion2);
            await _context.SaveChangesAsync();

            await _context.Database.CommitTransactionAsync();
            _logger.LogInformation("Seeded sample VSTEP B2 exam");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding exam data");
            await _context.Database.RollbackTransactionAsync();
            throw;
        }
    }
}