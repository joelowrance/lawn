using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LawnCare.Communications.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "email_records",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    from = table.Column<string>(type: "text", nullable: false),
                    to = table.Column<string>(type: "text", nullable: false),
                    cc = table.Column<List<string>>(type: "text[]", nullable: false),
                    bcc = table.Column<List<string>>(type: "text[]", nullable: false),
                    body = table.Column<string>(type: "text", nullable: false),
                    subject = table.Column<string>(type: "text", nullable: false),
                    date_created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    date_sent = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    failure_reason = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_email_records", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "email_records");
        }
    }
}
