using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LawnCare.StateMachine.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "estimate_processing_states",
                columns: table => new
                {
                    correlation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    current_state = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    estimate_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: true),
                    job_id = table.Column<Guid>(type: "uuid", nullable: true),
                    customer_info = table.Column<string>(type: "jsonb", nullable: false),
                    job_details = table.Column<string>(type: "jsonb", nullable: false),
                    estimator_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    is_new_customer = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    error_reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    welcome_email_error = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    version = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_estimate_processing_states", x => x.correlation_id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_estimate_processing_states_created_at",
                table: "estimate_processing_states",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_estimate_processing_states_current_state",
                table: "estimate_processing_states",
                column: "current_state");

            migrationBuilder.CreateIndex(
                name: "ix_estimate_processing_states_estimate_id",
                table: "estimate_processing_states",
                column: "estimate_id");

            migrationBuilder.CreateIndex(
                name: "ix_estimate_processing_states_tenant_id",
                table: "estimate_processing_states",
                column: "tenant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "estimate_processing_states");
        }
    }
}
