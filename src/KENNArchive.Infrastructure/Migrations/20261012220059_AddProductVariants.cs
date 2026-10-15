using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KENNArchive.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProductVariants : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Sifarişdə seçilmiş ölçünü saxlamaq üçün sütun əlavə edilir.
            migrationBuilder.AddColumn<string>(
                name: "Size",
                table: "OrderItems",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            // Səbət məhsulunun konkret ölçüsünü göstərir.
            // Nullable olduğuna görə köhnə səbət qeydləri qorunur.
            migrationBuilder.AddColumn<int>(
                name: "ProductVariantId",
                table: "CartItems",
                type: "int",
                nullable: true);

            // Məhsul ölçüləri və onların stokları üçün cədvəl.
            migrationBuilder.CreateTable(
                name: "ProductVariants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),

                    ProductId = table.Column<int>(
                        type: "int",
                        nullable: false),

                    Size = table.Column<string>(
                        type: "nvarchar(10)",
                        maxLength: 10,
                        nullable: false),

                    Stock = table.Column<int>(
                        type: "int",
                        nullable: false),

                    CreatedAt = table.Column<DateTime>(
                        type: "datetime2",
                        nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_ProductVariants",
                        x => x.Id);

                    table.ForeignKey(
                        name: "FK_ProductVariants_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Hər mövcud məhsul üçün altı ölçü yaradırıq.
            // Köhnə ümumi stok M ölçüsünə köçürülür.
            // Digər ölçülər sıfır stokla başlayır.
            migrationBuilder.Sql(@"
                INSERT INTO ProductVariants
                    (ProductId, Size, Stock, CreatedAt)
                SELECT
                    p.Id,
                    s.Size,
                    CASE
                        WHEN s.Size = 'M' THEN p.Stock
                        ELSE 0
                    END,
                    SYSUTCDATETIME()
                FROM Products AS p
                CROSS JOIN
                (
                    SELECT 'XS' AS Size
                    UNION ALL SELECT 'S'
                    UNION ALL SELECT 'M'
                    UNION ALL SELECT 'L'
                    UNION ALL SELECT 'XL'
                    UNION ALL SELECT 'XXL'
                ) AS s;
            ");

            // Səbət variantı üzrə axtarışı sürətləndirir.
            migrationBuilder.CreateIndex(
                name: "IX_CartItems_ProductVariantId",
                table: "CartItems",
                column: "ProductVariantId");

            // Eyni məhsul üçün eyni ölçünün təkrar yaradılmasının qarşısını alır.
            migrationBuilder.CreateIndex(
                name: "IX_ProductVariants_ProductId_Size",
                table: "ProductVariants",
                columns: new[] { "ProductId", "Size" },
                unique: true);

            // Səbət variantını ProductVariants cədvəlinə bağlayır.
            migrationBuilder.AddForeignKey(
                name: "FK_CartItems_ProductVariants_ProductVariantId",
                table: "CartItems",
                column: "ProductVariantId",
                principalTable: "ProductVariants",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CartItems_ProductVariants_ProductVariantId",
                table: "CartItems");

            migrationBuilder.DropTable(
                name: "ProductVariants");

            migrationBuilder.DropIndex(
                name: "IX_CartItems_ProductVariantId",
                table: "CartItems");

            migrationBuilder.DropColumn(
                name: "Size",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "ProductVariantId",
                table: "CartItems");
        }
    }
}