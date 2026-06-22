using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CS4760GrantApplication.Migrations
{
    /// <inheritdoc />
    public partial class _0622AddReviewStatusForSubmittedGrants : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Grants",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Grants");
        }
    }
}
