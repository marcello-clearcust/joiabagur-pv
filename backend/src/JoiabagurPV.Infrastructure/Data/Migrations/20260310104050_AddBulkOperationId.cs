using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JoiabagurPV.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBulkOperationId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "BulkOperationId",
                table: "Sales",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sales_BulkOperationId",
                table: "Sales",
                column: "BulkOperationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Sales_BulkOperationId",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "BulkOperationId",
                table: "Sales");
        }
    }
}
