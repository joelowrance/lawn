using AutoFixture;
using FluentAssertions;
using LawnCare.ManagementUI.Models;
using LawnCare.ManagementUI.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text;
using System.Text.Json;
using Xunit;

namespace LawnCare.ManagementUI.Tests;

public class CoreApiServiceTests
{
    private readonly Fixture _fixture;
    private readonly Mock<ILogger<CoreApiService>> _loggerMock;
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly HttpClient _httpClient;
    private readonly CoreApiService _coreApiService;

    public CoreApiServiceTests()
    {
        _fixture = new Fixture();
        _loggerMock = new Mock<ILogger<CoreApiService>>();
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("https://localhost:7001")
        };
        _coreApiService = new CoreApiService(_httpClient, _loggerMock.Object);
    }

    [Fact]
    public async Task UpdateJobAsync_WithValidRequest_ShouldReturnUpdatedJob()
    {
        // Arrange
        var jobId = Guid.NewGuid();
        var updateRequest = new UpdateJobRequest
        {
            Status = "InProgress",
            Priority = "Emergency",
            JobCost = 250.00m
        };

        var expectedResponse = CreateServiceRequest();
        var responseJson = JsonSerializer.Serialize(expectedResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Put &&
                    req.RequestUri!.ToString().Contains($"/jobs/{jobId}")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
            });

        // Act
        var result = await _coreApiService.UpdateJobAsync(jobId, updateRequest);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(expectedResponse.Id);
        result.CustomerName.Should().Be(expectedResponse.CustomerName);
        result.Status.Should().Be(expectedResponse.Status);
    }

    [Fact]
    public async Task UpdateJobAsync_WithHttpError_ShouldThrowException()
    {
        // Arrange
        var jobId = Guid.NewGuid();
        var updateRequest = new UpdateJobRequest
        {
            Status = "InProgress"
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = new StringContent("Bad Request", Encoding.UTF8, "application/json")
            });

        // Act & Assert
        var action = async () => await _coreApiService.UpdateJobAsync(jobId, updateRequest);
        await action.Should().ThrowAsync<HttpRequestException>();
    }

    [Fact]
    public async Task UpdateJobAsync_WithNullResponse_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var jobId = Guid.NewGuid();
        var updateRequest = new UpdateJobRequest
        {
            Status = "InProgress"
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("null", Encoding.UTF8, "application/json")
            });

        // Act & Assert
        var action = async () => await _coreApiService.UpdateJobAsync(jobId, updateRequest);
        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("CoreAPI returned null job after update");
    }

    [Fact]
    public async Task UpdateJobAsync_WithNetworkException_ShouldThrowException()
    {
        // Arrange
        var jobId = Guid.NewGuid();
        var updateRequest = new UpdateJobRequest
        {
            Status = "InProgress"
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Network error"));

        // Act & Assert
        var action = async () => await _coreApiService.UpdateJobAsync(jobId, updateRequest);
        await action.Should().ThrowAsync<HttpRequestException>()
            .WithMessage("Network error");
    }

    [Fact]
    public async Task UpdateJobAsync_WithInvalidJson_ShouldThrowJsonException()
    {
        // Arrange
        var jobId = Guid.NewGuid();
        var updateRequest = new UpdateJobRequest
        {
            Status = "InProgress"
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("invalid json", Encoding.UTF8, "application/json")
            });

        // Act & Assert
        var action = async () => await _coreApiService.UpdateJobAsync(jobId, updateRequest);
        await action.Should().ThrowAsync<JsonException>();
    }

    [Fact]
    public async Task UpdateJobAsync_ShouldSendCorrectHttpRequest()
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
            Notes = new List<string> { "Test note" }
        };

        var expectedResponse = CreateServiceRequest();
        var responseJson = JsonSerializer.Serialize(expectedResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        HttpRequestMessage? capturedRequest = null;
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((request, ct) =>
            {
                capturedRequest = request;
            })
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
            });

        // Act
        await _coreApiService.UpdateJobAsync(jobId, updateRequest);

        // Assert
        capturedRequest.Should().NotBeNull();
        capturedRequest!.Method.Should().Be(HttpMethod.Put);
        capturedRequest.RequestUri!.ToString().Should().Be($"https://localhost:7001/jobs/{jobId}");
        capturedRequest.Content.Should().NotBeNull();
        
        var requestContent = await capturedRequest.Content!.ReadAsStringAsync();
        var deserializedRequest = JsonSerializer.Deserialize<UpdateJobRequest>(requestContent, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        });
        
        deserializedRequest.Should().NotBeNull();
        deserializedRequest!.Status.Should().Be(updateRequest.Status);
        deserializedRequest.Priority.Should().Be(updateRequest.Priority);
        deserializedRequest.JobCost.Should().Be(updateRequest.JobCost);
        deserializedRequest.ServiceItems.Should().HaveCount(1);
        deserializedRequest.ServiceItems!.First().ServiceName.Should().Be("Test Service");
        deserializedRequest.Notes.Should().HaveCount(1);
        deserializedRequest.Notes!.First().Should().Be("Test note");
    }

    [Fact]
    public async Task UpdateJobAsync_WithEmptyUpdateRequest_ShouldSendEmptyRequest()
    {
        // Arrange
        var jobId = Guid.NewGuid();
        var updateRequest = new UpdateJobRequest();

        var expectedResponse = CreateServiceRequest();
        var responseJson = JsonSerializer.Serialize(expectedResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        HttpRequestMessage? capturedRequest = null;
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((request, ct) =>
            {
                capturedRequest = request;
            })
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
            });

        // Act
        await _coreApiService.UpdateJobAsync(jobId, updateRequest);

        // Assert
        capturedRequest.Should().NotBeNull();
        var requestContent = await capturedRequest!.Content!.ReadAsStringAsync();
        var deserializedRequest = JsonSerializer.Deserialize<UpdateJobRequest>(requestContent, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        });
        
        deserializedRequest.Should().NotBeNull();
        deserializedRequest!.Status.Should().BeNull();
        deserializedRequest.Priority.Should().BeNull();
        deserializedRequest.JobCost.Should().BeNull();
        deserializedRequest.ServiceItems.Should().BeNull();
        deserializedRequest.Notes.Should().BeNull();
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
            Status = "InProgress",
            Priority = "Emergency",
            AssignedTechnician = "Test Technician",
            EstimatedCost = 250.00m,
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
