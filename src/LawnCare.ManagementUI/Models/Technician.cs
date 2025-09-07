namespace LawnCare.ManagementUI.Models;

public class Technician
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string CellPhone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string PhotoUrl { get; set; } = string.Empty;
    public int YearsWithCompany { get; set; }
    public int MonthsWithCompany { get; set; }
    public string Status { get; set; } = "Active";
    public string Specialization { get; set; } = string.Empty;
    public DateTime HireDate { get; set; }
    public string Email { get; set; } = string.Empty;
    public string EmergencyContact { get; set; } = string.Empty;
    public string EmergencyPhone { get; set; } = string.Empty;
    public string LicenseNumber { get; set; } = string.Empty;
    public DateTime LicenseExpiry { get; set; }
    public string Notes { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public DateTime LastModified { get; set; }

    public string FullName => $"{FirstName} {LastName}";
    
    public string ExperienceDisplay
    {
        get
        {
            if (YearsWithCompany > 0)
            {
                return $"{YearsWithCompany} year{(YearsWithCompany == 1 ? "" : "s")}";
            }
            else
            {
                return $"{MonthsWithCompany} month{(MonthsWithCompany == 1 ? "" : "s")}";
            }
        }
    }
}

public class TechnicianStatus
{
    public const string Active = "Active";
    public const string Inactive = "Inactive";
    public const string OnLeave = "On Leave";
    public const string Terminated = "Terminated";
}

public class TechnicianSpecialization
{
    public const string LawnCare = "Lawn Care";
    public const string TreeService = "Tree Service";
    public const string Landscaping = "Landscaping";
    public const string SnowRemoval = "Snow Removal";
    public const string General = "General";
}
