using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LawnCare.CoreApi.Migrations
{
    /// <inheritdoc />
    public partial class AddTechnicianTableWithStartDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Technicians",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    first_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false, comment: "The first name of the technician"),
                    last_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false, comment: "The last name of the technician"),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    cell_phone = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false, comment: "The full address of the technician"),
                    photo_url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false, comment: "URL to the technician's profile photo"),
                    start_date = table.Column<DateTime>(type: "date", nullable: false, comment: "Date when the technician started with the company"),
                    status = table.Column<int>(type: "integer", nullable: false, comment: "Current status of the technician"),
                    specialization = table.Column<int>(type: "integer", nullable: false, comment: "Primary specialization of the technician"),
                    hire_date = table.Column<DateTime>(type: "date", nullable: false, comment: "Date when the technician was hired"),
                    emergency_contact = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false, comment: "Name of emergency contact person"),
                    emergency_phone = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    license_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "Professional license number"),
                    license_expiry = table.Column<DateTime>(type: "date", nullable: false, comment: "Expiration date of the professional license"),
                    notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false, comment: "Additional notes about the technician"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "When the record was created"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "When the record was last updated")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_technicians", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_technicians_email",
                schema: "public",
                table: "Technicians",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_technicians_license_number",
                schema: "public",
                table: "Technicians",
                column: "license_number");

            migrationBuilder.CreateIndex(
                name: "ix_technicians_specialization",
                schema: "public",
                table: "Technicians",
                column: "specialization");

            migrationBuilder.CreateIndex(
                name: "ix_technicians_status",
                schema: "public",
                table: "Technicians",
                column: "status");

            // Insert dummy technician data
            migrationBuilder.InsertData(
                schema: "public",
                table: "Technicians",
                columns: new[] { "id", "first_name", "last_name", "email", "cell_phone", "address", "photo_url", "start_date", "status", "specialization", "hire_date", "emergency_contact", "emergency_phone", "license_number", "license_expiry", "notes", "created_at", "updated_at" },
                values: new object[,]
                {
                    {
                        new Guid("11111111-1111-1111-1111-111111111111"),
                        "Robert",
                        "Williams",
                        "robert.williams@lawncare.com",
                        "(318) 924-7232",
                        "789 Maple Drive, Rockford, IL 61101",
                        "https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=150&h=150&fit=crop&crop=face",
                        new DateTime(2024, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc),
                        1,
                        1,
                        new DateTime(2024, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc),
                        "Sarah Williams",
                        "(318) 924-7233",
                        "LC838957",
                        new DateTime(2025, 12, 31, 0, 0, 0, 0, DateTimeKind.Utc),
                        "Experienced in residential lawn care and maintenance. Excellent customer service skills.",
                        new DateTimeOffset(2024, 1, 15, 8, 0, 0, TimeSpan.Zero),
                        new DateTimeOffset(2024, 1, 15, 8, 0, 0, TimeSpan.Zero)
                    },
                    {
                        new Guid("22222222-2222-2222-2222-222222222222"),
                        "Maria",
                        "Garcia",
                        "maria.garcia@lawncare.com",
                        "(555) 123-4567",
                        "456 Oak Street, Springfield, IL 62701",
                        "https://images.unsplash.com/photo-1494790108755-2616b612b786?w=150&h=150&fit=crop&crop=face",
                        new DateTime(2022, 3, 10, 0, 0, 0, 0, DateTimeKind.Utc),
                        1,
                        2,
                        new DateTime(2022, 3, 10, 0, 0, 0, 0, DateTimeKind.Utc),
                        "Carlos Garcia",
                        "(555) 123-4568",
                        "TC445123",
                        new DateTime(2025, 6, 30, 0, 0, 0, 0, DateTimeKind.Utc),
                        "Specializes in tree trimming and removal. Certified arborist with 5+ years experience.",
                        new DateTimeOffset(2022, 3, 10, 8, 0, 0, TimeSpan.Zero),
                        new DateTimeOffset(2024, 1, 15, 8, 0, 0, TimeSpan.Zero)
                    },
                    {
                        new Guid("33333333-3333-3333-3333-333333333333"),
                        "James",
                        "Johnson",
                        "james.johnson@lawncare.com",
                        "(312) 555-7890",
                        "123 Pine Avenue, Chicago, IL 60601",
                        "https://images.unsplash.com/photo-1472099645785-5658abf4ff4e?w=150&h=150&fit=crop&crop=face",
                        new DateTime(2023, 1, 20, 0, 0, 0, 0, DateTimeKind.Utc),
                        1,
                        3,
                        new DateTime(2023, 1, 20, 0, 0, 0, 0, DateTimeKind.Utc),
                        "Linda Johnson",
                        "(312) 555-7891",
                        "LS789456",
                        new DateTime(2026, 3, 15, 0, 0, 0, 0, DateTimeKind.Utc),
                        "Landscaping specialist with expertise in design and installation. Creative problem solver.",
                        new DateTimeOffset(2023, 1, 20, 8, 0, 0, TimeSpan.Zero),
                        new DateTimeOffset(2024, 1, 15, 8, 0, 0, TimeSpan.Zero)
                    },
                    {
                        new Guid("44444444-4444-4444-4444-444444444444"),
                        "Emily",
                        "Davis",
                        "emily.davis@lawncare.com",
                        "(217) 333-9876",
                        "789 Elm Street, Peoria, IL 61602",
                        "https://images.unsplash.com/photo-1438761681033-6461ffad8d80?w=150&h=150&fit=crop&crop=face",
                        new DateTime(2023, 7, 1, 0, 0, 0, 0, DateTimeKind.Utc),
                        1,
                        4,
                        new DateTime(2023, 7, 1, 0, 0, 0, 0, DateTimeKind.Utc),
                        "Michael Davis",
                        "(217) 333-9877",
                        "SR123789",
                        new DateTime(2025, 9, 30, 0, 0, 0, 0, DateTimeKind.Utc),
                        "Winter specialist focusing on snow removal and ice management. Available 24/7 during winter months.",
                        new DateTimeOffset(2023, 7, 1, 8, 0, 0, TimeSpan.Zero),
                        new DateTimeOffset(2024, 1, 15, 8, 0, 0, TimeSpan.Zero)
                    },
                    {
                        new Guid("55555555-5555-5555-5555-555555555555"),
                        "David",
                        "Brown",
                        "david.brown@lawncare.com",
                        "(309) 444-5555",
                        "321 Cedar Lane, Bloomington, IL 61701",
                        "https://images.unsplash.com/photo-1500648767791-00dcc994a43e?w=150&h=150&fit=crop&crop=face",
                        new DateTime(2021, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc),
                        1,
                        5,
                        new DateTime(2021, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc),
                        "Jennifer Brown",
                        "(309) 444-5556",
                        "GN456123",
                        new DateTime(2025, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc),
                        "General maintenance technician with broad skills across all service areas. Team lead for complex projects.",
                        new DateTimeOffset(2021, 4, 1, 8, 0, 0, TimeSpan.Zero),
                        new DateTimeOffset(2024, 1, 15, 8, 0, 0, TimeSpan.Zero)
                    },
                    {
                        new Guid("66666666-6666-6666-6666-666666666666"),
                        "Sarah",
                        "Wilson",
                        "sarah.wilson@lawncare.com",
                        "(618) 777-8888",
                        "654 Birch Road, Carbondale, IL 62901",
                        "https://images.unsplash.com/photo-1544005313-94ddf0286df2?w=150&h=150&fit=crop&crop=face",
                        new DateTime(2023, 9, 1, 0, 0, 0, 0, DateTimeKind.Utc),
                        2,
                        1,
                        new DateTime(2023, 9, 1, 0, 0, 0, 0, DateTimeKind.Utc),
                        "Tom Wilson",
                        "(618) 777-8889",
                        "LC987654",
                        new DateTime(2025, 8, 15, 0, 0, 0, 0, DateTimeKind.Utc),
                        "Part-time technician specializing in weekend and evening services. Reliable and punctual.",
                        new DateTimeOffset(2023, 9, 1, 8, 0, 0, TimeSpan.Zero),
                        new DateTimeOffset(2024, 1, 15, 8, 0, 0, TimeSpan.Zero)
                    },
                    {
                        new Guid("77777777-7777-7777-7777-777777777777"),
                        "Michael",
                        "Taylor",
                        "michael.taylor@lawncare.com",
                        "(815) 999-0000",
                        "987 Spruce Court, Rockford, IL 61103",
                        "https://images.unsplash.com/photo-1506794778202-cad84cf45f1d?w=150&h=150&fit=crop&crop=face",
                        new DateTime(2022, 11, 1, 0, 0, 0, 0, DateTimeKind.Utc),
                        3,
                        2,
                        new DateTime(2022, 11, 1, 0, 0, 0, 0, DateTimeKind.Utc),
                        "Lisa Taylor",
                        "(815) 999-0001",
                        "TC789123",
                        new DateTime(2026, 1, 31, 0, 0, 0, 0, DateTimeKind.Utc),
                        "Currently on medical leave. Expected to return in 2 months. Excellent safety record.",
                        new DateTimeOffset(2022, 11, 1, 8, 0, 0, TimeSpan.Zero),
                        new DateTimeOffset(2024, 1, 15, 8, 0, 0, TimeSpan.Zero)
                    },
                    {
                        new Guid("88888888-8888-8888-8888-888888888888"),
                        "Lisa",
                        "Anderson",
                        "lisa.anderson@lawncare.com",
                        "(312) 111-2222",
                        "147 Walnut Street, Evanston, IL 60201",
                        "https://images.unsplash.com/photo-1487412720507-e7ab37603c6f?w=150&h=150&fit=crop&crop=face",
                        new DateTime(2023, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc),
                        4,
                        3,
                        new DateTime(2023, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc),
                        "John Anderson",
                        "(312) 111-2223",
                        "LS321654",
                        new DateTime(2025, 11, 30, 0, 0, 0, 0, DateTimeKind.Utc),
                        "Recently terminated due to policy violations. Access should be revoked.",
                        new DateTimeOffset(2023, 12, 1, 8, 0, 0, TimeSpan.Zero),
                        new DateTimeOffset(2024, 1, 15, 8, 0, 0, TimeSpan.Zero)
                    }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Technicians",
                schema: "public");
        }
    }
}
