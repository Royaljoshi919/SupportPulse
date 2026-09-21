using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupportPulse.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddImageDescriptionColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "sentiment",
                table: "ticket_ai_results");

            migrationBuilder.DropColumn(
                name: "summary",
                table: "ticket_ai_results");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "ticket_ai_results",
                newName: "id");

            migrationBuilder.AlterColumn<string>(
                name: "transcription",
                table: "ticket_ai_results",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "image_description",
                table: "ticket_ai_results",
                type: "text",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "image_description",
                table: "ticket_ai_results");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "ticket_ai_results",
                newName: "Id");

            migrationBuilder.AlterColumn<string>(
                name: "transcription",
                table: "ticket_ai_results",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "sentiment",
                table: "ticket_ai_results",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "summary",
                table: "ticket_ai_results",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
