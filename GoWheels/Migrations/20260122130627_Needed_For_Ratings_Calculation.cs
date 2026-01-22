using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoWheels.Migrations
{
    /// <inheritdoc />
    public partial class Needed_For_Ratings_Calculation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RatingsCount",
                table: "AspNetUsers",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RatingsCount",
                table: "AspNetUsers");
        }
    }
}
