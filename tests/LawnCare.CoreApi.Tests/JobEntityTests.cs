using AutoFixture;
using FluentAssertions;
using LawnCare.CoreApi.Domain.Entities;
using LawnCare.CoreApi.Domain.ValueObjects;
using Xunit;

namespace LawnCare.CoreApi.Tests;

public class JobEntityTests
{
    private readonly Fixture _fixture;

    public JobEntityTests()
    {
        _fixture = new Fixture();
        _fixture.Customize<Money>(c => c.FromFactory(() => new Money(_fixture.Create<decimal>())));
    }

    [Fact]
    public void UpdateStatus_WithValidStatus_ShouldUpdateStatusAndTimestamp()
    {
        // Arrange
        var job = CreateValidJob();
        var originalUpdatedAt = job.UpdatedAt;
        var newStatus = JobStatus.InProgress;

        // Act
        job.UpdateStatus(newStatus);

        // Assert
        job.Status.Should().Be(newStatus);
        job.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }

    [Fact]
    public void UpdatePriority_WithValidPriority_ShouldUpdatePriorityAndTimestamp()
    {
        // Arrange
        var job = CreateValidJob();
        var originalUpdatedAt = job.UpdatedAt;
        var newPriority = JobPriority.Emergency;

        // Act
        job.UpdatePriority(newPriority);

        // Assert
        job.Priority.Should().Be(newPriority);
        job.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }

    [Fact]
    public void UpdateRequestedServiceDate_WithValidDate_ShouldUpdateDateAndTimestamp()
    {
        // Arrange
        var job = CreateValidJob();
        var originalUpdatedAt = job.UpdatedAt;
        var newDate = DateTimeOffset.UtcNow.AddDays(5);

        // Act
        job.UpdateRequestedServiceDate(newDate);

        // Assert
        job.RequestedServiceDate.Should().Be(newDate);
        job.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }

    [Fact]
    public void UpdateRequestedServiceDate_WithNull_ShouldUpdateDateAndTimestamp()
    {
        // Arrange
        var job = CreateValidJob();
        var originalUpdatedAt = job.UpdatedAt;

        // Act
        job.UpdateRequestedServiceDate(null);

        // Assert
        job.RequestedServiceDate.Should().BeNull();
        job.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }

    [Fact]
    public void UpdateJobCost_WithValidCost_ShouldUpdateCostAndTimestamp()
    {
        // Arrange
        var job = CreateValidJob();
        var originalUpdatedAt = job.UpdatedAt;
        var newCost = new Money(250.00m);

        // Act
        job.UpdateJobCost(newCost);

        // Assert
        job.JobCost.Should().Be(newCost);
        job.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }

    [Fact]
    public void UpdateJobCost_WithZeroCost_ShouldUpdateCostAndTimestamp()
    {
        // Arrange
        var job = CreateValidJob();
        var originalUpdatedAt = job.UpdatedAt;
        var newCost = new Money(0m);

        // Act
        job.UpdateJobCost(newCost);

        // Assert
        job.JobCost.Should().Be(newCost);
        job.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }

    [Fact]
    public void UpdateJobCost_WithNegativeCost_ShouldThrowArgumentException()
    {
        // Arrange
        var job = CreateValidJob();

        // Act & Assert
        // The Money constructor will throw the exception, not the Job method
        var action = () => new Money(-10m);
        action.Should().Throw<ArgumentException>()
            .WithMessage("Amount cannot be negative*")
            .And.ParamName.Should().Be("amount");
    }

    [Fact]
    public void ClearServices_ShouldRemoveAllServicesAndUpdateTimestamp()
    {
        // Arrange
        var job = CreateValidJob();
        job.AddService("Test Service", 1, "Test comment", new Money(100m));
        job.AddService("Another Service", 2, "Another comment", new Money(200m));
        var originalUpdatedAt = job.UpdatedAt;

        // Act
        job.ClearServices();

        // Assert
        job.ServiceItems.Should().BeEmpty();
        job.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }

    [Fact]
    public void ClearNotes_ShouldRemoveAllNotesAndUpdateTimestamp()
    {
        // Arrange
        var job = CreateValidJob();
        job.AddNote("First note");
        job.AddNote("Second note");
        var originalUpdatedAt = job.UpdatedAt;

        // Act
        job.ClearNotes();

        // Assert
        job.Notes.Should().BeEmpty();
        job.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }

    [Fact]
    public void AddService_AfterClearServices_ShouldAddNewService()
    {
        // Arrange
        var job = CreateValidJob();
        job.AddService("Original Service", 1, "Original comment", new Money(100m));
        job.ClearServices();

        // Act
        job.AddService("New Service", 2, "New comment", new Money(150m));

        // Assert
        job.ServiceItems.Should().HaveCount(1);
        job.ServiceItems.First().ServiceName.Should().Be("New Service");
        job.ServiceItems.First().Quantity.Should().Be(2);
        job.ServiceItems.First().Comment.Should().Be("New comment");
        job.ServiceItems.First().Price.Should().Be(new Money(150m));
    }

    [Fact]
    public void AddNote_AfterClearNotes_ShouldAddNewNote()
    {
        // Arrange
        var job = CreateValidJob();
        job.AddNote("Original note");
        job.ClearNotes();

        // Act
        job.AddNote("New note");

        // Assert
        job.Notes.Should().HaveCount(1);
        job.Notes.First().Note.Should().Be("New note");
    }

    [Fact]
    public void MultipleUpdates_ShouldUpdateTimestampForEachChange()
    {
        // Arrange
        var job = CreateValidJob();
        var originalUpdatedAt = job.UpdatedAt;

        // Act
        job.UpdateStatus(JobStatus.InProgress);
        var firstUpdate = job.UpdatedAt;
        
        job.UpdatePriority(JobPriority.Emergency);
        var secondUpdate = job.UpdatedAt;
        
        job.UpdateJobCost(new Money(300m));
        var thirdUpdate = job.UpdatedAt;

        // Assert
        firstUpdate.Should().BeAfter(originalUpdatedAt);
        secondUpdate.Should().BeAfter(firstUpdate);
        thirdUpdate.Should().BeAfter(secondUpdate);
    }

    private Job CreateValidJob()
    {
        var requestedDate = DateTimeOffset.UtcNow.AddDays(7);
        var priority = JobPriority.Normal;
        var cost = new Money(150.00m);
        var locationId = LocationId.Create();

        return new Job(requestedDate, priority, cost, locationId);
    }
}
