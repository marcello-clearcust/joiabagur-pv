using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JoiabagurPV.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddComponentManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ComponentTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComponentTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductComponents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Description = table.Column<string>(type: "character varying(35)", maxLength: 35, nullable: false),
                    CostPrice = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    SalePrice = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductComponents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ComponentTemplateItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    ComponentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComponentTemplateItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComponentTemplateItems_ComponentTemplates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "ComponentTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ComponentTemplateItems_ProductComponents_ComponentId",
                        column: x => x.ComponentId,
                        principalTable: "ProductComponents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductComponentAssignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    ComponentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    CostPrice = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    SalePrice = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductComponentAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductComponentAssignments_ProductComponents_ComponentId",
                        column: x => x.ComponentId,
                        principalTable: "ProductComponents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductComponentAssignments_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ComponentTemplateItems_ComponentId",
                table: "ComponentTemplateItems",
                column: "ComponentId");

            migrationBuilder.CreateIndex(
                name: "IX_ComponentTemplateItems_TemplateId",
                table: "ComponentTemplateItems",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_ComponentTemplateItems_TemplateId_ComponentId",
                table: "ComponentTemplateItems",
                columns: new[] { "TemplateId", "ComponentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ComponentTemplates_Name",
                table: "ComponentTemplates",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_ProductComponentAssignments_ComponentId",
                table: "ProductComponentAssignments",
                column: "ComponentId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductComponentAssignments_ProductId",
                table: "ProductComponentAssignments",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductComponentAssignments_ProductId_ComponentId",
                table: "ProductComponentAssignments",
                columns: new[] { "ProductId", "ComponentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductComponents_Description",
                table: "ProductComponents",
                column: "Description",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductComponents_IsActive",
                table: "ProductComponents",
                column: "IsActive");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ComponentTemplateItems");

            migrationBuilder.DropTable(
                name: "ProductComponentAssignments");

            migrationBuilder.DropTable(
                name: "ComponentTemplates");

            migrationBuilder.DropTable(
                name: "ProductComponents");
        }
    }
}
