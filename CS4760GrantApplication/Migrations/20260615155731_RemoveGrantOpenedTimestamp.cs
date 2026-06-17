using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CS4760GrantApplication.Migrations
{
    /// <inheritdoc />
    public partial class RemoveGrantOpenedTimestamp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OpenedTimestamp",
                table: "Grants");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "OpenedTimestamp",
                table: "Grants",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
