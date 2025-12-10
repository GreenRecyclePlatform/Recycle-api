using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace recycle.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixSupplierOrdersAddItemsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1️⃣ إضافة الـ Columns الناقصة في SupplierOrders
            migrationBuilder.AddColumn<DateTime>(
                name: "PaidAt",
                table: "SupplierOrders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "SupplierOrders",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "SupplierOrders",
                type: "datetime2",
                nullable: true);

            // 2️⃣ إنشاء جدول SupplierOrderItems
            migrationBuilder.CreateTable(
                name: "SupplierOrderItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MaterialId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PricePerKg = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplierOrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupplierOrderItems_SupplierOrders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "SupplierOrders",
                        principalColumn: "OrderId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SupplierOrderItems_Materials_MaterialId",
                        column: x => x.MaterialId,
                        principalTable: "Materials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // 3️⃣ إضافة Indexes للـ Performance
            migrationBuilder.CreateIndex(
                name: "IX_SupplierOrders_PaymentStatus",
                table: "SupplierOrders",
                column: "PaymentStatus");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierOrderItems_OrderId",
                table: "SupplierOrderItems",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierOrderItems_MaterialId",
                table: "SupplierOrderItems",
                column: "MaterialId");

            // 4️⃣ (اختياري) حذف OrderDetailsJson لو مش محتاجاه
            // ⚠️ لو عندك بيانات فيه، متحذفيهوش دلوقتي!
            // migrationBuilder.DropColumn(
            //     name: "OrderDetailsJson",
            //     table: "SupplierOrders");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Rollback في حالة حصلت مشكلة
            migrationBuilder.DropTable(name: "SupplierOrderItems");

            migrationBuilder.DropIndex(
                name: "IX_SupplierOrders_PaymentStatus",
                table: "SupplierOrders");

            migrationBuilder.DropColumn(name: "PaidAt", table: "SupplierOrders");
            migrationBuilder.DropColumn(name: "CreatedAt", table: "SupplierOrders");
            migrationBuilder.DropColumn(name: "UpdatedAt", table: "SupplierOrders");

            // لو كنتي حذفتي OrderDetailsJson، استرجعيه:
            // migrationBuilder.AddColumn<string>(
            //     name: "OrderDetailsJson",
            //     table: "SupplierOrders",
            //     type: "nvarchar(max)",
            //     nullable: false);
        }
    }
}