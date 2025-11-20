using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace recycle.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixApprovedByAdminColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
        DECLARE @var sysname;
        SELECT @var = [d].[name]
        FROM [sys].[default_constraints] [d]
        INNER JOIN [sys].[columns] [c] 
            ON [d].[parent_column_id] = [c].[column_id] 
           AND [d].[parent_object_id] = [c].[object_id]
        WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Payments]') 
          AND [c].[name] = N'ApprovedByAdminID');

        IF @var IS NOT NULL 
            EXEC('ALTER TABLE [Payments] DROP CONSTRAINT [' + @var + ']');

        ALTER TABLE [Payments] ALTER COLUMN [ApprovedByAdminID] UNIQUEIDENTIFIER NULL;
    ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
