using LawnCare.ManagementUI.Models;

namespace LawnCare.ManagementUI.Services;

public interface ISchedulingService
{
    Task<List<ServiceRequest>> GetUpcomingJobsAsync();
    Task<ServiceRequest?> GetJobByIdAsync(int id);
    Task<List<ServiceRequest>> GetJobsByDateAsync(DateTime date);
    Task<List<ServiceRequest>> GetJobsByStatusAsync(string status);
}

public class SchedulingService : ISchedulingService
{
    private readonly List<ServiceRequest> _dummyJobs;

    public SchedulingService()
    {
        _dummyJobs = GenerateDummyData();
    }

    public async Task<List<ServiceRequest>> GetUpcomingJobsAsync()
    {
        await Task.Delay(100); // Simulate API call
        return _dummyJobs
            .Where(job => job.ScheduledDate >= DateTime.Today)
            .OrderBy(job => job.ScheduledDate)
            .ThenBy(job => job.ScheduledTime)
            .ToList();
    }

    public async Task<ServiceRequest?> GetJobByIdAsync(int id)
    {
        await Task.Delay(50); // Simulate API call
        return _dummyJobs.FirstOrDefault(job => job.Id == id);
    }

    public async Task<List<ServiceRequest>> GetJobsByDateAsync(DateTime date)
    {
        await Task.Delay(50); // Simulate API call
        return _dummyJobs
            .Where(job => job.ScheduledDate.Date == date.Date)
            .OrderBy(job => job.ScheduledTime)
            .ToList();
    }

    public async Task<List<ServiceRequest>> GetJobsByStatusAsync(string status)
    {
        await Task.Delay(50); // Simulate API call
        return _dummyJobs
            .Where(job => job.Status == status)
            .OrderBy(job => job.ScheduledDate)
            .ThenBy(job => job.ScheduledTime)
            .ToList();
    }

    private List<ServiceRequest> GenerateDummyData()
    {
        var jobs = new List<ServiceRequest>();
        var random = new Random(42); // Fixed seed for consistent data

        var serviceTypes = new[]
        {
            new { Name = "Lawn Mowing", Icon = "🌱", Color = "#10B981" },
            new { Name = "Tree Pruning", Icon = "🌳", Color = "#059669" },
            new { Name = "Fertilization", Icon = "🌿", Color = "#16A34A" },
            new { Name = "Weed Control", Icon = "🌾", Color = "#65A30D" },
            new { Name = "Leaf Cleanup", Icon = "🍂", Color = "#CA8A04" },
            new { Name = "Snow Removal", Icon = "❄️", Color = "#0EA5E9" },
            new { Name = "Aeration", Icon = "🕳️", Color = "#7C3AED" },
            new { Name = "Pest Control", Icon = "🐛", Color = "#DC2626" }
        };

        var customerNames = new[]
        {
            "Smith Family", "Johnson Residence", "Williams Property", "Brown Estate",
            "Davis Home", "Miller House", "Wilson Property", "Moore Residence",
            "Taylor Estate", "Anderson Home", "Thomas Property", "Jackson House",
            "White Residence", "Harris Estate", "Martin Property", "Thompson Home"
        };

        var addresses = new[]
        {
            "123 Oak Street", "456 Maple Avenue", "789 Pine Road", "321 Elm Drive",
            "654 Birch Lane", "987 Cedar Court", "147 Spruce Street", "258 Willow Way",
            "369 Ash Boulevard", "741 Poplar Place", "852 Hickory Hill", "963 Sycamore Street",
            "159 Dogwood Drive", "357 Magnolia Lane", "468 Redwood Road", "579 Sequoia Avenue"
        };

        var technicians = new[]
        {
            "Mike Rodriguez", "Sarah Johnson", "David Chen", "Lisa Martinez",
            "Tom Wilson", "Jennifer Brown", "Chris Davis", "Amanda Taylor"
        };

        var statuses = new[] { JobStatus.Scheduled, JobStatus.InProgress, JobStatus.Completed };
        var priorities = new[] { JobPriority.Low, JobPriority.Medium, JobPriority.High, JobPriority.Urgent };

        for (int i = 1; i <= 50; i++)
        {
            var serviceType = serviceTypes[random.Next(serviceTypes.Length)];
            var scheduledDate = DateTime.Today.AddDays(random.Next(-2, 14)); // Past 2 days to 2 weeks ahead
            var scheduledTime = TimeSpan.FromHours(8 + random.Next(0, 8)); // 8 AM to 4 PM
            var duration = TimeSpan.FromMinutes(30 + random.Next(0, 180)); // 30 minutes to 3.5 hours

            jobs.Add(new ServiceRequest
            {
                Id = i,
                CustomerName = customerNames[random.Next(customerNames.Length)],
                PropertyAddress = addresses[random.Next(addresses.Length)],
                ServiceType = serviceType.Name,
                Description = GetServiceDescription(serviceType.Name),
                ScheduledDate = scheduledDate,
                ScheduledTime = scheduledTime,
                EstimatedDuration = duration,
                Status = statuses[random.Next(statuses.Length)],
                Priority = priorities[random.Next(priorities.Length)],
                AssignedTechnician = technicians[random.Next(technicians.Length)],
                EstimatedCost = 50 + random.Next(0, 300),
                Notes = GetRandomNotes(random),
                PropertySize = $"{random.Next(1000, 10000)} sq ft",
                SpecialInstructions = GetSpecialInstructions(random),
                ContactPhone = $"(555) {random.Next(100, 999)}-{random.Next(1000, 9999)}",
                ContactEmail = $"customer{i}@example.com",
                CreatedDate = DateTime.Now.AddDays(-random.Next(1, 30)),
                LastModified = DateTime.Now.AddHours(-random.Next(1, 48))
            });
        }

        return jobs;
    }

    private string GetServiceDescription(string serviceType)
    {
        return serviceType switch
        {
            "Lawn Mowing" => "Regular lawn mowing and edging service",
            "Tree Pruning" => "Professional tree trimming and pruning",
            "Fertilization" => "Lawn fertilization and soil treatment",
            "Weed Control" => "Weed prevention and removal treatment",
            "Leaf Cleanup" => "Fall leaf removal and yard cleanup",
            "Snow Removal" => "Driveway and walkway snow clearing",
            "Aeration" => "Lawn aeration and overseeding",
            "Pest Control" => "Pest and insect control treatment",
            _ => "Professional lawn care service"
        };
    }

    private string GetRandomNotes(Random random)
    {
        var notes = new[]
        {
            "Customer prefers morning appointments",
            "Gate code: 1234",
            "Please avoid the flower beds",
            "Dog in backyard - please be careful",
            "Customer will be home during service",
            "Leave invoice in mailbox",
            "Special attention to front yard",
            "Customer requested specific fertilizer brand",
            "Property has irrigation system",
            "Please call 30 minutes before arrival"
        };

        return random.Next(3) == 0 ? notes[random.Next(notes.Length)] : string.Empty;
    }

    private string GetSpecialInstructions(Random random)
    {
        var instructions = new[]
        {
            "Use eco-friendly products only",
            "Avoid parking on the grass",
            "Please water plants after service",
            "Customer has allergies - use hypoallergenic products",
            "Property has security cameras",
            "Please clean up all debris",
            "Customer prefers organic treatments",
            "Please lock gate when leaving",
            "Avoid the vegetable garden area",
            "Customer will provide refreshments"
        };

        return random.Next(4) == 0 ? instructions[random.Next(instructions.Length)] : string.Empty;
    }
}
