using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingProductColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add missing columns to Products table that were added to the EF model
            // but never included in the actual migration Up() method.
            // Uses ADD COLUMN IF NOT EXISTS for idempotency.

            migrationBuilder.Sql(@"
                ALTER TABLE ""Products""
                ADD COLUMN IF NOT EXISTS ""IsQikinkProduct"" boolean NOT NULL DEFAULT false;
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE ""Products""
                ADD COLUMN IF NOT EXISTS ""QikinkProductId"" character varying(100);
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE ""Products""
                ADD COLUMN IF NOT EXISTS ""QikinkProductName"" character varying(200);
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE ""Products""
                ADD COLUMN IF NOT EXISTS ""DesignReference"" character varying(200);
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE ""Products""
                ADD COLUMN IF NOT EXISTS ""DesignFileUrl"" character varying(1000);
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE ""Products""
                ADD COLUMN IF NOT EXISTS ""MockupUrl"" character varying(1000);
            ");

            // Add missing QikinkSku column to ProductVariants table
            migrationBuilder.Sql(@"
                ALTER TABLE ""ProductVariants""
                ADD COLUMN IF NOT EXISTS ""QikinkSku"" character varying(100);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove columns (safe only if no data dependency)
            migrationBuilder.Sql(@"ALTER TABLE ""Products"" DROP COLUMN IF EXISTS ""IsQikinkProduct"";");
            migrationBuilder.Sql(@"ALTER TABLE ""Products"" DROP COLUMN IF EXISTS ""QikinkProductId"";");
            migrationBuilder.Sql(@"ALTER TABLE ""Products"" DROP COLUMN IF EXISTS ""QikinkProductName"";");
            migrationBuilder.Sql(@"ALTER TABLE ""Products"" DROP COLUMN IF EXISTS ""DesignReference"";");
            migrationBuilder.Sql(@"ALTER TABLE ""Products"" DROP COLUMN IF EXISTS ""DesignFileUrl"";");
            migrationBuilder.Sql(@"ALTER TABLE ""Products"" DROP COLUMN IF EXISTS ""MockupUrl"";");
            migrationBuilder.Sql(@"ALTER TABLE ""ProductVariants"" DROP COLUMN IF EXISTS ""QikinkSku"";");
        }
    }
}
