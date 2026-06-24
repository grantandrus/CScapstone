using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CS4760GrantApplication.Migrations
{
    /// <inheritdoc />
    public partial class UpdateReviewGrantRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Reveiws_GrantId",
                table: "Reveiws");

            migrationBuilder.AddColumn<int>(
                name: "UserID",
                table: "Reveiws",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Reveiws_GrantId",
                table: "Reveiws",
                column: "GrantId");

            migrationBuilder.CreateIndex(
                name: "IX_Reveiws_UserID",
                table: "Reveiws",
                column: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_Reveiws_Users_UserID",
                table: "Reveiws",
                column: "UserID",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reveiws_Users_UserID",
                table: "Reveiws");

            migrationBuilder.DropIndex(
                name: "IX_Reveiws_GrantId",
                table: "Reveiws");

            migrationBuilder.DropIndex(
                name: "IX_Reveiws_UserID",
                table: "Reveiws");

            migrationBuilder.DropColumn(
                name: "UserID",
                table: "Reveiws");

            migrationBuilder.CreateIndex(
                name: "IX_Reveiws_GrantId",
                table: "Reveiws",
                column: "GrantId",
                unique: true);
        }
    }
}
