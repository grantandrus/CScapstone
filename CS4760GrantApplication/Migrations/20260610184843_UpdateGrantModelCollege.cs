using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CS4760GrantApplication.Migrations
{
    /// <inheritdoc />
    public partial class UpdateGrantModelCollege : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CollegeId",
                table: "Grants",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "CollegeId",
                table: "Departments",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_Grants_CollegeId",
                table: "Grants",
                column: "CollegeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Grants_Colleges_CollegeId",
                table: "Grants",
                column: "CollegeId",
                principalTable: "Colleges",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Grants_Colleges_CollegeId",
                table: "Grants");

            migrationBuilder.DropIndex(
                name: "IX_Grants_CollegeId",
                table: "Grants");

            migrationBuilder.DropColumn(
                name: "CollegeId",
                table: "Grants");

            migrationBuilder.AlterColumn<int>(
                name: "CollegeId",
                table: "Departments",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
