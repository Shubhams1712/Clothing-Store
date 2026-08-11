using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentIdempotencyAndWebhookSecret : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Shipments_FulfillmentOrders_FulfillmentOrderId",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "LastSyncedAt",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "ShippingStatus",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "MaxRetries",
                table: "FulfillmentOrders");

            migrationBuilder.DropColumn(
                name: "RetryCount",
                table: "FulfillmentOrders");

            migrationBuilder.RenameColumn(
                name: "SupportsSandbox",
                table: "FulfillmentProviders",
                newName: "IsEnabled");

            migrationBuilder.RenameColumn(
                name: "LastSyncedAt",
                table: "FulfillmentOrders",
                newName: "CompletedAt");

            migrationBuilder.AddColumn<string>(
                name: "RazorpayWebhookSecret",
                table: "StoreSettings",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "FulfillmentOrderId",
                table: "Shipments",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_PaymentId",
                table: "Orders",
                column: "PaymentId",
                unique: true,
                filter: "\"PaymentId\" IS NOT NULL AND \"PaymentMethod\" = 'Razorpay'");

            migrationBuilder.AddForeignKey(
                name: "FK_Shipments_FulfillmentOrders_FulfillmentOrderId",
                table: "Shipments",
                column: "FulfillmentOrderId",
                principalTable: "FulfillmentOrders",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Shipments_FulfillmentOrders_FulfillmentOrderId",
                table: "Shipments");

            migrationBuilder.DropIndex(
                name: "IX_Orders_PaymentId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "RazorpayWebhookSecret",
                table: "StoreSettings");

            migrationBuilder.RenameColumn(
                name: "IsEnabled",
                table: "FulfillmentProviders",
                newName: "SupportsSandbox");

            migrationBuilder.RenameColumn(
                name: "CompletedAt",
                table: "FulfillmentOrders",
                newName: "LastSyncedAt");

            migrationBuilder.AlterColumn<Guid>(
                name: "FulfillmentOrderId",
                table: "Shipments",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastSyncedAt",
                table: "Shipments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ShippingStatus",
                table: "Shipments",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MaxRetries",
                table: "FulfillmentOrders",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RetryCount",
                table: "FulfillmentOrders",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_Shipments_FulfillmentOrders_FulfillmentOrderId",
                table: "Shipments",
                column: "FulfillmentOrderId",
                principalTable: "FulfillmentOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
