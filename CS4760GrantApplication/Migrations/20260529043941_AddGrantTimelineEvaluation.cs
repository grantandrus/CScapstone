using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CS4760GrantApplication.Migrations
{
    /// <inheritdoc />
    public partial class AddGrantTimelineEvaluation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Grants_Departments_DepartmentId",
                table: "Grants");

            migrationBuilder.DropForeignKey(
                name: "FK_Grants_Users_UserId",
                table: "Grants");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "Grants",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "DepartmentId",
                table: "Grants",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProjectTimeline",
                table: "Grants",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SuccessEvaluation",
                table: "Grants",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_Grants_Departments_DepartmentId",
                table: "Grants",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Grants_Users_UserId",
                table: "Grants",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Grants_Departments_DepartmentId",
                table: "Grants");

            migrationBuilder.DropForeignKey(
                name: "FK_Grants_Users_UserId",
                table: "Grants");

            migrationBuilder.DropColumn(
                name: "ProjectTimeline",
                table: "Grants");

            migrationBuilder.DropColumn(
                name: "SuccessEvaluation",
                table: "Grants");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "Grants",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "DepartmentId",
                table: "Grants",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Grants_Departments_DepartmentId",
                table: "Grants",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Grants_Users_UserId",
                table: "Grants",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
