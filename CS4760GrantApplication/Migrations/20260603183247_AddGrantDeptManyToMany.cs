using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CS4760GrantApplication.Migrations
{
    /// <inheritdoc />
    public partial class AddGrantDeptManyToMany : Migration
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

            migrationBuilder.DropIndex(
                name: "IX_Grants_DepartmentId",
                table: "Grants");

            migrationBuilder.DropColumn(
                name: "DepartmentId",
                table: "Grants");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "Grants",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateTable(
                name: "DepartmentGrant",
                columns: table => new
                {
                    DepartmentsId = table.Column<int>(type: "int", nullable: false),
                    GrantsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DepartmentGrant", x => new { x.DepartmentsId, x.GrantsId });
                    table.ForeignKey(
                        name: "FK_DepartmentGrant_Departments_DepartmentsId",
                        column: x => x.DepartmentsId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DepartmentGrant_Grants_GrantsId",
                        column: x => x.GrantsId,
                        principalTable: "Grants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentGrant_GrantsId",
                table: "DepartmentGrant",
                column: "GrantsId");

            migrationBuilder.AddForeignKey(
                name: "FK_Grants_Users_UserId",
                table: "Grants",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Grants_Users_UserId",
                table: "Grants");

            migrationBuilder.DropTable(
                name: "DepartmentGrant");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "Grants",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DepartmentId",
                table: "Grants",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Grants_DepartmentId",
                table: "Grants",
                column: "DepartmentId");

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
    }
}
