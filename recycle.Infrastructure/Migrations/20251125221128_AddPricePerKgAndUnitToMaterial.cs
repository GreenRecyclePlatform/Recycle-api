using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace recycle.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPricePerKgAndUnitToMaterial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "BuyingPrice",
                table: "Materials",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Icon",
                table: "Materials",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Image",
                table: "Materials",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SellingPrice",
                table: "Materials",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Materials",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BuyingPrice",
                table: "Materials");

            migrationBuilder.DropColumn(
                name: "Icon",
                table: "Materials");

            migrationBuilder.DropColumn(
                name: "Image",
                table: "Materials");

            migrationBuilder.DropColumn(
                name: "SellingPrice",
                table: "Materials");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Materials");
        }
    }
}
