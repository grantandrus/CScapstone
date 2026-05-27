using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CS4760GrantApplication.Migrations
{
    /// <inheritdoc />
    public partial class UpdateGrantModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "title",
                table: "Grants",
                newName: "Title");

            migrationBuilder.RenameColumn(
                name: "description",
                table: "Grants",
                newName: "Description");

            migrationBuilder.AddColumn<int>(
                name: "DepartmentId",
                table: "Grants",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Justification",
                table: "Grants",
                type: "nvarchar(max)",
                maxLength: 5000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ProjectImpact",
                table: "Grants",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ProjectSummary",
                table: "Grants",
                type: "nvarchar(max)",
                maxLength: 5000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "isMultipleDepartments",
                table: "Grants",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Grants_DepartmentId",
                table: "Grants",
                column: "DepartmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Grants_Departments_DepartmentId",
                table: "Grants",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Grants_Departments_DepartmentId",
                table: "Grants");

            migrationBuilder.DropIndex(
                name: "IX_Grants_DepartmentId",
                table: "Grants");

            migrationBuilder.DropColumn(
                name: "DepartmentId",
                table: "Grants");

            migrationBuilder.DropColumn(
                name: "Justification",
                table: "Grants");

            migrationBuilder.DropColumn(
                name: "ProjectImpact",
                table: "Grants");

            migrationBuilder.DropColumn(
                name: "ProjectSummary",
                table: "Grants");

            migrationBuilder.DropColumn(
                name: "isMultipleDepartments",
                table: "Grants");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "Grants",
                newName: "title");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "Grants",
                newName: "description");
        }
    }
}
