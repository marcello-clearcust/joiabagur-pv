using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JoiabagurPV.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPointOfSale : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PointOfSales",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Address = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PointOfSales", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PointOfSales_Code",
                table: "PointOfSales",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PointOfSales_IsActive",
                table: "PointOfSales",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_PointOfSales_Name",
                table: "PointOfSales",
                column: "Name");

            migrationBuilder.AddForeignKey(
                name: "FK_UserPointOfSales_PointOfSales_PointOfSaleId",
                table: "UserPointOfSales",
                column: "PointOfSaleId",
                principalTable: "PointOfSales",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserPointOfSales_PointOfSales_PointOfSaleId",
                table: "UserPointOfSales");

            migrationBuilder.DropTable(
                name: "PointOfSales");
        }
    }
}
