using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CS4760GrantApplication.Migrations
{
    /// <inheritdoc />
    public partial class GrantAcknowledgements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Acknowledgement1",
                table: "Grants",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Acknowledgement2",
                table: "Grants",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Acknowledgement3",
                table: "Grants",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Acknowledgement4",
                table: "Grants",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "OpenedTimestamp",
                table: "Grants",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Signature",
                table: "Grants",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "SignatureDate",
                table: "Grants",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Acknowledgement1",
                table: "Grants");

            migrationBuilder.DropColumn(
                name: "Acknowledgement2",
                table: "Grants");

            migrationBuilder.DropColumn(
                name: "Acknowledgement3",
                table: "Grants");

            migrationBuilder.DropColumn(
                name: "Acknowledgement4",
                table: "Grants");

            migrationBuilder.DropColumn(
                name: "OpenedTimestamp",
                table: "Grants");

            migrationBuilder.DropColumn(
                name: "Signature",
                table: "Grants");

            migrationBuilder.DropColumn(
                name: "SignatureDate",
                table: "Grants");
        }
    }
}
