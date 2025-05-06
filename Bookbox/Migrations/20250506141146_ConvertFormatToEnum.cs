using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bookbox.Migrations
{
    /// <inheritdoc />
    public partial class ConvertFormatToEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // First add a temporary column to hold the new integer values
            migrationBuilder.AddColumn<int>(
                name: "FormatInt",
                table: "Books",
                type: "integer",
                nullable: false,
                defaultValue: 1); // Default to Paperback

            // Update the temporary column with mapped values
            migrationBuilder.Sql(@"
                UPDATE ""Books"" 
                SET ""FormatInt"" = CASE 
                    WHEN LOWER(""Format"") ~ 'paperback' THEN 1
                    WHEN LOWER(""Format"") ~ 'hardcover' THEN 2
                    WHEN LOWER(""Format"") ~ 'ebook' THEN 3
                    WHEN LOWER(""Format"") ~ 'audio' THEN 4
                    WHEN LOWER(""Format"") ~ 'signed' THEN 5
                    WHEN LOWER(""Format"") ~ 'limited' THEN 6
                    WHEN LOWER(""Format"") ~ 'first' THEN 7
                    WHEN LOWER(""Format"") ~ 'collector' THEN 8
                    WHEN LOWER(""Format"") ~ 'author' THEN 9
                    WHEN LOWER(""Format"") ~ 'deluxe' THEN 10
                    WHEN LOWER(""Format"") ~ 'illustrated' THEN 11
                    WHEN LOWER(""Format"") ~ 'boxed' THEN 12
                    WHEN LOWER(""Format"") ~ 'leather' THEN 13
                    ELSE 1 END;
            ");

            // Drop the old column
            migrationBuilder.DropColumn(
                name: "Format",
                table: "Books");

            // Rename the temporary column to Format
            migrationBuilder.RenameColumn(
                name: "FormatInt",
                table: "Books",
                newName: "Format");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Add back a string column temporarily
            migrationBuilder.AddColumn<string>(
                name: "FormatString",
                table: "Books",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Paperback");

            // Convert integer values back to strings
            migrationBuilder.Sql(@"
                UPDATE ""Books""
                SET ""FormatString"" = CASE ""Format""
                    WHEN 1 THEN 'Paperback'
                    WHEN 2 THEN 'Hardcover'
                    WHEN 3 THEN 'Ebook'
                    WHEN 4 THEN 'AudioBook'
                    WHEN 5 THEN 'Signed'
                    WHEN 6 THEN 'Limited'
                    WHEN 7 THEN 'First'
                    WHEN 8 THEN 'Collectors'
                    WHEN 9 THEN 'Authors'
                    WHEN 10 THEN 'Deluxe'
                    WHEN 11 THEN 'Illustrated'
                    WHEN 12 THEN 'Boxed'
                    WHEN 13 THEN 'Leather'
                    ELSE 'Paperback' END;
            ");

            // Drop the integer column
            migrationBuilder.DropColumn(
                name: "Format",
                table: "Books");

            // Rename the string column back to Format
            migrationBuilder.RenameColumn(
                name: "FormatString",
                table: "Books",
                newName: "Format");
        }
    }
}
