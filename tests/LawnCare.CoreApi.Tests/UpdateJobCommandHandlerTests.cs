using AutoFixture;
using FluentAssertions;
using LawnCare.CoreApi.Domain.Common;
using LawnCare.CoreApi.Domain.Entities;
using LawnCare.CoreApi.Domain.ValueObjects;
using LawnCare.CoreApi.Infrastructure.Database;
using LawnCare.CoreApi.UseCases;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace LawnCare.CoreApi.Tests;

public class UpdateJobCommandHandlerTests : IDisposable
{
    private readonly Fixture _fixture;
    private readonly CoreDbContext _dbContext;
    private readonly Mock<ILogger<CoreDbContext>> _dbLoggerMock;
    private readonly Mock<ILogger<UpdateJobCommandHandler>> _handlerLoggerMock;
    private readonly Mock<JobMappingService> _mappingServiceMock;
    private readonly UpdateJobCommandHandler _handler;

    public UpdateJobCommandHandlerTests()
    {
        _fixture = new Fixture();
        _fixture.Customize<Money>(c => c.FromFactory(() => new Money(_fixture.Create<decimal>())));
        _fixture.Customize<EmailAddress>(c => c.FromFactory(() => new EmailAddress(_fixture.Create<string>() + "@example.com")));
        _fixture.Customize<PhoneNumber>(c => c.FromFactory(() => new PhoneNumber("5552345678")));
        _fixture.Customize<Postcode>(c => c.FromFactory(() => new Postcode("12345")));
        _fixture.Customize<Customer>(c => c.FromFactory(() => 
            new Customer(
                _fixture.Create<string>(), 
                _fixture.Create<string>(), 
                new EmailAddress(_fixture.Create<string>() + "@example.com"),
                new PhoneNumber("5552345678"),
                new PhoneNumber("5559876543")
            )));

        // Setup in-memory database
        var options = new DbContextOptionsBuilder<CoreDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(warnings => warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;
            
        _dbLoggerMock = new Mock<ILogger<CoreDbContext>>();
        _dbContext = new CoreDbContext(options, _dbLoggerMock.Object);
        
        _handlerLoggerMock = new Mock<ILogger<UpdateJobCommandHandler>>();
        _mappingServiceMock = new Mock<JobMappingService>(_dbContext);
        
        _handler = new UpdateJobCommandHandler(_dbContext, _mappingServiceMock.Object, _handlerLoggerMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidJobId_ShouldUpdateJobAndReturnSuccess()
    {
        // Arrange
        var job = await CreateAndSaveJob();
        var command = new UpdateJobCommand
        {
            JobId = job.JobId.Value,
            Status = "InProgress",
            Priority = "Emergency",
            RequestedServiceDate = DateTimeOffset.UtcNow.AddDays(5),
            JobCost = 250.00m
        };

        var expectedDto = CreateServiceRequestDto();
        _mappingServiceMock.Setup(x => x.MapToServiceRequestDtoAsync(It.IsAny<Job>()))
            .ReturnsAsync(expectedDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expectedDto);
        result.Error.Should().BeNull();

        // Verify job was updated in database
        var updatedJob = await _dbContext.Jobs.FindAsync(job.JobId);
        updatedJob.Should().NotBeNull();
        updatedJob!.Status.Should().Be(JobStatus.InProgress);
        updatedJob.Priority.Should().Be(JobPriority.Emergency);
        updatedJob.JobCost.Amount.Should().Be(250.00m);
    }

    [Fact]
    public async Task Handle_WithNonExistentJobId_ShouldReturnFailure()
    {
        // Arrange
        var nonExistentJobId = Guid.NewGuid();
        var command = new UpdateJobCommand
        {
            JobId = nonExistentJobId,
            Status = "InProgress"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Value.Should().BeNull();
        result.Error.Should().Be("Job not found");
    }

    [Fact]
    public async Task Handle_WithInvalidStatus_ShouldNotUpdateStatus()
    {
        // Arrange
        var job = await CreateAndSaveJob();
        var originalStatus = job.Status;
        var command = new UpdateJobCommand
        {
            JobId = job.JobId.Value,
            Status = "InvalidStatus"
        };

        var expectedDto = CreateServiceRequestDto();
        _mappingServiceMock.Setup(x => x.MapToServiceRequestDtoAsync(It.IsAny<Job>()))
            .ReturnsAsync(expectedDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Verify status was not changed
        var updatedJob = await _dbContext.Jobs.FindAsync(job.JobId);
        updatedJob!.Status.Should().Be(originalStatus);
    }

    [Fact]
    public async Task Handle_WithInvalidPriority_ShouldNotUpdatePriority()
    {
        // Arrange
        var job = await CreateAndSaveJob();
        var originalPriority = job.Priority;
        var command = new UpdateJobCommand
        {
            JobId = job.JobId.Value,
            Priority = "InvalidPriority"
        };

        var expectedDto = CreateServiceRequestDto();
        _mappingServiceMock.Setup(x => x.MapToServiceRequestDtoAsync(It.IsAny<Job>()))
            .ReturnsAsync(expectedDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Verify priority was not changed
        var updatedJob = await _dbContext.Jobs.FindAsync(job.JobId);
        updatedJob!.Priority.Should().Be(originalPriority);
    }

    [Fact]
    public async Task Handle_WithServiceItems_ShouldUpdateServiceItems()
    {
        // Arrange
        var job = await CreateAndSaveJob();
        var command = new UpdateJobCommand
        {
            JobId = job.JobId.Value,
            ServiceItems = new List<ServiceItemRequest>
            {
                new() { ServiceName = "New Service", Quantity = 2, Comment = "New comment", Price = 100m },
                new() { ServiceName = "Another Service", Quantity = 1, Comment = "Another comment", Price = 50m }
            }
        };

        var expectedDto = CreateServiceRequestDto();
        _mappingServiceMock.Setup(x => x.MapToServiceRequestDtoAsync(It.IsAny<Job>()))
            .ReturnsAsync(expectedDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Verify service items were updated
        var updatedJob = await _dbContext.Jobs
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
    public async Task Handle_WithNotes_ShouldUpdateNotes()
    {
        // Arrange
        var job = await CreateAndSaveJob();
        var command = new UpdateJobCommand
        {
            JobId = job.JobId.Value,
            Notes = new List<string> { "First note", "Second note", "Third note" }
        };

        var expectedDto = CreateServiceRequestDto();
        _mappingServiceMock.Setup(x => x.MapToServiceRequestDtoAsync(It.IsAny<Job>()))
            .ReturnsAsync(expectedDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Verify notes were updated
        var updatedJob = await _dbContext.Jobs
            .Include(j => j.Notes)
            .FirstOrDefaultAsync(j => j.JobId == job.JobId);
        
        updatedJob.Should().NotBeNull();
        updatedJob!.Notes.Should().HaveCount(3);
        updatedJob.Notes.Select(n => n.Note).Should().Contain("First note");
        updatedJob.Notes.Select(n => n.Note).Should().Contain("Second note");
        updatedJob.Notes.Select(n => n.Note).Should().Contain("Third note");
    }

    [Fact]
    public async Task Handle_WithEmptyNotes_ShouldClearNotes()
    {
        // Arrange
        var job = await CreateAndSaveJob();
        job.AddNote("Original note");
        await _dbContext.SaveChangesAsync();

        var command = new UpdateJobCommand
        {
            JobId = job.JobId.Value,
            Notes = new List<string>()
        };

        var expectedDto = CreateServiceRequestDto();
        _mappingServiceMock.Setup(x => x.MapToServiceRequestDtoAsync(It.IsAny<Job>()))
            .ReturnsAsync(expectedDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Verify notes were cleared
        var updatedJob = await _dbContext.Jobs
            .Include(j => j.Notes)
            .FirstOrDefaultAsync(j => j.JobId == job.JobId);
        
        updatedJob.Should().NotBeNull();
        updatedJob!.Notes.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithWhitespaceOnlyNotes_ShouldIgnoreWhitespaceNotes()
    {
        // Arrange
        var job = await CreateAndSaveJob();
        var command = new UpdateJobCommand
        {
            JobId = job.JobId.Value,
            Notes = new List<string> { "Valid note", "   ", "", "Another valid note" }
        };

        var expectedDto = CreateServiceRequestDto();
        _mappingServiceMock.Setup(x => x.MapToServiceRequestDtoAsync(It.IsAny<Job>()))
            .ReturnsAsync(expectedDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Verify only non-whitespace notes were added
        var updatedJob = await _dbContext.Jobs
            .Include(j => j.Notes)
            .FirstOrDefaultAsync(j => j.JobId == job.JobId);
        
        updatedJob.Should().NotBeNull();
        updatedJob!.Notes.Should().HaveCount(2);
        updatedJob.Notes.Select(n => n.Note).Should().Contain("Valid note");
        updatedJob.Notes.Select(n => n.Note).Should().Contain("Another valid note");
    }

    [Fact]
    public async Task Handle_WithDatabaseException_ShouldReturnFailure()
    {
        // Arrange
        var job = await CreateAndSaveJob();
        var command = new UpdateJobCommand
        {
            JobId = job.JobId.Value,
            Status = "InProgress"
        };

        // Dispose the context to simulate database error
        _dbContext.Dispose();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Value.Should().BeNull();
        result.Error.Should().Be("An error occurred while updating the job");
    }

    [Fact]
    public async Task Handle_WithMappingServiceException_ShouldReturnFailure()
    {
        // Arrange
        var job = await CreateAndSaveJob();
        var command = new UpdateJobCommand
        {
            JobId = job.JobId.Value,
            Status = "InProgress"
        };

        _mappingServiceMock.Setup(x => x.MapToServiceRequestDtoAsync(It.IsAny<Job>()))
            .ThrowsAsync(new Exception("Mapping failed"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Value.Should().BeNull();
        result.Error.Should().Be("An error occurred while updating the job");
    }

    private async Task<Job> CreateAndSaveJob()
    {
        var customer = _fixture.Create<Customer>();
        _dbContext.Customers.Add(customer);
        await _dbContext.SaveChangesAsync();

        var location = new Location(
            "123 Main St",
            null,
            null,
            "Anytown",
            "NY",
            new Postcode("12345"),
            customer
        );
        _dbContext.Locations.Add(location);
        await _dbContext.SaveChangesAsync();

        var job = new Job(
            DateTimeOffset.UtcNow.AddDays(7),
            JobPriority.Normal,
            new Money(150.00m),
            location.LocationId
        );
        _dbContext.Jobs.Add(job);
        await _dbContext.SaveChangesAsync();

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
        _dbContext?.Dispose();
    }
}
