using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JoiabagurPV.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProductPhotoEmbeddings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProductPhotoEmbeddings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductPhotoId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductSku = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EmbeddingVector = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductPhotoEmbeddings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductPhotoEmbeddings_ProductPhotos_ProductPhotoId",
                        column: x => x.ProductPhotoId,
                        principalTable: "ProductPhotos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductPhotoEmbeddings_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductPhotoEmbeddings_ProductId",
                table: "ProductPhotoEmbeddings",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductPhotoEmbeddings_ProductPhotoId",
                table: "ProductPhotoEmbeddings",
                column: "ProductPhotoId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductPhotoEmbeddings");
        }
    }
}
