using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupportPulse.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddTicketEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                CREATE TABLE IF NOT EXISTS `tickets` (
                    `Id` int NOT NULL AUTO_INCREMENT,
                    `UserId` int NOT NULL,
                    `Subject` longtext NOT NULL,
                    `Description` longtext NOT NULL,
                    `Status` longtext NOT NULL,
                    `Priority` longtext NOT NULL,
                    `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
                    PRIMARY KEY (`Id`)
                ) CHARACTER SET=utf8mb4;
                """);

            migrationBuilder.Sql("""
                SET @column_exists = (
                    SELECT COUNT(*)
                    FROM information_schema.columns
                    WHERE table_schema = DATABASE()
                      AND table_name = 'tickets'
                      AND column_name = 'CreatedAt'
                );
                SET @sql = IF(
                    @column_exists = 0,
                    'ALTER TABLE `tickets` ADD COLUMN `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6)',
                    'SELECT 1'
                );
                PREPARE add_created_at FROM @sql;
                EXECUTE add_created_at;
                DEALLOCATE PREPARE add_created_at;
                """);

            migrationBuilder.Sql("""
                CREATE TABLE IF NOT EXISTS `ticket_files` (
                    `Id` int NOT NULL AUTO_INCREMENT,
                    `TicketId` int NOT NULL,
                    `FilePath` longtext NOT NULL,
                    `FileType` longtext NOT NULL,
                    PRIMARY KEY (`Id`),
                    CONSTRAINT `FK_ticket_files_tickets_TicketId`
                        FOREIGN KEY (`TicketId`) REFERENCES `tickets` (`Id`) ON DELETE CASCADE
                ) CHARACTER SET=utf8mb4;
                """);

            migrationBuilder.Sql("""
                SET @column_exists = (
                    SELECT COUNT(*)
                    FROM information_schema.columns
                    WHERE table_schema = DATABASE()
                      AND table_name = 'ticket_files'
                      AND column_name = 'TicketId'
                );
                SET @sql = IF(
                    @column_exists = 0,
                    'ALTER TABLE `ticket_files` ADD COLUMN `TicketId` int NOT NULL DEFAULT 0',
                    'SELECT 1'
                );
                PREPARE add_ticket_id FROM @sql;
                EXECUTE add_ticket_id;
                DEALLOCATE PREPARE add_ticket_id;
                """);

            migrationBuilder.Sql("""
                SET @index_exists = (
                    SELECT COUNT(*)
                    FROM information_schema.statistics
                    WHERE table_schema = DATABASE()
                      AND table_name = 'ticket_files'
                      AND index_name = 'IX_ticket_files_TicketId'
                );
                SET @sql = IF(
                    @index_exists = 0,
                    'CREATE INDEX `IX_ticket_files_TicketId` ON `ticket_files` (`TicketId`)',
                    'SELECT 1'
                );
                PREPARE add_ticket_file_index FROM @sql;
                EXECUTE add_ticket_file_index;
                DEALLOCATE PREPARE add_ticket_file_index;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ticket_files");

            migrationBuilder.DropTable(
                name: "tickets");
        }
    }
}
