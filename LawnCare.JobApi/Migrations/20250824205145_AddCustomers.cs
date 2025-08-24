using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LawnCare.JobApi.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "service_address",
                schema: "JobService",
                table: "Jobs");

            migrationBuilder.AddColumn<Guid>(
                name: "actual_service_address_id",
                schema: "JobService",
                table: "Jobs",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "Address",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    street1 = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    street2 = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    street3 = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    city = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    state = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    zip_code = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_address", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Customers",
                schema: "JobService",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "Customer's first name"),
                    last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "Customer's last name"),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false, comment: "Customer's email address"),
                    home_phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true, comment: "Customer's home phone number"),
                    cell_phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true, comment: "Customer's cell phone number"),
                    address_id = table.Column<Guid>(type: "uuid", nullable: false),
                    customer_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "Residential"),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "Active"),
                    notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true, comment: "Additional notes about the customer"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP", comment: "Timestamp when customer was created"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP", comment: "Timestamp when customer was last updated"),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "User who created the customer"),
                    updated_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "User who last updated the customer"),
                    id1 = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_customers", x => x.id);
                    table.ForeignKey(
                        name: "fk_customers_address_address_id",
                        column: x => x.address_id,
                        principalTable: "Address",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "ix_jobs_actual_service_address_id",
                schema: "JobService",
                table: "Jobs",
                column: "actual_service_address_id");

            migrationBuilder.CreateIndex(
                name: "ix_customers_address_id",
                schema: "JobService",
                table: "Customers",
                column: "address_id");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_Email",
                schema: "JobService",
                table: "Customers",
                column: "email");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_Status",
                schema: "JobService",
                table: "Customers",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_TenantId",
                schema: "JobService",
                table: "Customers",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_TenantId_Email",
                schema: "JobService",
                table: "Customers",
                columns: new[] { "tenant_id", "email" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Customers_TenantId_Name",
                schema: "JobService",
                table: "Customers",
                columns: new[] { "tenant_id", "last_name", "first_name" });

            migrationBuilder.AddForeignKey(
                name: "fk_jobs_address_actual_service_address_id",
                schema: "JobService",
                table: "Jobs",
                column: "actual_service_address_id",
                principalTable: "Address",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_jobs_address_actual_service_address_id",
                schema: "JobService",
                table: "Jobs");

            migrationBuilder.DropTable(
                name: "Customers",
                schema: "JobService");

            migrationBuilder.DropTable(
                name: "Address");

            migrationBuilder.DropIndex(
                name: "ix_jobs_actual_service_address_id",
                schema: "JobService",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "actual_service_address_id",
                schema: "JobService",
                table: "Jobs");

            migrationBuilder.AddColumn<string>(
                name: "service_address",
                schema: "JobService",
                table: "Jobs",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
