using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LawnCare.CoreApi.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.CreateTable(
                name: "Jobs",
                schema: "public",
                columns: table => new
                {
                    JobId = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    priority = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    requested_service_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    JobCostAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    JobCostCurrency = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false, defaultValue: "USD")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_jobs", x => x.JobId);
                });

            migrationBuilder.CreateTable(
                name: "JobLineItems",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    service_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false, comment: "The name of the service"),
                    quantity = table.Column<decimal>(type: "numeric", nullable: false, defaultValue: 1m, comment: "The quantity of the service"),
                    comment = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true, comment: "Any additional comments"),
                    is_complete = table.Column<bool>(type: "boolean", nullable: false),
                    job_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, comment: "The price of the service"),
                    price_currency = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false, defaultValue: "USD", comment: "The currency of the price")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_job_line_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_JobLineItems_Job",
                        column: x => x.job_id,
                        principalSchema: "public",
                        principalTable: "Jobs",
                        principalColumn: "JobId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "JobNotes",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    note = table.Column<string>(type: "text", maxLength: -1, nullable: false, comment: "The note"),
                    job_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_job_notes", x => x.id);
                    table.ForeignKey(
                        name: "FK_JobNotes_Job",
                        column: x => x.job_id,
                        principalSchema: "public",
                        principalTable: "Jobs",
                        principalColumn: "JobId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_JobServiceItems_JobId",
                schema: "public",
                table: "JobLineItems",
                column: "job_id");

            migrationBuilder.CreateIndex(
                name: "IX_JobNotes_JobId",
                schema: "public",
                table: "JobNotes",
                column: "job_id");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_Priority",
                schema: "public",
                table: "Jobs",
                column: "priority");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_Status",
                schema: "public",
                table: "Jobs",
                column: "status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JobLineItems",
                schema: "public");

            migrationBuilder.DropTable(
                name: "JobNotes",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Jobs",
                schema: "public");
        }
    }
}
