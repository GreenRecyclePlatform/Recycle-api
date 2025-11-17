using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace recycle.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIdtoUserandDriver : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "StripeAccountId",
                table: "DriverProfiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StripeAccountId",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StripeAccountId",
                table: "DriverProfiles");

            migrationBuilder.DropColumn(
                name: "StripeAccountId",
                table: "AspNetUsers");
        }
    }
}
