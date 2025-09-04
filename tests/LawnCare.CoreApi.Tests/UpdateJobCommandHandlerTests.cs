using AutoFixture;
using FluentAssertions;
using LawnCare.CoreApi.Domain.Common;
using LawnCare.CoreApi.Domain.Entities;
using LawnCare.CoreApi.Domain.ValueObjects;
using LawnCare.CoreApi.Infrastructure.Database;
using LawnCare.CoreApi.UseCases;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Sqlite;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace LawnCare.CoreApi.Tests;

public class UpdateJobCommandHandlerTests : IDisposable
{
    private readonly Fixture fixture;
    private readonly CoreDbContext dbContext;
    private readonly Mock<ILogger<CoreDbContext>> dbLoggerMock;
    private readonly Mock<ILogger<UpdateJobCommandHandler>> handlerLoggerMock;
    private readonly Mock<IJobMappingService> mappingServiceMock;
    private readonly UpdateJobCommandHandler handler;
    private readonly string databasePath;

    public UpdateJobCommandHandlerTests()
    {
        fixture = new Fixture();
        fixture.Customize<Money>(c => c.FromFactory(() => new Money(fixture.Create<decimal>())));
        fixture.Customize<EmailAddress>(c => c.FromFactory(() => new EmailAddress(fixture.Create<string>() + "@example.com")));
        fixture.Customize<PhoneNumber>(c => c.FromFactory(() => new PhoneNumber("5552345678")));
        fixture.Customize<Postcode>(c => c.FromFactory(() => new Postcode("12345")));
        fixture.Customize<Customer>(c => c.FromFactory(() =>
            new Customer(
                fixture.Create<string>(),
                fixture.Create<string>(),
                new EmailAddress(fixture.Create<string>() + "@example.com"),
                new PhoneNumber("5552345678"),
                new PhoneNumber("5559876543")
            )));

        // Setup file-based SQLite database for better complex property support
        databasePath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.db");
        var options = new DbContextOptionsBuilder<CoreDbContext>()
            .UseSqlite($"DataSource={databasePath}")
            .Options;

        dbLoggerMock = new Mock<ILogger<CoreDbContext>>();
        dbContext = new CoreDbContext(options, dbLoggerMock.Object);

        // Ensure database is created and migrations are applied
        dbContext.Database.EnsureCreated();

        handlerLoggerMock = new Mock<ILogger<UpdateJobCommandHandler>>();
        mappingServiceMock = new Mock<IJobMappingService>();

        handler = new UpdateJobCommandHandler(dbContext, mappingServiceMock.Object, handlerLoggerMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidJobId_ShouldUpdateJobAndReturnSuccessAsync()
    {
        // Arrange
        var job = await CreateAndSaveJobAsync();
        var command = new UpdateJobCommand
        {
            JobId = job.JobId.Value,
            Status = "InProgress",
            Priority = "Emergency",
            RequestedServiceDate = DateTimeOffset.UtcNow.AddDays(5),
            JobCost = 250.00m,
            Reason = "Customer requested priority service"
        };

        var expectedDto = CreateServiceRequestDto();

        mappingServiceMock.Setup(x => x.MapToServiceRequestDto(It.IsAny<Job>(), It.IsAny<Location>()))
            .Returns(expectedDto);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue($"Expected success but got error: {result.Error}");
        result.Value.Should().Be(expectedDto);
        result.Error.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WithNonExistentJobId_ShouldReturnFailureAsync()
    {
        // Arrange
        var nonExistentJobId = Guid.NewGuid();
        var command = new UpdateJobCommand
        {
            JobId = nonExistentJobId,
            Status = "InProgress",
            Reason = "Test update"
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Value.Should().BeNull();
        result.Error.Should().Contain("Job not found");
    }

    [Fact]
    public async Task Handle_WithInvalidStatus_ShouldNotUpdateStatusAsync()
    {
        // Arrange
        var job = await CreateAndSaveJobAsync();
        var originalStatus = job.Status;
        var command = new UpdateJobCommand
        {
            JobId = job.JobId.Value,
            Status = "InvalidStatus",
            Reason = "Test update"
        };

        var expectedDto = CreateServiceRequestDto();

        mappingServiceMock.Setup(x => x.MapToServiceRequestDto(It.IsAny<Job>(), It.IsAny<Location>()))
            .Returns(expectedDto);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Verify status was not changed
        var updatedJob = await dbContext.Jobs.FindAsync(job.JobId);
        updatedJob!.Status.Should().Be(originalStatus);
    }

    [Fact]
    public async Task Handle_WithInvalidPriority_ShouldNotUpdatePriorityAsync()
    {
        // Arrange
        var job = await CreateAndSaveJobAsync();
        var originalPriority = job.Priority;
        var command = new UpdateJobCommand
        {
            JobId = job.JobId.Value,
            Priority = "InvalidPriority",
            Reason = "Test update"
        };

        var expectedDto = CreateServiceRequestDto();

        mappingServiceMock.Setup(x => x.MapToServiceRequestDto(It.IsAny<Job>(), It.IsAny<Location>()))
            .Returns(expectedDto);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Verify priority was not changed
        var updatedJob = await dbContext.Jobs.FindAsync(job.JobId);
        updatedJob!.Priority.Should().Be(originalPriority);
    }

    [Fact]
    public async Task Handle_WithServiceItems_ShouldUpdateServiceItemsAsync()
    {
        // Arrange
        var job = await CreateAndSaveJobAsync();
        var command = new UpdateJobCommand
        {
            JobId = job.JobId.Value,
            ServiceItems = new List<ServiceItemRequest>
            {
                new() { ServiceName = "New Service", Quantity = 2, Comment = "New comment", Price = 100m },
                new() { ServiceName = "Another Service", Quantity = 1, Comment = "Another comment", Price = 50m }
            },
            Reason = "Updated service requirements"
        };

        var expectedDto = CreateServiceRequestDto();

        mappingServiceMock.Setup(x => x.MapToServiceRequestDto(It.IsAny<Job>(), It.IsAny<Location>()))
            .Returns(expectedDto);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Verify service items were updated
        var updatedJob = await dbContext.Jobs
            .Include(j => j.ServiceItems)
            .FirstOrDefaultAsync(j => j.JobId == job.JobId);

        updatedJob.Should().NotBeNull();
        updatedJob!.ServiceItems.Should().HaveCount(2);
        updatedJob.ServiceItems.First().ServiceName.Should().Be("New Service");
        updatedJob.ServiceItems.First().Quantity.Should().Be(2);
        updatedJob.ServiceItems.First().Comment.Should().Be("New comment");
        updatedJob.ServiceItems.First().Price.Amount.Should().Be(100m);
    }

    [Fact]
    public async Task Handle_WithReason_ShouldAddReasonAsNoteAsync()
    {
        // Arrange
        var job = await CreateAndSaveJobAsync();
        var command = new UpdateJobCommand
        {
            JobId = job.JobId.Value,
            Status = "InProgress",
            Reason = "Customer requested status change"
        };

        var expectedDto = CreateServiceRequestDto();

        mappingServiceMock.Setup(x => x.MapToServiceRequestDto(It.IsAny<Job>(), It.IsAny<Location>()))
            .Returns(expectedDto);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Verify reason was added as a note
        var updatedJob = await dbContext.Jobs
            .Include(j => j.Notes)
            .FirstOrDefaultAsync(j => j.JobId == job.JobId);

        updatedJob.Should().NotBeNull();
        updatedJob!.Notes.Should().HaveCount(1);
        updatedJob.Notes.First().Note.Should().Be("Job updated: Customer requested status change");
    }

    [Fact]
    public async Task Handle_WithDatabaseException_ShouldReturnFailureAsync()
    {
        // Arrange
        var job = await CreateAndSaveJobAsync();
        var command = new UpdateJobCommand
        {
            JobId = job.JobId.Value,
            Status = "InProgress",
            Reason = "Test update"
        };

        // Dispose the context to simulate database error
        dbContext.Dispose();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Value.Should().BeNull();
        result.Error.Should().Contain("An error occurred while updating the job");
    }

    [Fact]
    public async Task Handle_WithMappingServiceException_ShouldReturnFailureAsync()
    {
        // Arrange
        var job = await CreateAndSaveJobAsync();
        var command = new UpdateJobCommand
        {
            JobId = job.JobId.Value,
            Status = "InProgress",
            Reason = "Test update"
        };

        mappingServiceMock.Setup(x => x.MapToServiceRequestDto(It.IsAny<Job>(), It.IsAny<Location>()))
            .Throws(new Exception("Mapping failed"));

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Value.Should().BeNull();
        result.Error.Should().Contain("An error occurred while updating the job");
    }

    private async Task<Job> CreateAndSaveJobAsync()
    {
        var customer = fixture.Create<Customer>();
        dbContext.Customers.Add(customer);
        await dbContext.SaveChangesAsync();

        var location = new Location(
            "123 Main St",
            null,
            null,
            "Anytown",
            "NY",
            new Postcode("12345"),
            customer
        );
        dbContext.Locations.Add(location);
        await dbContext.SaveChangesAsync();

        var job = new Job(
            DateTimeOffset.UtcNow.AddDays(7),
            JobPriority.Normal,
            new Money(150.00m),
            location.LocationId
        );
        dbContext.Jobs.Add(job);
        await dbContext.SaveChangesAsync();

        return job;
    }

    private ServiceRequestDto CreateServiceRequestDto()
    {
        return new ServiceRequestDto
        {
            Id = Guid.NewGuid(),
            CustomerName = "Test Customer",
            PropertyAddress = "123 Test St",
            ServiceType = "Test Service",
            Description = "Test Description",
            ScheduledDate = DateTime.UtcNow,
            ScheduledTime = TimeSpan.FromHours(9),
            EstimatedDuration = TimeSpan.FromHours(1),
            Status = "Pending",
            Priority = "Normal",
            AssignedTechnician = "Test Technician",
            EstimatedCost = 150.00m,
            Notes = "Test Notes",
            PropertySize = "Medium",
            SpecialInstructions = "Test Instructions",
            ContactPhone = "555-1234",
            ContactEmail = "test@example.com",
            CreatedDate = DateTime.UtcNow,
            LastModified = DateTime.UtcNow
        };
    }

    public void Dispose()
    {
        dbContext?.Dispose();

        // Clean up temporary database file
        if (File.Exists(databasePath))
        {
            try
            {
                File.Delete(databasePath);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }
}
