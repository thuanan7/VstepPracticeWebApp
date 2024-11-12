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
    public DbSet<Question> Questions { get; set; }
    public DbSet<QuestionOption> QuestionOptions { get; set; }
    public DbSet<StudentAttempt> StudentAttempts { get; set; }
    public DbSet<Answer> Answers { get; set; }

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

        // Configure Exam relationships
        builder.Entity<Exam>(entity =>
        {
            entity.HasOne(e => e.CreatedBy)
                .WithMany(u => u.CreatedExams)
                .HasForeignKey(e => e.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure Section relationships
        builder.Entity<Section>(entity =>
        {
            entity.HasOne(s => s.Exam)
                .WithMany(e => e.Sections)
                .HasForeignKey(s => s.ExamId)
                .OnDelete(DeleteBehavior.Cascade);
        });

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

        // Configure Question relationships
        builder.Entity<Question>(entity =>
        {
            entity.HasOne(q => q.Section)
                .WithMany(s => s.Questions)
                .HasForeignKey(q => q.SectionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure QuestionOption relationships
        builder.Entity<QuestionOption>(entity =>
        {
            entity.HasOne(o => o.Question)
                .WithMany(q => q.Options)
                .HasForeignKey(o => o.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(o => o.IsCorrect)
                .HasDefaultValue(false);
        });

        // Configure Answer relationships
        builder.Entity<Answer>(entity =>
        {
            entity.HasIndex(e => new { e.AttemptId, e.QuestionId })
                .IsUnique();

            entity.HasOne(a => a.Attempt)
                .WithMany(sa => sa.Answers)
                .HasForeignKey(a => a.AttemptId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(a => a.Question)
                .WithMany()
                .HasForeignKey(a => a.QuestionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(a => a.SelectedOption)
                .WithMany()
                .HasForeignKey(a => a.SelectedOptionId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}