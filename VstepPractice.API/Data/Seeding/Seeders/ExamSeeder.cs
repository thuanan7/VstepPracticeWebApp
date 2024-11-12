using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VstepPractice.API.Common.Enums;
using VstepPractice.API.Data.Seeding.Abstracts;
using VstepPractice.API.Models.Entities;

namespace VstepPractice.API.Data.Seeding.Seeders;

public class ExamSeeder : BaseSeeder
{
    private readonly UserManager<User> _userManager;

    public ExamSeeder(
        ApplicationDbContext context,
        UserManager<User> userManager,
    ILogger<ExamSeeder> logger)
        : base(context, logger)
    {
        _userManager = userManager;
    }

    public override async Task SeedAsync()
    {
        try
        {
            if (await _context.Exams.AnyAsync())
                return;

            var teacher = await _userManager.FindByEmailAsync("teacher1@vstep.com");
            if (teacher == null)
                throw new InvalidOperationException("Teacher user not found");

            var exam = new Exam
            {
                Title = "VSTEP B2 Practice Exam 2024",
                Description = "Comprehensive practice exam for VSTEP B2 certification",
                CreatedById = teacher.Id,
                Sections = new List<Section>
                {
                    new Section
                    {
                        Type = SectionType.Listening,
                        Title = "Listening Comprehension",
                        Instructions = "Listen to the audio and answer the questions. You will hear each recording twice.",
                        OrderNum = 1,
                        Questions = new List<Question>
                        {
                            new Question
                            {
                                QuestionText = "Listen to the conversation about a company meeting. What is the main topic discussed?",
                                MediaUrl = "/audio/listening1.mp3",
                                Points = 2,
                                Options = new List<QuestionOption>
                                {
                                    new QuestionOption { OptionText = "Budget planning for next year", IsCorrect = true },
                                    new QuestionOption { OptionText = "Employee performance reviews" },
                                    new QuestionOption { OptionText = "Office renovation plans" },
                                    new QuestionOption { OptionText = "Marketing strategy" }
                                }
                            },
                            new Question
                            {
                                QuestionText = "What does the speaker suggest about project deadlines?",
                                MediaUrl = "/audio/listening2.mp3",
                                Points = 2,
                                Options = new List<QuestionOption>
                                {
                                    new QuestionOption { OptionText = "They should be extended" },
                                    new QuestionOption { OptionText = "They are unrealistic", IsCorrect = true },
                                    new QuestionOption { OptionText = "They are too flexible" },
                                    new QuestionOption { OptionText = "They need more discussion" }
                                }
                            }
                        }
                    },
                    new Section
                    {
                        Type = SectionType.Reading,
                        Title = "Reading Comprehension",
                        Instructions = "Read the passages carefully and answer the questions that follow.",
                        OrderNum = 2,
                        Questions = new List<Question>
                        {
                            new Question
                            {
                                QuestionText = "Based on the passage, what is the author's main argument about climate change?",
                                Points = 2,
                                Options = new List<QuestionOption>
                                {
                                    new QuestionOption { OptionText = "It is an unsolvable problem" },
                                    new QuestionOption { OptionText = "Individual actions have no impact" },
                                    new QuestionOption { OptionText = "Immediate collective action is necessary", IsCorrect = true },
                                    new QuestionOption { OptionText = "Technology will solve everything" }
                                }
                            },
                            new Question
                            {
                                QuestionText = "What evidence does the author provide to support the economic impact?",
                                Points = 2,
                                Options = new List<QuestionOption>
                                {
                                    new QuestionOption { OptionText = "Historical data only" },
                                    new QuestionOption { OptionText = "Expert opinions and statistical analysis", IsCorrect = true },
                                    new QuestionOption { OptionText = "Personal observations" },
                                    new QuestionOption { OptionText = "Media reports" }
                                }
                            }
                        }
                    },
                    new Section
                    {
                        Type = SectionType.Writing,
                        Title = "Writing",
                        Instructions = "Write an essay of at least 250 words on the given topic. Pay attention to structure, grammar, and coherence.",
                        OrderNum = 3,
                        Questions = new List<Question>
                        {
                            new Question
                            {
                                QuestionText = "Some people believe that technology has made life more complicated, while others think it has made life easier. Discuss both views and give your opinion.",
                                Points = 10
                            }
                        }
                    }
                }
            };

            _context.Exams.Add(exam);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Seeded sample VSTEP B2 exam");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding exam data");
            throw;
        }
    }
}