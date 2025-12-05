using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WSV.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddSourceBehaviour : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BehaviourProfile",
                table: "Sources",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsEnabled",
                table: "Sources",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BehaviourProfile",
                table: "Sources");

            migrationBuilder.DropColumn(
                name: "IsEnabled",
                table: "Sources");
        }
    }
}
