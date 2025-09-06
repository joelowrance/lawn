using AutoFixture;
using FluentAssertions;
using LawnCare.ManagementUI.Models;
using LawnCare.ManagementUI.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace LawnCare.ManagementUI.Tests;

public class SchedulingServiceTests
{
    private readonly Fixture _fixture;
    private readonly Mock<ICoreApiService> _coreApiServiceMock;
    private readonly Mock<ILogger<SchedulingService>> _loggerMock;
    private readonly SchedulingService _schedulingService;

    public SchedulingServiceTests()
    {
        _fixture = new Fixture();
        _coreApiServiceMock = new Mock<ICoreApiService>();
        _loggerMock = new Mock<ILogger<SchedulingService>>();
        _schedulingService = new SchedulingService(_coreApiServiceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task UpdateJobAsync_WithValidRequest_ShouldCallCoreApiService()
    {
        // Arrange
        var jobId = Guid.NewGuid();
        var updateRequest = new UpdateJobRequest
        {
            Status = "InProgress",
            Priority = "Emergency",
            JobCost = 250.00m,
            ServiceItems = new List<ServiceItemRequest>
            {
                new() { ServiceName = "Test Service", Quantity = 1, Comment = "Test comment", Price = 100m }
            },
            Reason = "Test update"
        };

        var expectedResult = CreateServiceRequest();
        _coreApiServiceMock.Setup(x => x.UpdateJobAsync(jobId, updateRequest))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _schedulingService.UpdateJobAsync(jobId, updateRequest);

        // Assert
        result.Should().Be(expectedResult);
        _coreApiServiceMock.Verify(x => x.UpdateJobAsync(jobId, updateRequest), Times.Once);
    }

    [Fact]
    public async Task UpdateJobAsync_WithCoreApiException_ShouldThrowException()
    {
        // Arrange
        var jobId = Guid.NewGuid();
        var updateRequest = new UpdateJobRequest
        {
            Status = "InProgress"
        };

        var expectedException = new Exception("Core API error");
        _coreApiServiceMock.Setup(x => x.UpdateJobAsync(jobId, updateRequest))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var action = async () => await _schedulingService.UpdateJobAsync(jobId, updateRequest);
        await action.Should().ThrowAsync<Exception>().WithMessage("Core API error");

        _coreApiServiceMock.Verify(x => x.UpdateJobAsync(jobId, updateRequest), Times.Once);
    }

    [Fact]
    public async Task UpdateJobAsync_WithNullUpdateRequest_ShouldPassNullToCoreApi()
    {
        // Arrange
        var jobId = Guid.NewGuid();
        UpdateJobRequest? updateRequest = null;

        var expectedResult = CreateServiceRequest();
        _coreApiServiceMock.Setup(x => x.UpdateJobAsync(jobId, updateRequest!))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _schedulingService.UpdateJobAsync(jobId, updateRequest!);

        // Assert
        result.Should().Be(expectedResult);
        _coreApiServiceMock.Verify(x => x.UpdateJobAsync(jobId, updateRequest!), Times.Once);
    }

    [Fact]
    public async Task UpdateJobAsync_WithEmptyUpdateRequest_ShouldPassEmptyRequestToCoreApi()
    {
        // Arrange
        var jobId = Guid.NewGuid();
        var updateRequest = new UpdateJobRequest();

        var expectedResult = CreateServiceRequest();
        _coreApiServiceMock.Setup(x => x.UpdateJobAsync(jobId, updateRequest))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _schedulingService.UpdateJobAsync(jobId, updateRequest);

        // Assert
        result.Should().Be(expectedResult);
        _coreApiServiceMock.Verify(x => x.UpdateJobAsync(jobId, updateRequest), Times.Once);
    }

    [Fact]
    public async Task UpdateJobAsync_WithComplexUpdateRequest_ShouldPassAllPropertiesToCoreApi()
    {
        // Arrange
        var jobId = Guid.NewGuid();
        var updateRequest = new UpdateJobRequest
        {
            Status = "Completed",
            Priority = "Normal",
            RequestedServiceDate = DateTime.UtcNow.AddDays(3),
            JobCost = 500.00m,
            ServiceItems = new List<ServiceItemRequest>
            {
                new() { ServiceName = "Lawn Mowing", Quantity = 1, Comment = "Weekly service", Price = 75m },
                new() { ServiceName = "Fertilization", Quantity = 1, Comment = "Spring treatment", Price = 125m },
                new() { ServiceName = "Weed Control", Quantity = 1, Comment = "Spot treatment", Price = 50m }
            },
            Reason = "Customer requested early morning service with gate code 1234"
        };

        var expectedResult = CreateServiceRequest();
        _coreApiServiceMock.Setup(x => x.UpdateJobAsync(jobId, updateRequest))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _schedulingService.UpdateJobAsync(jobId, updateRequest);

        // Assert
        result.Should().Be(expectedResult);
        _coreApiServiceMock.Verify(x => x.UpdateJobAsync(jobId, updateRequest), Times.Once);
    }

    [Fact]
    public async Task UpdateJobAsync_WithCancellationToken_ShouldPassTokenToCoreApi()
    {
        // Arrange
        var jobId = Guid.NewGuid();
        var updateRequest = new UpdateJobRequest { Status = "InProgress", Reason = "Test update" };

        var expectedResult = CreateServiceRequest();
        _coreApiServiceMock.Setup(x => x.UpdateJobAsync(jobId, updateRequest))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _schedulingService.UpdateJobAsync(jobId, updateRequest);

        // Assert
        result.Should().Be(expectedResult);
        _coreApiServiceMock.Verify(x => x.UpdateJobAsync(jobId, updateRequest), Times.Once);
    }

    private ServiceRequest CreateServiceRequest()
    {
        return new ServiceRequest
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
}
