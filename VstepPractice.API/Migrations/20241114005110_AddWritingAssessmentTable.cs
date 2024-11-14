using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VstepPractice.API.Migrations
{
    /// <inheritdoc />
    public partial class AddWritingAssessmentTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WritingAssessments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AnswerId = table.Column<int>(type: "int", nullable: false),
                    TaskAchievement = table.Column<decimal>(type: "decimal(4,2)", precision: 4, scale: 2, nullable: false),
                    CoherenceCohesion = table.Column<decimal>(type: "decimal(4,2)", precision: 4, scale: 2, nullable: false),
                    LexicalResource = table.Column<decimal>(type: "decimal(4,2)", precision: 4, scale: 2, nullable: false),
                    GrammarAccuracy = table.Column<decimal>(type: "decimal(4,2)", precision: 4, scale: 2, nullable: false),
                    DetailedFeedback = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AssessedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WritingAssessments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WritingAssessments_Answers_AnswerId",
                        column: x => x.AnswerId,
                        principalTable: "Answers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WritingAssessments_AnswerId",
                table: "WritingAssessments",
                column: "AnswerId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WritingAssessments");
        }
    }
}
