using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bookbox.Migrations
{
    public partial class AddOrderInfoToReviews : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // First, check if columns already exist to avoid errors
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    BEGIN
                        IF NOT EXISTS (SELECT 1 FROM information_schema.columns 
                                      WHERE table_name='Reviews' AND column_name='OrderId') THEN
                            ALTER TABLE ""Reviews"" ADD ""OrderId"" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
                        END IF;
                    EXCEPTION
                        WHEN duplicate_column THEN
                    END;
                    
                    BEGIN
                        IF NOT EXISTS (SELECT 1 FROM information_schema.columns 
                                      WHERE table_name='Reviews' AND column_name='OrderItemId') THEN
                            ALTER TABLE ""Reviews"" ADD ""OrderItemId"" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
                        END IF;
                    EXCEPTION
                        WHEN duplicate_column THEN
                    END;
                END
                $$;
            ");

            // Add foreign key constraints if they don't exist
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM pg_constraint 
                        WHERE conname = 'FK_Reviews_Orders_OrderId'
                    ) THEN
                        ALTER TABLE ""Reviews"" ADD CONSTRAINT ""FK_Reviews_Orders_OrderId""
                            FOREIGN KEY (""OrderId"") REFERENCES ""Orders""(""OrderId"")
                            ON DELETE CASCADE;
                    END IF;
                    
                    IF NOT EXISTS (
                        SELECT 1 FROM pg_constraint 
                        WHERE conname = 'FK_Reviews_OrderItems_OrderItemId'
                    ) THEN
                        ALTER TABLE ""Reviews"" ADD CONSTRAINT ""FK_Reviews_OrderItems_OrderItemId""
                            FOREIGN KEY (""OrderItemId"") REFERENCES ""OrderItems""(""OrderItemId"")
                            ON DELETE CASCADE;
                    END IF;
                END
                $$;
            ");
            
            // Create indexes if they don't exist
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM pg_indexes 
                        WHERE indexname = 'IX_Reviews_OrderId'
                    ) THEN
                        CREATE INDEX ""IX_Reviews_OrderId"" ON ""Reviews"" (""OrderId"");
                    END IF;
                    
                    IF NOT EXISTS (
                        SELECT 1 FROM pg_indexes 
                        WHERE indexname = 'IX_Reviews_OrderItemId'
                    ) THEN
                        CREATE INDEX ""IX_Reviews_OrderItemId"" ON ""Reviews"" (""OrderItemId"");
                    END IF;
                END
                $$;
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_Orders_OrderId",
                table: "Reviews");

            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_OrderItems_OrderItemId",
                table: "Reviews");

            migrationBuilder.DropIndex(
                name: "IX_Reviews_OrderId",
                table: "Reviews");
                
            migrationBuilder.DropIndex(
                name: "IX_Reviews_OrderItemId",
                table: "Reviews");
                
            migrationBuilder.DropColumn(
                name: "OrderId",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "OrderItemId",
                table: "Reviews");
        }
    }
}