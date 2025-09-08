using System;

namespace LawnCare.CoreApi.Domain.DTOs
{
    public class TechnicianDto
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string CellPhone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string PhotoUrl { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public int Status { get; set; }
        public string StatusDisplay { get; set; } = string.Empty;
        public int Specialization { get; set; }
        public string SpecializationDisplay { get; set; } = string.Empty;
        public DateTime HireDate { get; set; }
        public string EmergencyContact { get; set; } = string.Empty;
        public string EmergencyPhone { get; set; } = string.Empty;
        public string LicenseNumber { get; set; } = string.Empty;
        public DateTime LicenseExpiry { get; set; }
        public string Notes { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string ExperienceDisplay { get; set; } = string.Empty;
        public int YearsWithCompany { get; set; }
        public int MonthsWithCompany { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
    }
}

