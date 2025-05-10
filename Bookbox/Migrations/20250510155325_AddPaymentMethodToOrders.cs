// filepath: Bookbox/Migrations/[timestamp]_AddPaymentMethodToOrders.cs
using Microsoft.EntityFrameworkCore.Migrations;
using Bookbox.Constants;

namespace Bookbox.Migrations
{
    public partial class AddPaymentMethodToOrders : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Check if column exists, if not add it
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM information_schema.columns 
                        WHERE table_name = 'Orders' AND column_name = 'PaymentMethod'
                    ) THEN
                        ALTER TABLE ""Orders"" ADD COLUMN ""PaymentMethod"" integer NOT NULL DEFAULT 1;
                    END IF;
                END $$;
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                table: "Orders");
        }
    }
}