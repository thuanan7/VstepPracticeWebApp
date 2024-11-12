using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VstepPractice.API.Migrations
{
    /// <inheritdoc />
    public partial class RemoveRoleColumnInUserTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Role",
                table: "Users");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Role",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 4);
        }
    }
}
