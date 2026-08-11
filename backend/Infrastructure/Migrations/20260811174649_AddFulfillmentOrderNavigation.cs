using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFulfillmentOrderNavigation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FulfillmentOrders_OrderId",
                table: "FulfillmentOrders");

            migrationBuilder.CreateIndex(
                name: "IX_FulfillmentOrders_OrderId",
                table: "FulfillmentOrders",
                column: "OrderId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FulfillmentOrders_OrderId",
                table: "FulfillmentOrders");

            migrationBuilder.CreateIndex(
                name: "IX_FulfillmentOrders_OrderId",
                table: "FulfillmentOrders",
                column: "OrderId");
        }
    }
}
