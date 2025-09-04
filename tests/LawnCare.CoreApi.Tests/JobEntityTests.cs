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
    public void UpdateJobDetails_WithValidChanges_ShouldUpdatePropertiesAndAddNote()
    {
        // Arrange
        var job = CreateValidJob();
        var originalUpdatedAt = job.UpdatedAt;
        var newStatus = JobStatus.InProgress;
        var newPriority = JobPriority.Emergency;
        var newDate = DateTimeOffset.UtcNow.AddDays(5);
        var newCost = new Money(250.00m);
        var reason = "Customer requested priority service";

        // Act
        job.UpdateJobDetails(newStatus, newPriority, newDate, newCost, null, reason);

        // Assert
        job.Status.Should().Be(newStatus);
        job.Priority.Should().Be(newPriority);
        job.RequestedServiceDate.Should().Be(newDate);
        job.JobCost.Should().Be(newCost);
        job.UpdatedAt.Should().BeAfter(originalUpdatedAt);
        job.Notes.Should().HaveCount(1);
        job.Notes.First().Note.Should().Be($"Job updated: {reason}");
    }

    [Fact]
    public void UpdateJobDetails_WithServiceItems_ShouldReplaceServiceItems()
    {
        // Arrange
        var job = CreateValidJob();
        var newServiceItems = new List<JobLineItem>
        {
            new JobLineItem(job.JobId, "New Service", 2, "New comment", new Money(150m)),
            new JobLineItem(job.JobId, "Another Service", 1, "Another comment", new Money(75m))
        };
        var reason = "Updated service requirements";

        // Act
        job.UpdateJobDetails(null, null, null, null, newServiceItems, reason);

        // Assert
        job.ServiceItems.Should().HaveCount(2);
        job.ServiceItems.First().ServiceName.Should().Be("New Service");
        job.ServiceItems.First().Quantity.Should().Be(2);
        job.ServiceItems.First().Comment.Should().Be("New comment");
        job.ServiceItems.First().Price.Should().Be(new Money(150m));
        job.Notes.Should().HaveCount(1);
        job.Notes.First().Note.Should().Be($"Job updated: {reason}");
    }

    [Fact]
    public void UpdateJobDetails_WithEmptyReason_ShouldThrowArgumentException()
    {
        // Arrange
        var job = CreateValidJob();

        // Act & Assert
        var action = () => job.UpdateJobDetails(null, null, null, null, null, "");
        action.Should().Throw<ArgumentException>()
            .WithMessage("Reason is required for job updates*")
            .And.ParamName.Should().Be("reason");
    }

    [Fact]
    public void UpdateJobDetails_WithNullReason_ShouldThrowArgumentException()
    {
        // Arrange
        var job = CreateValidJob();

        // Act & Assert
        var action = () => job.UpdateJobDetails(null, null, null, null, null, null!);
        action.Should().Throw<ArgumentException>()
            .WithMessage("Reason is required for job updates*")
            .And.ParamName.Should().Be("reason");
    }

    [Fact]
    public void UpdateJobDetails_WithWhitespaceReason_ShouldThrowArgumentException()
    {
        // Arrange
        var job = CreateValidJob();

        // Act & Assert
        var action = () => job.UpdateJobDetails(null, null, null, null, null, "   ");
        action.Should().Throw<ArgumentException>()
            .WithMessage("Reason is required for job updates*")
            .And.ParamName.Should().Be("reason");
    }

    [Fact]
    public void UpdateJobDetails_WithNoChanges_ShouldNotUpdateTimestampOrAddNote()
    {
        // Arrange
        var job = CreateValidJob();
        var originalUpdatedAt = job.UpdatedAt;
        var originalNotesCount = job.Notes.Count;

        // Act
        job.UpdateJobDetails(job.Status, job.Priority, job.RequestedServiceDate, job.JobCost, null, "No changes made");

        // Assert
        job.UpdatedAt.Should().Be(originalUpdatedAt);
        job.Notes.Should().HaveCount(originalNotesCount);
    }

    [Fact]
    public void UpdateJobDetails_WithPartialChanges_ShouldUpdateOnlyChangedProperties()
    {
        // Arrange
        var job = CreateValidJob();
        var originalStatus = job.Status;
        var originalPriority = job.Priority;
        var newDate = DateTimeOffset.UtcNow.AddDays(10);
        var reason = "Rescheduled for next week";

        // Act
        job.UpdateJobDetails(null, null, newDate, null, null, reason);

        // Assert
        job.Status.Should().Be(originalStatus); // Unchanged
        job.Priority.Should().Be(originalPriority); // Unchanged
        job.RequestedServiceDate.Should().Be(newDate); // Changed
        job.Notes.Should().HaveCount(1);
        job.Notes.First().Note.Should().Be($"Job updated: {reason}");
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
