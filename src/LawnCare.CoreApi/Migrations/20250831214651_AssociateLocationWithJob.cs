using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LawnCare.CoreApi.Migrations
{
    /// <inheritdoc />
    public partial class AssociateLocationWithJob : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "location_id",
                schema: "public",
                table: "Jobs",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_LocationId",
                schema: "public",
                table: "Jobs",
                column: "location_id");

            migrationBuilder.AddForeignKey(
                name: "fk_jobs_locations_location_id",
                schema: "public",
                table: "Jobs",
                column: "location_id",
                principalTable: "Locations",
                principalColumn: "location_id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_jobs_locations_location_id",
                schema: "public",
                table: "Jobs");

            migrationBuilder.DropIndex(
                name: "IX_Jobs_LocationId",
                schema: "public",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "location_id",
                schema: "public",
                table: "Jobs");
        }
    }
}
