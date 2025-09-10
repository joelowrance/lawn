using System.ComponentModel.DataAnnotations;
using System.Net.Mail;
using System.Text.Json;

using LawnCare.Shared.MessageContracts;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

using OpenTelemetry.Instrumentation.AspNetCore;

namespace LawnCare.Communications
{
	public class EmailDbContext : DbContext
	{
		private readonly ILogger<EmailDbContext> _logger;

		public static readonly JsonSerializerOptions JsonOptions = new()
		{
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
			WriteIndented = false,
			PropertyNameCaseInsensitive = true
		};

		public EmailDbContext(DbContextOptions options, ILogger<EmailDbContext> logger) : base(options)
		{
			_logger = logger;
		}

		public DbSet<EmailRecord> EmailRecords { get; set; }
		
		
	}

	public class EmailRecord
	{
		public Guid Id { get; set; }
		public required string From { get; set; }
		public required string To { get; set; }
		public List<string> Cc { get; set; } = new();
		public List<string> Bcc { get; set; } = new();
		public required string Body { get; set; }
		public required string Subject { get; set; }
		public DateTimeOffset DateCreated { get; set; }
		public DateTimeOffset? DateSent { get; set; }
		public string? FailureReason { get; set; }


		public class EmailRecordConfiguration : IEntityTypeConfiguration<EmailRecord>
		{
			public void Configure(EntityTypeBuilder<EmailRecord> builder)
			{
				// Create value converter for List<string> to JSON string
				var stringListConverter = new ValueConverter<List<string>, string>(
					v => JsonSerializer.Serialize(v, (EmailDbContext.JsonOptions)),
					v => DeserializeStringList(v));

				// Create value comparer to properly detect changes in the lists
				var stringListComparer = new ValueComparer<List<string>>(
					// Compare two lists for equality
					(c1, c2) => c1 != null && c2 != null && c1.SequenceEqual(c2),
					// Generate hash code for a list
					c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
					// Create snapshot for change tracking
					c => c.ToList());

				builder.HasKey(x => x.Id);
				builder.ToTable("EmailRecords");
				builder.Property(x => x.Id).ValueGeneratedOnAdd();
				builder.Property(x => x.DateCreated).HasDefaultValueSql("CURRENT_TIMESTAMP");
				builder.Property(x => x.DateSent);
				builder.Property(x => x.FailureReason).HasMaxLength(255);
				builder.Property(x => x.From).HasMaxLength(255);
				builder.Property(x => x.To).HasMaxLength(255);

				// Configure Cc to be stored as JSON
				builder.Property(x => x.Cc)
					.HasColumnType("jsonb")
					.HasConversion(stringListConverter)
					//.HasMaxLength(-1)
					.Metadata.SetValueComparer(stringListComparer);
					
				
				builder.Property(x => x.Bcc)
					.HasColumnType("jsonb")
					.HasConversion(stringListConverter)
					//.HasMaxLength(-1)
					.Metadata.SetValueComparer(stringListComparer);

				builder.Property(x => x.Subject).HasMaxLength(512);
				builder.Property(x => x.Body).HasMaxLength(-1);
			}

			private static List<string> DeserializeStringList(string json)
			{
				// Handle null/empty JSON
				if (string.IsNullOrWhiteSpace(json))
				{
					return new List<string>(); // Return empty list as fallback
				}

				try
				{
					var result = JsonSerializer.Deserialize<List<string>>(json, EmailDbContext.JsonOptions);
					return result ?? new List<string>(); // Return empty list if deserialization returns null
				}
				catch (JsonException)
				{
					// If JSON is corrupted, return safe fallback instead of throwing
					return new List<string>();
				}
			}
		}

}

	public interface IEmailService
	{
		public Task SendWelcomeEmail(CustomerInfo messageCustomer);
		public Task SendJobCreatedEmailAsync(string customerEmail, string customerFirstName, string customerLastName, 
			string jobDescription, decimal estimatedCost, DateTimeOffset scheduledDate, string technicianName, string propertyAddress);
		public Task SendJobUpdatedEmailAsync(string customerEmail, string customerFirstName, string customerLastName, 
			string jobDescription, decimal estimatedCost, DateTimeOffset? scheduledDate, string technicianName, 
			string propertyAddress, string updateReason, string changesSummary);
		public Task SendJobCompletedEmailAsync(string customerEmail, string customerFirstName, string customerLastName, 
			string jobDescription, decimal actualCost, decimal estimatedCost, DateTimeOffset completedDate, 
			string technicianName, string propertyAddress, string completionNotes);
	}

	public class EmailService : IEmailService
	{
		ILogger<EmailService> _logger;
		private readonly SmtpClient _client;
		private readonly EmailDbContext _dbContext;

		public EmailService(ILogger<EmailService> logger, SmtpClient client, EmailDbContext dbContext)
		{
			_logger = logger;
			_client = client;
			_dbContext = dbContext;
		}

		public Task SendWelcomeEmail(CustomerInfo messageCustomer)
		{
			var message = new MailMessage();
			message.To.Add(new MailAddress(messageCustomer.Email));
			message.Subject = "Welcome to Fictional Lawn Care";
			message.Body = "Thank you for registering with Fictional Lawn Care.  We hope you enjoy your experience."; 
			message.IsBodyHtml = true;
			message.From = new MailAddress("fictionallawncar@gmail.com");
			try
			{
				_client.Send(message);
				return Task.CompletedTask;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error sending email to {Email}", messageCustomer.Email);
				return Task.CompletedTask;
			}
		}

		public async Task SendJobCreatedEmailAsync(string customerEmail, string customerFirstName, string customerLastName, 
			string jobDescription, decimal estimatedCost, DateTimeOffset scheduledDate, string technicianName, string propertyAddress)
		{
			var subject = "Your Lawn Care Service Has Been Scheduled";
			var body = $@"
				<html>
				<body>
					<h2>Hello {customerFirstName} {customerLastName}!</h2>
					<p>Your lawn care service has been successfully scheduled.</p>
					
					<h3>Service Details:</h3>
					<ul>
						<li><strong>Service:</strong> {jobDescription}</li>
						<li><strong>Estimated Cost:</strong> ${estimatedCost:F2}</li>
						<li><strong>Scheduled Date:</strong> {scheduledDate:MMM dd, yyyy 'at' h:mm tt}</li>
						<li><strong>Technician:</strong> {technicianName}</li>
						<li><strong>Property Address:</strong> {propertyAddress}</li>
					</ul>
					
					<p>We'll contact you if there are any changes to your scheduled service.</p>
					<p>Thank you for choosing Fictional Lawn Care!</p>
				</body>
				</html>";

			await SendEmailAsync(customerEmail, subject, body, EmailType.JobScheduled);
		}

		public async Task SendJobUpdatedEmailAsync(string customerEmail, string customerFirstName, string customerLastName, 
			string jobDescription, decimal estimatedCost, DateTimeOffset? scheduledDate, string technicianName, 
			string propertyAddress, string updateReason, string changesSummary)
		{
			var subject = "Your Lawn Care Service Has Been Updated";
			var scheduledDateText = scheduledDate?.ToString("MMM dd, yyyy 'at' h:mm tt") ?? "TBD";
			
			var body = $@"
				<html>
				<body>
					<h2>Hello {customerFirstName} {customerLastName}!</h2>
					<p>Your lawn care service has been updated.</p>
					
					<h3>Update Details:</h3>
					<p><strong>Reason for Update:</strong> {updateReason}</p>
					<p><strong>Changes Made:</strong> {changesSummary}</p>
					
					<h3>Current Service Details:</h3>
					<ul>
						<li><strong>Service:</strong> {jobDescription}</li>
						<li><strong>Estimated Cost:</strong> ${estimatedCost:F2}</li>
						<li><strong>Scheduled Date:</strong> {scheduledDateText}</li>
						<li><strong>Technician:</strong> {technicianName}</li>
						<li><strong>Property Address:</strong> {propertyAddress}</li>
					</ul>
					
					<p>If you have any questions about these changes, please don't hesitate to contact us.</p>
					<p>Thank you for choosing Fictional Lawn Care!</p>
				</body>
				</html>";

			await SendEmailAsync(customerEmail, subject, body, EmailType.JobScheduled);
		}

		public async Task SendJobCompletedEmailAsync(string customerEmail, string customerFirstName, string customerLastName, 
			string jobDescription, decimal actualCost, decimal estimatedCost, DateTimeOffset completedDate, 
			string technicianName, string propertyAddress, string completionNotes)
		{
			var subject = "Your Lawn Care Service Has Been Completed";
			var costDifference = actualCost - estimatedCost;
			var costDifferenceText = costDifference == 0 ? "No change" : 
				costDifference > 0 ? $"${costDifference:F2} additional" : 
				$"${Math.Abs(costDifference):F2} savings";
			
			var body = $@"
				<html>
				<body>
					<h2>Hello {customerFirstName} {customerLastName}!</h2>
					<p>Your lawn care service has been completed successfully.</p>
					
					<h3>Service Summary:</h3>
					<ul>
						<li><strong>Service:</strong> {jobDescription}</li>
						<li><strong>Estimated Cost:</strong> ${estimatedCost:F2}</li>
						<li><strong>Actual Cost:</strong> ${actualCost:F2}</li>
						<li><strong>Cost Difference:</strong> {costDifferenceText}</li>
						<li><strong>Completed Date:</strong> {completedDate:MMM dd, yyyy 'at' h:mm tt}</li>
						<li><strong>Technician:</strong> {technicianName}</li>
						<li><strong>Property Address:</strong> {propertyAddress}</li>
					</ul>
					
					{(string.IsNullOrWhiteSpace(completionNotes) ? "" : $"<h3>Completion Notes:</h3><p>{completionNotes}</p>")}
					
					<p>We hope you're satisfied with our service! If you have any questions or concerns, please don't hesitate to contact us.</p>
					<p>Thank you for choosing Fictional Lawn Care!</p>
				</body>
				</html>";

			await SendEmailAsync(customerEmail, subject, body, EmailType.JobCompleted);
		}

		private async Task SendEmailAsync(string toEmail, string subject, string body, EmailType emailType)
		{
			var emailRecord = new EmailRecord
			{
				Id = Guid.NewGuid(),
				From = "fictionallawncar@gmail.com",
				To = toEmail,
				Subject = subject,
				Body = body,
				DateCreated = DateTimeOffset.UtcNow
			};

			try
			{
				var message = new MailMessage();
				message.To.Add(new MailAddress(toEmail));
				message.Subject = subject;
				message.Body = body;
				message.IsBodyHtml = true;
				message.From = new MailAddress("fictionallawncar@gmail.com");

				_client.Send(message);
				
				emailRecord.DateSent = DateTimeOffset.UtcNow;
				_logger.LogInformation("Successfully sent {EmailType} email to {Email}", emailType, toEmail);
			}
			catch (Exception ex)
			{
				emailRecord.FailureReason = ex.Message;
				_logger.LogError(ex, "Error sending {EmailType} email to {Email}", emailType, toEmail);
				throw; // Re-throw to trigger retry mechanisms
			}
			finally
			{
				_dbContext.EmailRecords.Add(emailRecord);
				await _dbContext.SaveChangesAsync();
			}
		}
	}
		// public class SmtpSettings
    	// {
    	// 	public required string Host { get; set; } 
    	// 	public int Port { get; set; }
    	// 	public required string Username { get; set; }
    	// 	public required string Password { get; set; }	
    	// }

	
	// What do i need 
	// Name, Address, Email Address, Job Services, Schedule Date,
	// these can all arrive as commands from various services.
	
	//TODO:
	// All the tedious shit
	// Add EF,
	// Add Rabbit
	// Add Otel
	
	


	public enum EmailType
	{
		NewCustomerWelcome,
		EstimateCreated,
		JobScheduled,
		JobDayBeforeReminder,
		JobCompleted,
		JobCancelled,
	}
}