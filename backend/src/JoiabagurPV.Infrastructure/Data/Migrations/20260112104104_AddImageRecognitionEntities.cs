using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JoiabagurPV.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddImageRecognitionEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ModelMetadata",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Version = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TrainedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModelPath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    AccuracyMetrics = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    TotalPhotosUsed = table.Column<int>(type: "integer", nullable: false),
                    TotalProductsUsed = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModelMetadata", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ModelTrainingJobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InitiatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ProgressPercentage = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CurrentStage = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ErrorMessage = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ResultModelVersion = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DurationSeconds = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModelTrainingJobs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ModelTrainingJobs_Users_InitiatedBy",
                        column: x => x.InitiatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ModelMetadata_IsActive",
                table: "ModelMetadata",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_ModelMetadata_Version",
                table: "ModelMetadata",
                column: "Version",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ModelTrainingJobs_CreatedAt",
                table: "ModelTrainingJobs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ModelTrainingJobs_InitiatedBy",
                table: "ModelTrainingJobs",
                column: "InitiatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ModelTrainingJobs_Status",
                table: "ModelTrainingJobs",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ModelTrainingJobs_Status_CreatedAt",
                table: "ModelTrainingJobs",
                columns: new[] { "Status", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ModelMetadata");

            migrationBuilder.DropTable(
                name: "ModelTrainingJobs");
        }
    }
}
