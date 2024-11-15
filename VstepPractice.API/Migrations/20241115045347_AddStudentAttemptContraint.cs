using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VstepPractice.API.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentAttemptContraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StudentAttempts_UserId",
                table: "StudentAttempts");

            migrationBuilder.CreateIndex(
                name: "IX_StudentAttempts_UserId_ExamId_Status",
                table: "StudentAttempts",
                columns: new[] { "UserId", "ExamId", "Status" },
                unique: true,
                filter: "[Status] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StudentAttempts_UserId_ExamId_Status",
                table: "StudentAttempts");

            migrationBuilder.CreateIndex(
                name: "IX_StudentAttempts_UserId",
                table: "StudentAttempts",
                column: "UserId");
        }
    }
}
