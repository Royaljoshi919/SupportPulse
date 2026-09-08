using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupportPulse.Api.Migrations
{
    /// <inheritdoc />
    public partial class RepairTicketUserId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                SET @column_exists = (
                    SELECT COUNT(*)
                    FROM information_schema.columns
                    WHERE table_schema = DATABASE()
                      AND table_name = 'tickets'
                      AND column_name = 'UserId'
                );
                SET @sql = IF(
                    @column_exists = 0,
                    'ALTER TABLE `tickets` ADD COLUMN `UserId` int NOT NULL DEFAULT 0',
                    'SELECT 1'
                );
                PREPARE add_user_id FROM @sql;
                EXECUTE add_user_id;
                DEALLOCATE PREPARE add_user_id;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
