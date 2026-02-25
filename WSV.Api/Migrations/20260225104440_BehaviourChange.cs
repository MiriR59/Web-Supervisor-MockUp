using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WSV.Api.Migrations
{
    /// <inheritdoc />
    public partial class BehaviourChange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BehaviourProfile",
                table: "Sources");

            migrationBuilder.AddColumn<int>(
                name: "Behaviour",
                table: "Sources",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Behaviour",
                table: "Sources");

            migrationBuilder.AddColumn<string>(
                name: "BehaviourProfile",
                table: "Sources",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
