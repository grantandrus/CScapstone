using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CS4760GrantApplication.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueReviewConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reveiws_Grants_GrantId",
                table: "Reveiws");

            migrationBuilder.DropForeignKey(
                name: "FK_Reveiws_Users_UserID",
                table: "Reveiws");

            migrationBuilder.DropIndex(
                name: "IX_Reveiws_UserID",
                table: "Reveiws");

            migrationBuilder.RenameColumn(
                name: "UserID",
                table: "Reveiws",
                newName: "UserId");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "Reveiws",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "GrantId",
                table: "Reveiws",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_Reveiws_UserId_GrantId",
                table: "Reveiws",
                columns: new[] { "UserId", "GrantId" },
                unique: true,
                filter: "[UserId] IS NOT NULL AND [GrantId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Reveiws_Grants_GrantId",
                table: "Reveiws",
                column: "GrantId",
                principalTable: "Grants",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Reveiws_Users_UserId",
                table: "Reveiws",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reveiws_Grants_GrantId",
                table: "Reveiws");

            migrationBuilder.DropForeignKey(
                name: "FK_Reveiws_Users_UserId",
                table: "Reveiws");

            migrationBuilder.DropIndex(
                name: "IX_Reveiws_UserId_GrantId",
                table: "Reveiws");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Reveiws",
                newName: "UserID");

            migrationBuilder.AlterColumn<int>(
                name: "UserID",
                table: "Reveiws",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "GrantId",
                table: "Reveiws",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reveiws_UserID",
                table: "Reveiws",
                column: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_Reveiws_Grants_GrantId",
                table: "Reveiws",
                column: "GrantId",
                principalTable: "Grants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reveiws_Users_UserID",
                table: "Reveiws",
                column: "UserID",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
