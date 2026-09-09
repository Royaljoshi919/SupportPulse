using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupportPulse.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryAndSentiment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Category aur Sentiment columns database mein pehle se maujood hain,
            // isliye yahan koi SQL command run nahi karenge taaki duplicate error na aaye.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "tickets");

            migrationBuilder.DropColumn(
                name: "Sentiment",
                table: "tickets");
        }
    }
}