using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingFulfillmentTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // FulfillmentProviders
            migrationBuilder.Sql(@"
                DO $$ BEGIN
                    IF NOT EXISTS (SELECT 1 FROM pg_class WHERE relname = 'FulfillmentProviders') THEN
                        CREATE TABLE ""FulfillmentProviders"" (
                            ""Id"" uuid NOT NULL,
                            ""Name"" character varying(100) NOT NULL,
                            ""Code"" character varying(50),
                            ""ApiBaseUrl"" character varying(500),
                            ""IsEnabled"" boolean NOT NULL,
                            ""CreatedAt"" timestamp with time zone NOT NULL,
                            ""UpdatedAt"" timestamp with time zone NOT NULL,
                            ""IsActive"" boolean NOT NULL,
                            CONSTRAINT ""PK_FulfillmentProviders"" PRIMARY KEY (""Id"")
                        );
                    END IF;
                END $$;");

            // ProductFulfillmentMappings
            migrationBuilder.Sql(@"
                DO $$ BEGIN
                    IF NOT EXISTS (SELECT 1 FROM pg_class WHERE relname = 'ProductFulfillmentMappings') THEN
                        CREATE TABLE ""ProductFulfillmentMappings"" (
                            ""Id"" uuid NOT NULL,
                            ""ProductId"" uuid NOT NULL,
                            ""ProductVariantId"" uuid,
                            ""ProviderId"" uuid NOT NULL,
                            ""ExternalProductId"" character varying(100) NOT NULL,
                            ""ExternalVariantId"" character varying(100),
                            ""ExternalSku"" character varying(100) NOT NULL,
                            ""DesignReference"" character varying(200),
                            ""DesignFileUrl"" character varying(1000),
                            ""PrintingType"" character varying(50),
                            ""PrintingPlacement"" character varying(50),
                            ""CreatedAt"" timestamp with time zone NOT NULL,
                            ""UpdatedAt"" timestamp with time zone NOT NULL,
                            ""IsActive"" boolean NOT NULL,
                            CONSTRAINT ""PK_ProductFulfillmentMappings"" PRIMARY KEY (""Id""),
                            CONSTRAINT ""FK_ProductFulfillmentMappings_Products_ProductId"" FOREIGN KEY (""ProductId"") REFERENCES ""Products"" (""Id"") ON DELETE CASCADE,
                            CONSTRAINT ""FK_ProductFulfillmentMappings_ProductVariants_ProductVariantId"" FOREIGN KEY (""ProductVariantId"") REFERENCES ""ProductVariants"" (""Id""),
                            CONSTRAINT ""FK_ProductFulfillmentMappings_FulfillmentProviders_ProviderId"" FOREIGN KEY (""ProviderId"") REFERENCES ""FulfillmentProviders"" (""Id"") ON DELETE CASCADE
                        );
                    END IF;
                END $$;");

            // FulfillmentOrders
            migrationBuilder.Sql(@"
                DO $$ BEGIN
                    IF NOT EXISTS (SELECT 1 FROM pg_class WHERE relname = 'FulfillmentOrders') THEN
                        CREATE TABLE ""FulfillmentOrders"" (
                            ""Id"" uuid NOT NULL,
                            ""OrderId"" uuid NOT NULL,
                            ""ProviderId"" uuid NOT NULL,
                            ""ExternalOrderId"" character varying(100),
                            ""Status"" integer NOT NULL,
                            ""ProviderStatus"" character varying(100),
                            ""FailureReason"" character varying(2000),
                            ""ErrorCategory"" character varying(100),
                            ""SubmittedAt"" timestamp with time zone,
                            ""CompletedAt"" timestamp with time zone,
                            ""CreatedAt"" timestamp with time zone NOT NULL,
                            ""UpdatedAt"" timestamp with time zone NOT NULL,
                            ""IsActive"" boolean NOT NULL,
                            CONSTRAINT ""PK_FulfillmentOrders"" PRIMARY KEY (""Id""),
                            CONSTRAINT ""FK_FulfillmentOrders_Orders_OrderId"" FOREIGN KEY (""OrderId"") REFERENCES ""Orders"" (""Id"") ON DELETE CASCADE,
                            CONSTRAINT ""FK_FulfillmentOrders_FulfillmentProviders_ProviderId"" FOREIGN KEY (""ProviderId"") REFERENCES ""FulfillmentProviders"" (""Id"") ON DELETE CASCADE
                        );
                    END IF;
                END $$;");

            // FulfillmentOrderItems
            migrationBuilder.Sql(@"
                DO $$ BEGIN
                    IF NOT EXISTS (SELECT 1 FROM pg_class WHERE relname = 'FulfillmentOrderItems') THEN
                        CREATE TABLE ""FulfillmentOrderItems"" (
                            ""Id"" uuid NOT NULL,
                            ""FulfillmentOrderId"" uuid NOT NULL,
                            ""OrderItemId"" uuid NOT NULL,
                            ""ExternalProductId"" character varying(100) NOT NULL,
                            ""ExternalVariantId"" character varying(100),
                            ""ExternalSku"" character varying(100) NOT NULL,
                            ""Quantity"" integer NOT NULL,
                            ""Status"" character varying(100),
                            ""FailureReason"" character varying(1000),
                            ""DesignReference"" character varying(200),
                            ""DesignFileUrl"" character varying(1000),
                            ""MockupUrl"" character varying(1000),
                            ""CreatedAt"" timestamp with time zone NOT NULL,
                            ""UpdatedAt"" timestamp with time zone NOT NULL,
                            ""IsActive"" boolean NOT NULL,
                            CONSTRAINT ""PK_FulfillmentOrderItems"" PRIMARY KEY (""Id""),
                            CONSTRAINT ""FK_FulfillmentOrderItems_FulfillmentOrders_FulfillmentOrderId"" FOREIGN KEY (""FulfillmentOrderId"") REFERENCES ""FulfillmentOrders"" (""Id"") ON DELETE CASCADE,
                            CONSTRAINT ""FK_FulfillmentOrderItems_OrderItems_OrderItemId"" FOREIGN KEY (""OrderItemId"") REFERENCES ""OrderItems"" (""Id"") ON DELETE CASCADE
                        );
                    END IF;
                END $$;");

            // Shipments
            migrationBuilder.Sql(@"
                DO $$ BEGIN
                    IF NOT EXISTS (SELECT 1 FROM pg_class WHERE relname = 'Shipments') THEN
                        CREATE TABLE ""Shipments"" (
                            ""Id"" uuid NOT NULL,
                            ""OrderId"" uuid NOT NULL,
                            ""FulfillmentOrderId"" uuid,
                            ""TrackingNumber"" character varying(100),
                            ""CourierName"" character varying(100),
                            ""TrackingUrl"" character varying(1000),
                            ""ProviderShippingStatus"" character varying(100),
                            ""ShippedAt"" timestamp with time zone,
                            ""DeliveredAt"" timestamp with time zone,
                            ""CreatedAt"" timestamp with time zone NOT NULL,
                            ""UpdatedAt"" timestamp with time zone NOT NULL,
                            ""IsActive"" boolean NOT NULL,
                            CONSTRAINT ""PK_Shipments"" PRIMARY KEY (""Id""),
                            CONSTRAINT ""FK_Shipments_Orders_OrderId"" FOREIGN KEY (""OrderId"") REFERENCES ""Orders"" (""Id"") ON DELETE CASCADE,
                            CONSTRAINT ""FK_Shipments_FulfillmentOrders_FulfillmentOrderId"" FOREIGN KEY (""FulfillmentOrderId"") REFERENCES ""FulfillmentOrders"" (""Id"")
                        );
                    END IF;
                END $$;");

            // Indexes for ProductFulfillmentMappings
            migrationBuilder.Sql(@"
                DO $$ BEGIN
                    IF NOT EXISTS (SELECT 1 FROM pg_class WHERE relname = 'IX_ProductFulfillmentMappings_ProductVariantId') THEN
                        CREATE INDEX ""IX_ProductFulfillmentMappings_ProductVariantId"" ON ""ProductFulfillmentMappings"" (""ProductVariantId"");
                    END IF;
                END $$;");

            migrationBuilder.Sql(@"
                DO $$ BEGIN
                    IF NOT EXISTS (SELECT 1 FROM pg_class WHERE relname = 'IX_ProductFulfillmentMappings_ProviderId') THEN
                        CREATE INDEX ""IX_ProductFulfillmentMappings_ProviderId"" ON ""ProductFulfillmentMappings"" (""ProviderId"");
                    END IF;
                END $$;");

            migrationBuilder.Sql(@"
                DO $$ BEGIN
                    IF NOT EXISTS (SELECT 1 FROM pg_class WHERE relname = 'IX_ProductFulfillmentMappings_ProductId_ProductVariantId_ProviderId') THEN
                        CREATE UNIQUE INDEX ""IX_ProductFulfillmentMappings_ProductId_ProductVariantId_ProviderId"" ON ""ProductFulfillmentMappings"" (""ProductId"", ""ProductVariantId"", ""ProviderId"");
                    END IF;
                END $$;");

            // Indexes for FulfillmentOrders
            migrationBuilder.Sql(@"
                DO $$ BEGIN
                    IF NOT EXISTS (SELECT 1 FROM pg_class WHERE relname = 'IX_FulfillmentOrders_ExternalOrderId') THEN
                        CREATE INDEX ""IX_FulfillmentOrders_ExternalOrderId"" ON ""FulfillmentOrders"" (""ExternalOrderId"");
                    END IF;
                END $$;");

            migrationBuilder.Sql(@"
                DO $$ BEGIN
                    IF NOT EXISTS (SELECT 1 FROM pg_class WHERE relname = 'IX_FulfillmentOrders_OrderId') THEN
                        CREATE UNIQUE INDEX ""IX_FulfillmentOrders_OrderId"" ON ""FulfillmentOrders"" (""OrderId"");
                    END IF;
                END $$;");

            migrationBuilder.Sql(@"
                DO $$ BEGIN
                    IF NOT EXISTS (SELECT 1 FROM pg_class WHERE relname = 'IX_FulfillmentOrders_ProviderId') THEN
                        CREATE INDEX ""IX_FulfillmentOrders_ProviderId"" ON ""FulfillmentOrders"" (""ProviderId"");
                    END IF;
                END $$;");

            migrationBuilder.Sql(@"
                DO $$ BEGIN
                    IF NOT EXISTS (SELECT 1 FROM pg_class WHERE relname = 'IX_FulfillmentOrders_Status') THEN
                        CREATE INDEX ""IX_FulfillmentOrders_Status"" ON ""FulfillmentOrders"" (""Status"");
                    END IF;
                END $$;");

            // Indexes for FulfillmentOrderItems
            migrationBuilder.Sql(@"
                DO $$ BEGIN
                    IF NOT EXISTS (SELECT 1 FROM pg_class WHERE relname = 'IX_FulfillmentOrderItems_FulfillmentOrderId') THEN
                        CREATE INDEX ""IX_FulfillmentOrderItems_FulfillmentOrderId"" ON ""FulfillmentOrderItems"" (""FulfillmentOrderId"");
                    END IF;
                END $$;");

            migrationBuilder.Sql(@"
                DO $$ BEGIN
                    IF NOT EXISTS (SELECT 1 FROM pg_class WHERE relname = 'IX_FulfillmentOrderItems_OrderItemId') THEN
                        CREATE INDEX ""IX_FulfillmentOrderItems_OrderItemId"" ON ""FulfillmentOrderItems"" (""OrderItemId"");
                    END IF;
                END $$;");

            // Indexes for Shipments
            migrationBuilder.Sql(@"
                DO $$ BEGIN
                    IF NOT EXISTS (SELECT 1 FROM pg_class WHERE relname = 'IX_Shipments_FulfillmentOrderId') THEN
                        CREATE UNIQUE INDEX ""IX_Shipments_FulfillmentOrderId"" ON ""Shipments"" (""FulfillmentOrderId"");
                    END IF;
                END $$;");

            migrationBuilder.Sql(@"
                DO $$ BEGIN
                    IF NOT EXISTS (SELECT 1 FROM pg_class WHERE relname = 'IX_Shipments_OrderId') THEN
                        CREATE INDEX ""IX_Shipments_OrderId"" ON ""Shipments"" (""OrderId"");
                    END IF;
                END $$;");

            // RazorpayWebhookSecret on StoreSettings (idempotent)
            migrationBuilder.Sql(@"
                DO $$ BEGIN
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'StoreSettings' AND column_name = 'RazorpayWebhookSecret') THEN
                        ALTER TABLE ""StoreSettings"" ADD ""RazorpayWebhookSecret"" character varying(500);
                    END IF;
                END $$;");

            // PaymentId index on Orders (idempotent, partial for Razorpay)
            migrationBuilder.Sql(@"
                DO $$ BEGIN
                    IF NOT EXISTS (SELECT 1 FROM pg_class WHERE relname = 'IX_Orders_PaymentId') THEN
                        CREATE UNIQUE INDEX ""IX_Orders_PaymentId"" ON ""Orders"" (""PaymentId"") WHERE ""PaymentId"" IS NOT NULL AND ""PaymentMethod"" = 'Razorpay';
                    END IF;
                END $$;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""IX_Orders_PaymentId"";");
            migrationBuilder.Sql(@"ALTER TABLE ""StoreSettings"" DROP COLUMN IF EXISTS ""RazorpayWebhookSecret"";");
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""IX_Shipments_OrderId"";");
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""IX_Shipments_FulfillmentOrderId"";");
            migrationBuilder.Sql(@"DROP TABLE IF EXISTS ""Shipments"";");
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""IX_FulfillmentOrderItems_OrderItemId"";");
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""IX_FulfillmentOrderItems_FulfillmentOrderId"";");
            migrationBuilder.Sql(@"DROP TABLE IF EXISTS ""FulfillmentOrderItems"";");
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""IX_FulfillmentOrders_Status"";");
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""IX_FulfillmentOrders_ProviderId"";");
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""IX_FulfillmentOrders_OrderId"";");
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""IX_FulfillmentOrders_ExternalOrderId"";");
            migrationBuilder.Sql(@"DROP TABLE IF EXISTS ""FulfillmentOrders"";");
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""IX_ProductFulfillmentMappings_ProductId_ProductVariantId_ProviderId"";");
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""IX_ProductFulfillmentMappings_ProviderId"";");
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""IX_ProductFulfillmentMappings_ProductVariantId"";");
            migrationBuilder.Sql(@"DROP TABLE IF EXISTS ""ProductFulfillmentMappings"";");
            migrationBuilder.Sql(@"DROP TABLE IF EXISTS ""FulfillmentProviders"";");
        }
    }
}
