using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VstepPractice.API.Common.Enums;
using VstepPractice.API.Models.Entities;

namespace VstepPractice.API.Data;

public class ApplicationDbContext : IdentityDbContext<User, Role, int>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Exam> Exams { get; set; }
    public DbSet<Section> Sections { get; set; }
    public DbSet<SectionPart> SectionParts { get; set; }
    public DbSet<Passage> Passages { get; set; }
    public DbSet<Question> Questions { get; set; }
    public DbSet<QuestionOption> QuestionOptions { get; set; }
    public DbSet<StudentAttempt> StudentAttempts { get; set; }
    public DbSet<Answer> Answers { get; set; }
    public DbSet<WritingAssessment> WritingAssessments { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure Identity tables
        builder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.Property(u => u.UserName)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(256);

            // Index for email uniqueness
            entity.HasIndex(u => u.Email)
                .IsUnique();

            // Index for username uniqueness
            entity.HasIndex(u => u.UserName)
                .IsUnique();
        });

        builder.Entity<Role>(entity =>
        {
            entity.ToTable("Roles");
        });

        builder.Entity<IdentityUserRole<int>>(entity =>
        {
            entity.ToTable("UserRoles");
        });

        builder.Entity<IdentityUserClaim<int>>(entity =>
        {
            entity.ToTable("UserClaims");
        });

        builder.Entity<IdentityUserLogin<int>>(entity =>
        {
            entity.ToTable("UserLogins");
        });

        builder.Entity<IdentityRoleClaim<int>>(entity =>
        {
            entity.ToTable("RoleClaims");
        });

        builder.Entity<IdentityUserToken<int>>(entity =>
        {
            entity.ToTable("UserTokens");
        });


        // Exam -> Section relationship
        builder.Entity<Section>()
            .HasOne(s => s.Exam)
            .WithMany(e => e.Sections)
            .HasForeignKey(s => s.ExamId)
            .OnDelete(DeleteBehavior.Cascade);

        // Section -> SectionPart relationship
        builder.Entity<SectionPart>()
            .HasOne(p => p.Section)
            .WithMany(s => s.Parts)
            .HasForeignKey(p => p.SectionId)
            .OnDelete(DeleteBehavior.Restrict);

        // Section -> Passage relationship
        builder.Entity<Passage>()
            .HasOne(p => p.Section)
            .WithMany(s => s.Passages)
            .HasForeignKey(p => p.SectionId)
            .OnDelete(DeleteBehavior.Restrict);

        // SectionPart -> Passage relationship
        builder.Entity<Passage>()
            .HasOne(p => p.Part)
            .WithMany(sp => sp.Passages)
            .HasForeignKey(p => p.PartId)
            .OnDelete(DeleteBehavior.Restrict);

        // Section -> Question relationship
        builder.Entity<Question>()
            .HasOne(q => q.Section)
            .WithMany(s => s.Questions)
            .HasForeignKey(q => q.SectionId)
            .OnDelete(DeleteBehavior.Restrict);

        // SectionPart -> Question relationship
        builder.Entity<Question>()
            .HasOne(q => q.Part)
            .WithMany(p => p.Questions)
            .HasForeignKey(q => q.PartId)
            .OnDelete(DeleteBehavior.Restrict);

        // Passage -> Question relationship
        builder.Entity<Question>()
            .HasOne(q => q.Passage)
            .WithMany(p => p.Questions)
            .HasForeignKey(q => q.PassageId)
            .OnDelete(DeleteBehavior.Restrict);

        // Question -> QuestionOption relationship
        builder.Entity<QuestionOption>()
            .HasOne(o => o.Question)
            .WithMany(q => q.Options)
            .HasForeignKey(o => o.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);

        // StudentAttempt relationships
        builder.Entity<StudentAttempt>()
            .HasOne(sa => sa.User)
            .WithMany(u => u.StudentAttempts)
            .HasForeignKey(sa => sa.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<StudentAttempt>()
            .HasOne(sa => sa.Exam)
            .WithMany(e => e.StudentAttempts)
            .HasForeignKey(sa => sa.ExamId)
            .OnDelete(DeleteBehavior.Restrict);

        // Answer relationships
        builder.Entity<Answer>()
            .HasOne(a => a.Attempt)
            .WithMany(sa => sa.Answers)
            .HasForeignKey(a => a.AttemptId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Answer>()
            .HasOne(a => a.Question)
            .WithMany()
            .HasForeignKey(a => a.QuestionId)
            .OnDelete(DeleteBehavior.Restrict);

        // Update Answer configuration to make SelectedOptionId optional
        builder.Entity<Answer>()
            .HasOne(a => a.SelectedOption)
            .WithMany()
            .HasForeignKey(a => a.SelectedOptionId)
            .IsRequired(false)  // Make the relationship optional
            .OnDelete(DeleteBehavior.Restrict);

        // Configure StudentAttempt relationships
        builder.Entity<StudentAttempt>(entity =>
        {
            entity.HasOne(a => a.User)
                .WithMany(u => u.StudentAttempts)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(a => a.Exam)
                .WithMany(e => e.StudentAttempts)
                .HasForeignKey(a => a.ExamId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Property(e => e.Status)
                .HasDefaultValue(AttemptStatus.InProgress);
        });

        // WritingAssessment configuration
        builder.Entity<WritingAssessment>()
            .HasOne(wa => wa.Answer)
            .WithOne()
            .HasForeignKey<WritingAssessment>(wa => wa.AnswerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<WritingAssessment>()
            .Property(wa => wa.TaskAchievement)
            .HasPrecision(4, 2);

        builder.Entity<WritingAssessment>()
            .Property(wa => wa.CoherenceCohesion)
            .HasPrecision(4, 2);

        builder.Entity<WritingAssessment>()
            .Property(wa => wa.LexicalResource)
            .HasPrecision(4, 2);

        builder.Entity<WritingAssessment>()
            .Property(wa => wa.GrammarAccuracy)
            .HasPrecision(4, 2);
    }
}