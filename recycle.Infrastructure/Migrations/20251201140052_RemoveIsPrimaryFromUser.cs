using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace recycle.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveIsPrimaryFromUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPrimary",
                table: "Addresses");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPrimary",
                table: "Addresses",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
