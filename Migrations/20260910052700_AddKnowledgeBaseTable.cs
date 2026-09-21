using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupportPulse.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddKnowledgeBaseTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // migrationBuilder.CreateTable(
            //     name: "knowledge_base",
            //     columns: table => new
            //     {
            //         id = table.Column<int>(type: "int", nullable: false)
            //             .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
            //         title = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
            //             .Annotation("MySql:CharSet", "utf8mb4"),
            //         content = table.Column<string>(type: "longtext", nullable: false)
            //             .Annotation("MySql:CharSet", "utf8mb4"),
            //         category = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
            //             .Annotation("MySql:CharSet", "utf8mb4"),
            //         created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
            //         updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
            //     },
            //     constraints: table =>
            //     {
            //         table.PrimaryKey("PK_knowledge_base", x => x.id);
            //     })
            //     .Annotation("MySql:CharSet", "utf8mb4");

            // migrationBuilder.CreateTable(
            //     name: "ticket_notes",
            //     columns: table => new
            //     {
            //         id = table.Column<int>(type: "int", nullable: false)
            //             .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
            //         ticket_id = table.Column<int>(type: "int", nullable: false),
            //         agent_id = table.Column<int>(type: "int", nullable: false),
            //         note = table.Column<string>(type: "longtext", nullable: false)
            //             .Annotation("MySql:CharSet", "utf8mb4"),
            //         created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
            //     },
            //     constraints: table =>
            //     {
            //         table.PrimaryKey("PK_ticket_notes", x => x.id);
            //         table.ForeignKey(
            //             name: "FK_ticket_notes_tickets_ticket_id",
            //             column: x => x.ticket_id,
            //             principalTable: "tickets",
            //             principalColumn: "Id",
            //             onDelete: ReferentialAction.Cascade);
            //         table.ForeignKey(
            //             name: "FK_ticket_notes_users_agent_id",
            //             column: x => x.agent_id,
            //             principalTable: "users",
            //             principalColumn: "Id",
            //             onDelete: ReferentialAction.Cascade);
            //     })
            //     .Annotation("MySql:CharSet", "utf8mb4");

            // migrationBuilder.CreateIndex(
            //     name: "IX_ticket_notes_agent_id",
            //     table: "ticket_notes",
            //     column: "agent_id");

            // migrationBuilder.CreateIndex(
            //     name: "IX_ticket_notes_ticket_id",
            //     table: "ticket_notes",
            //     column: "ticket_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "knowledge_base");

            migrationBuilder.DropTable(
                name: "ticket_notes");
        }
    }
}
