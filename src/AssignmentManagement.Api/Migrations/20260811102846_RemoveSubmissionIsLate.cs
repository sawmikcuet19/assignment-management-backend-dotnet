using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssignmentManagement.Api.Migrations
{
    /// <inheritdoc />
    public partial class RemoveSubmissionIsLate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsLate",
                table: "Submissions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsLate",
                table: "Submissions",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
