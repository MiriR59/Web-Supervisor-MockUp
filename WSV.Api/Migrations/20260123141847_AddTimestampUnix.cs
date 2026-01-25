using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WSV.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddTimestampUnix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "TimestampUnixMs",
                table: "SourceReadings",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TimestampUnixMs",
                table: "SourceReadings");
        }
    }
}
