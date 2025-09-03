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

public class SubmitEstimateSimpleTests : IDisposable
{
    private readonly Fixture _fixture;
    private readonly CoreDbContext _dbContext;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<CoreDbContext>> _loggerMock;
    private readonly SubmitEstimateCommandHandler _handler;

    public SubmitEstimateSimpleTests()
    {
        _fixture = new Fixture();
        
        // Configure AutoFixture to handle complex types
        _fixture.Customize<EmailAddress>(c => c.FromFactory(() => new EmailAddress(_fixture.Create<string>() + "@example.com")));
        _fixture.Customize<PhoneNumber>(c => c.FromFactory(() => new PhoneNumber("5552345678")));
        _fixture.Customize<Postcode>(c => c.FromFactory(() => new Postcode("12345")));
        _fixture.Customize<Money>(c => c.FromFactory(() => new Money(_fixture.Create<decimal>())));
        
        // Configure Customer creation to use proper constructor
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
            
        _loggerMock = new Mock<ILogger<CoreDbContext>>();
        _dbContext = new CoreDbContext(options, _loggerMock.Object);
        
        // Setup UnitOfWork mock to call real DbContext SaveChangesAsync
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(async (CancellationToken ct) => await _dbContext.SaveChangesAsync(ct));
        
        _handler = new SubmitEstimateCommandHandler(_unitOfWorkMock.Object, _dbContext);
    }

    [Fact]
    public async Task Handle_WithNewCustomerAndLocation_ShouldReturnValidResult()
    {
        // Arrange
        var estimate = CreateValidEstimate();
        var command = new SubmitEstimateCommand(estimate);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.JobId.Should().NotBeNull();
        result.CustomerName.Should().Be($"{estimate.CustomerFirstName} {estimate.CustomerLastName}");
        result.PropertyAddress.Should().Be($"{estimate.CustomerAddress1}, {estimate.CustomerCity}, {estimate.CustomerState} {estimate.CustomerZip}");

        // Verify UnitOfWork was called
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithExistingCustomer_ShouldReturnValidResult()
    {
        // Arrange
        var existingCustomer = _fixture.Create<Customer>();
        _dbContext.Customers.Add(existingCustomer);
        await _dbContext.SaveChangesAsync();

        var estimate = CreateValidEstimate();
        estimate.CustomerEmail = existingCustomer.Email.Value;
        estimate.CustomerCellPhone = existingCustomer.CellPhone.Value;
        
        var command = new SubmitEstimateCommand(estimate);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.JobId.Should().NotBeNull();
        result.CustomerName.Should().Be($"{existingCustomer.FirstName} {existingCustomer.LastName}");

        // Verify UnitOfWork was called
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithEmptyServices_ShouldReturnValidResult()
    {
        // Arrange
        var estimate = CreateValidEstimate();
        estimate.Services.Clear();
        
        var command = new SubmitEstimateCommand(estimate);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.JobId.Should().NotBeNull();

        // Verify UnitOfWork was called
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithEmptyDescription_ShouldReturnValidResult()
    {
        // Arrange
        var estimate = CreateValidEstimate();
        estimate.Description = string.Empty;
        
        var command = new SubmitEstimateCommand(estimate);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.JobId.Should().NotBeNull();

        // Verify UnitOfWork was called
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithMultipleServices_ShouldReturnValidResult()
    {
        // Arrange
        var estimate = CreateValidEstimate();
        estimate.Services = new List<JobServiceItem>
        {
            new() { ServiceName = "Lawn Mowing", Cost = 50.00m, Notes = "Weekly service" },
            new() { ServiceName = "Fertilization", Cost = 75.00m, Notes = "Spring treatment" },
            new() { ServiceName = "Weed Control", Cost = 30.00m, Notes = "Spot treatment" }
        };
        
        var command = new SubmitEstimateCommand(estimate);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.JobId.Should().NotBeNull();

        // Verify UnitOfWork was called
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithScheduledDate_ShouldReturnValidResult()
    {
        // Arrange
        var localTime = new DateTimeOffset(2024, 6, 15, 14, 30, 0, TimeSpan.FromHours(-5)); // EST
        var estimate = CreateValidEstimate();
        estimate.ScheduledDate = localTime;
        
        var command = new SubmitEstimateCommand(estimate);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.JobId.Should().NotBeNull();

        // Verify UnitOfWork was called
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithZeroEstimatedCost_ShouldReturnValidResult()
    {
        // Arrange
        var estimate = CreateValidEstimate();
        estimate.EstimatedCost = 0m;
        
        var command = new SubmitEstimateCommand(estimate);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.JobId.Should().NotBeNull();

        // Verify UnitOfWork was called
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithLargeEstimatedCost_ShouldReturnValidResult()
    {
        // Arrange
        var estimate = CreateValidEstimate();
        estimate.EstimatedCost = 9999.99m;
        
        var command = new SubmitEstimateCommand(estimate);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.JobId.Should().NotBeNull();

        // Verify UnitOfWork was called
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCallUnitOfWorkSaveChanges()
    {
        // Arrange
        var estimate = CreateValidEstimate();
        var command = new SubmitEstimateCommand(estimate);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassTokenToUnitOfWork()
    {
        // Arrange
        var estimate = CreateValidEstimate();
        var command = new SubmitEstimateCommand(estimate);
        var cancellationToken = new CancellationToken();

        // Act
        await _handler.Handle(command, cancellationToken);

        // Assert
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(cancellationToken), Times.Once);
    }

    private JobEstimate CreateValidEstimate()
    {
        return new JobEstimate
        {
            UserId = _fixture.Create<string>(),
            CustomerFirstName = "John",
            CustomerLastName = "Doe",
            CustomerAddress1 = "123 Main St",
            CustomerAddress2 = "Apt 4B",
            CustomerAddress3 = "",
            CustomerCity = "Anytown",
            CustomerState = "NY",
            CustomerZip = "12345",
            CustomerHomePhone = "5552345678",
            CustomerCellPhone = "5559876543",
            CustomerEmail = "john.doe@example.com",
            ScheduledDate = DateTimeOffset.Now.AddDays(7),
            EstimatedCost = 150.00m,
            EstimatedDuration = 120,
            Description = "Weekly lawn maintenance service",
            Services = new List<JobServiceItem>
            {
                new() { ServiceName = "Lawn Mowing", Cost = 75.00m, Notes = "Regular mowing" },
                new() { ServiceName = "Edging", Cost = 25.00m, Notes = "Edge trimming" },
                new() { ServiceName = "Cleanup", Cost = 50.00m, Notes = "Debris removal" }
            }
        };
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}
