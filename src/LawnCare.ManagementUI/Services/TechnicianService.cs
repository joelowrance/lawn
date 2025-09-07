using LawnCare.ManagementUI.Models;

namespace LawnCare.ManagementUI.Services;

public class TechnicianService : ITechnicianService
{
    private readonly List<Technician> _technicians;

    public TechnicianService()
    {
        _technicians = GenerateDummyTechnicians();
    }

    public Task<List<Technician>> GetAllTechniciansAsync()
    {
        return Task.FromResult(_technicians.ToList());
    }

    public Task<Technician?> GetTechnicianByIdAsync(Guid id)
    {
        return Task.FromResult(_technicians.FirstOrDefault(t => t.Id == id));
    }

    public Task<List<Technician>> GetTechniciansByStatusAsync(string status)
    {
        return Task.FromResult(_technicians.Where(t => t.Status == status).ToList());
    }

    public Task<List<Technician>> SearchTechniciansAsync(string searchTerm)
    {
        var term = searchTerm.ToLowerInvariant();
        return Task.FromResult(_technicians.Where(t => 
            t.FirstName.ToLowerInvariant().Contains(term) ||
            t.LastName.ToLowerInvariant().Contains(term) ||
            t.FullName.ToLowerInvariant().Contains(term) ||
            t.Specialization.ToLowerInvariant().Contains(term)
        ).ToList());
    }

    public Task<Technician> CreateTechnicianAsync(Technician technician)
    {
        technician.Id = Guid.NewGuid();
        technician.CreatedDate = DateTime.UtcNow;
        technician.LastModified = DateTime.UtcNow;
        _technicians.Add(technician);
        return Task.FromResult(technician);
    }

    public Task<Technician> UpdateTechnicianAsync(Technician technician)
    {
        var existing = _technicians.FirstOrDefault(t => t.Id == technician.Id);
        if (existing != null)
        {
            technician.LastModified = DateTime.UtcNow;
            var index = _technicians.IndexOf(existing);
            _technicians[index] = technician;
        }
        return Task.FromResult(technician);
    }

    public Task<bool> DeleteTechnicianAsync(Guid id)
    {
        var technician = _technicians.FirstOrDefault(t => t.Id == id);
        if (technician != null)
        {
            _technicians.Remove(technician);
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    private List<Technician> GenerateDummyTechnicians()
    {
        var random = new Random(42); // Fixed seed for consistent data
        var technicians = new List<Technician>();
        var specializations = new[] { "Lawn Care", "Tree Service", "Landscaping", "Snow Removal", "General" };
        var statuses = new[] { "Active", "Active", "Active", "Active", "On Leave" }; // Mostly active with some on leave

        var firstNames = new[]
        {
            "James", "Michael", "Robert", "John", "David", "William", "Richard", "Joseph", "Thomas", "Christopher",
            "Charles", "Daniel", "Matthew", "Anthony", "Mark", "Donald", "Steven", "Paul", "Andrew", "Joshua"
        };

        var lastNames = new[]
        {
            "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis", "Rodriguez", "Martinez",
            "Hernandez", "Lopez", "Gonzalez", "Wilson", "Anderson", "Thomas", "Taylor", "Moore", "Jackson", "Martin"
        };

        var addresses = new[]
        {
            "123 Oak Street, Springfield, IL 62701",
            "456 Pine Avenue, Chicago, IL 60601",
            "789 Maple Drive, Rockford, IL 61101",
            "321 Elm Street, Peoria, IL 61602",
            "654 Cedar Lane, Aurora, IL 60502",
            "987 Birch Road, Naperville, IL 60540",
            "147 Spruce Court, Joliet, IL 60435",
            "258 Willow Way, Waukegan, IL 60085",
            "369 Ash Street, Cicero, IL 60804",
            "741 Poplar Place, Champaign, IL 61820",
            "852 Hickory Hill, Bloomington, IL 61701",
            "963 Sycamore Street, Arlington Heights, IL 60004",
            "159 Dogwood Drive, Evanston, IL 60201",
            "357 Magnolia Lane, Decatur, IL 62521",
            "468 Redwood Road, Schaumburg, IL 60173",
            "579 Cherry Street, Bolingbrook, IL 60440",
            "680 Walnut Avenue, Palatine, IL 60067",
            "791 Chestnut Circle, Skokie, IL 60076",
            "802 Beech Boulevard, Des Plaines, IL 60016",
            "913 Fir Street, Oak Lawn, IL 60453"
        };

        var photoUrls = new[]
        {
            "https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=150&h=150&fit=crop&crop=face",
            "https://images.unsplash.com/photo-1472099645785-5658abf4ff4e?w=150&h=150&fit=crop&crop=face",
            "https://images.unsplash.com/photo-1500648767791-00dcc994a43e?w=150&h=150&fit=crop&crop=face",
            "https://images.unsplash.com/photo-1494790108755-2616b612b786?w=150&h=150&fit=crop&crop=face",
            "https://images.unsplash.com/photo-1506794778202-cad84cf45f1d?w=150&h=150&fit=crop&crop=face",
            "https://images.unsplash.com/photo-1519244703995-f4e0f30006d5?w=150&h=150&fit=crop&crop=face",
            "https://images.unsplash.com/photo-1507591064344-4c6ce005b128?w=150&h=150&fit=crop&crop=face",
            "https://images.unsplash.com/photo-1517841905240-472988babdf9?w=150&h=150&fit=crop&crop=face",
            "https://images.unsplash.com/photo-1534528741775-53994a69daeb?w=150&h=150&fit=crop&crop=face",
            "https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=150&h=150&fit=crop&crop=face",
            "https://images.unsplash.com/photo-1472099645785-5658abf4ff4e?w=150&h=150&fit=crop&crop=face",
            "https://images.unsplash.com/photo-1500648767791-00dcc994a43e?w=150&h=150&fit=crop&crop=face",
            "https://images.unsplash.com/photo-1494790108755-2616b612b786?w=150&h=150&fit=crop&crop=face",
            "https://images.unsplash.com/photo-1506794778202-cad84cf45f1d?w=150&h=150&fit=crop&crop=face",
            "https://images.unsplash.com/photo-1519244703995-f4e0f30006d5?w=150&h=150&fit=crop&crop=face",
            "https://images.unsplash.com/photo-1507591064344-4c6ce005b128?w=150&h=150&fit=crop&crop=face",
            "https://images.unsplash.com/photo-1517841905240-472988babdf9?w=150&h=150&fit=crop&crop=face",
            "https://images.unsplash.com/photo-1534528741775-53994a69daeb?w=150&h=150&fit=crop&crop=face",
            "https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=150&h=150&fit=crop&crop=face",
            "https://images.unsplash.com/photo-1472099645785-5658abf4ff4e?w=150&h=150&fit=crop&crop=face"
        };

        for (int i = 0; i < 20; i++)
        {
            var yearsWithCompany = random.Next(0, 9);
            var monthsWithCompany = yearsWithCompany == 0 ? random.Next(1, 12) : 0;
            var hireDate = DateTime.UtcNow.AddYears(-yearsWithCompany).AddMonths(-monthsWithCompany);
            
            var technician = new Technician
            {
                Id = Guid.NewGuid(),
                FirstName = firstNames[i],
                LastName = lastNames[i],
                CellPhone = GeneratePhoneNumber(random),
                Address = addresses[i],
                PhotoUrl = photoUrls[i],
                YearsWithCompany = yearsWithCompany,
                MonthsWithCompany = monthsWithCompany,
                Status = statuses[random.Next(statuses.Length)],
                Specialization = specializations[random.Next(specializations.Length)],
                HireDate = hireDate,
                Email = $"{firstNames[i].ToLower()}.{lastNames[i].ToLower()}@lawncare.com",
                EmergencyContact = $"{firstNames[i]} {lastNames[i]} Sr.",
                EmergencyPhone = GeneratePhoneNumber(random),
                LicenseNumber = $"LC{random.Next(100000, 999999)}",
                LicenseExpiry = DateTime.UtcNow.AddYears(random.Next(1, 3)),
                Notes = GenerateRandomNotes(random),
                CreatedDate = hireDate,
                LastModified = DateTime.UtcNow.AddDays(-random.Next(0, 30))
            };

            technicians.Add(technician);
        }

        return technicians;
    }

    private string GeneratePhoneNumber(Random random)
    {
        return $"({random.Next(200, 999)}) {random.Next(200, 999)}-{random.Next(1000, 9999)}";
    }

    private string GenerateRandomNotes(Random random)
    {
        var notes = new[]
        {
            "Excellent customer service skills",
            "Certified in pesticide application",
            "Experienced with commercial equipment",
            "Bilingual - English and Spanish",
            "CDL license holder",
            "Specializes in organic lawn care",
            "Available for weekend work",
            "Team lead for route optimization",
            "Trained in tree climbing safety",
            "Certified arborist",
            "Equipment maintenance specialist",
            "Customer favorite technician",
            "New hire - completing training",
            "Seasonal worker - summer only",
            "Part-time availability",
            "Reliable and punctual",
            "Strong problem-solving skills",
            "Works well in team environment",
            "Independent worker",
            "Safety-conscious approach"
        };

        return notes[random.Next(notes.Length)];
    }
}
