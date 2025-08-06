using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LawnCare.JobApi.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "JobService");

            migrationBuilder.CreateTable(
                name: "Jobs",
                schema: "JobService",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: true),
                    customer_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, comment: "Name of the customer for this job"),
                    service_address = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    priority = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false, comment: "Detailed description of the work to be performed"),
                    special_instructions = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true, comment: "Special instructions from customer or management"),
                    estimated_duration = table.Column<long>(type: "bigint", nullable: false),
                    estimated_cost = table.Column<string>(type: "text", nullable: false),
                    actual_cost = table.Column<string>(type: "text", nullable: true),
                    requested_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "Date requested by customer"),
                    scheduled_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, comment: "Date scheduled for technician"),
                    completed_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, comment: "Date job was completed"),
                    assigned_technician_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP", comment: "Timestamp when job was created"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP", comment: "Timestamp when job was last updated")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_jobs", x => x.id);
                    table.CheckConstraint("CK_Jobs_CompletedDate_Future", "\"completed_date\" IS NULL OR \"completed_date\" >= \"created_at\"");
                    table.CheckConstraint("CK_Jobs_ScheduledDate_Future", "\"scheduled_date\" IS NULL OR \"scheduled_date\" >= \"created_at\"");
                });

            migrationBuilder.CreateTable(
                name: "JobNotes",
                schema: "JobService",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    author = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, comment: "Author of the note (technician, customer, system, etc.)"),
                    content = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false, comment: "Content of the note"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP", comment: "Timestamp when note was created"),
                    job_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Foreign key to the Job")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_job_notes", x => x.id);
                    table.ForeignKey(
                        name: "FK_JobNotes_Job",
                        column: x => x.job_id,
                        principalSchema: "JobService",
                        principalTable: "Jobs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "JobServiceItems",
                schema: "JobService",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    service_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, comment: "Name of the service provided"),
                    quantity = table.Column<int>(type: "integer", nullable: false, defaultValue: 1, comment: "Quantity of service provided"),
                    comment = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true, comment: "Optional comment about the service"),
                    price = table.Column<decimal>(type: "numeric(7,2)", precision: 7, scale: 2, nullable: false, comment: "Price per unit of service"),
                    is_fulfilled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false, comment: "Whether this service has been fulfilled"),
                    job_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Foreign key to the Job")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_job_service_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_JobRequirements_Job",
                        column: x => x.job_id,
                        principalSchema: "JobService",
                        principalTable: "Jobs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_JobNotes_Author",
                schema: "JobService",
                table: "JobNotes",
                column: "author");

            migrationBuilder.CreateIndex(
                name: "IX_JobNotes_CreatedAt",
                schema: "JobService",
                table: "JobNotes",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_JobNotes_JobId",
                schema: "JobService",
                table: "JobNotes",
                column: "job_id");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_AssignedTechnicianId",
                schema: "JobService",
                table: "Jobs",
                column: "assigned_technician_id");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_CreatedAt",
                schema: "JobService",
                table: "Jobs",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_CustomerId",
                schema: "JobService",
                table: "Jobs",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_Priority",
                schema: "JobService",
                table: "Jobs",
                column: "priority");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_RequestedDate",
                schema: "JobService",
                table: "Jobs",
                column: "requested_date");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_ScheduledDate",
                schema: "JobService",
                table: "Jobs",
                column: "scheduled_date");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_Status",
                schema: "JobService",
                table: "Jobs",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_TenantId",
                schema: "JobService",
                table: "Jobs",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_TenantId_CustomerId",
                schema: "JobService",
                table: "Jobs",
                columns: new[] { "tenant_id", "customer_id" });

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_TenantId_Status",
                schema: "JobService",
                table: "Jobs",
                columns: new[] { "tenant_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_TenantId_TechnicianId_ScheduledDate",
                schema: "JobService",
                table: "Jobs",
                columns: new[] { "tenant_id", "assigned_technician_id", "scheduled_date" });

            migrationBuilder.CreateIndex(
                name: "IX_JobServiceItems_IsFulfilled",
                schema: "JobService",
                table: "JobServiceItems",
                column: "is_fulfilled");

            migrationBuilder.CreateIndex(
                name: "IX_JobServiceItems_JobId",
                schema: "JobService",
                table: "JobServiceItems",
                column: "job_id");

            migrationBuilder.CreateIndex(
                name: "IX_JobServiceItems_ServiceName",
                schema: "JobService",
                table: "JobServiceItems",
                column: "service_name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JobNotes",
                schema: "JobService");

            migrationBuilder.DropTable(
                name: "JobServiceItems",
                schema: "JobService");

            migrationBuilder.DropTable(
                name: "Jobs",
                schema: "JobService");
        }
    }
}
