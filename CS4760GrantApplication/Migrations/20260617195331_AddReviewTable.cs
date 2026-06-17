using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CS4760GrantApplication.Migrations
{
    /// <inheritdoc />
    public partial class AddReviewTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Review_Grants_GrantId",
                table: "Review");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Review",
                table: "Review");

            migrationBuilder.RenameTable(
                name: "Review",
                newName: "Reveiws");

            migrationBuilder.RenameIndex(
                name: "IX_Review_GrantId",
                table: "Reveiws",
                newName: "IX_Reveiws_GrantId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Reveiws",
                table: "Reveiws",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Reveiws_Grants_GrantId",
                table: "Reveiws",
                column: "GrantId",
                principalTable: "Grants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reveiws_Grants_GrantId",
                table: "Reveiws");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Reveiws",
                table: "Reveiws");

            migrationBuilder.RenameTable(
                name: "Reveiws",
                newName: "Review");

            migrationBuilder.RenameIndex(
                name: "IX_Reveiws_GrantId",
                table: "Review",
                newName: "IX_Review_GrantId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Review",
                table: "Review",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Review_Grants_GrantId",
                table: "Review",
                column: "GrantId",
                principalTable: "Grants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
