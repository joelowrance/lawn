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
	}

	public class EmailService : IEmailService
	{
		ILogger<EmailService> _logger;
		private readonly SmtpClient _client;


		public EmailService(ILogger<EmailService> logger, SmtpClient client)
		{
			_logger = logger;
			_client = client;
		}

		//public async Task SendWelcomeEmail(Email email)
		// {
		// 	//todo:  local smtp
		// 	//todo: ses / whatever azure has / something esle
		// }
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