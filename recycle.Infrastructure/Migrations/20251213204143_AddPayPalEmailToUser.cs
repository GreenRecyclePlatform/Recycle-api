using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace recycle.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPayPalEmailToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PayPalEmail",
                table: "AspNetUsers",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PayPalEmail",
                table: "AspNetUsers");
        }
    }
}
