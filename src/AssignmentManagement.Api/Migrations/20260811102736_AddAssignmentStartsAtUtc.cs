using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssignmentManagement.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddAssignmentStartsAtUtc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "StartsAtUtc",
                table: "Assignments",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StartsAtUtc",
                table: "Assignments");
        }
    }
}
