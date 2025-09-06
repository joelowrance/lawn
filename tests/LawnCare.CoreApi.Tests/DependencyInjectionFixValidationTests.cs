using FluentAssertions;
using LawnCare.CoreApi.UseCases;
using Xunit;

namespace LawnCare.CoreApi.Tests;

/// <summary>
/// Tests to validate that the specific DI fix we made is working correctly.
/// These tests ensure that the SearchJobsQueryHandler uses IJobMappingService interface.
/// </summary>
public class DependencyInjectionFixValidationTests
{
    [Fact]
    public void SearchJobsQueryHandler_Constructor_ShouldAcceptIJobMappingService()
    {
        // Arrange
        var handlerType = typeof(SearchJobsQueryHandler);
        var constructor = handlerType.GetConstructors().First();

        // Act
        var parameters = constructor.GetParameters();

        // Assert
        var mappingServiceParameter = parameters.FirstOrDefault(p => 
            p.ParameterType == typeof(IJobMappingService));

        mappingServiceParameter.Should().NotBeNull(
            "SearchJobsQueryHandler constructor should accept IJobMappingService parameter");
        mappingServiceParameter!.ParameterType.Should().Be(typeof(IJobMappingService),
            "SearchJobsQueryHandler should use IJobMappingService interface, not concrete JobMappingService");
    }

    [Fact]
    public void SearchJobsQueryHandler_Constructor_ShouldNotAcceptConcreteJobMappingService()
    {
        // Arrange
        var handlerType = typeof(SearchJobsQueryHandler);
        var constructor = handlerType.GetConstructors().First();

        // Act
        var parameters = constructor.GetParameters();

        // Assert
        var concreteMappingServiceParameter = parameters.FirstOrDefault(p => 
            p.ParameterType == typeof(JobMappingService));

        concreteMappingServiceParameter.Should().BeNull(
            "SearchJobsQueryHandler constructor should NOT accept concrete JobMappingService parameter. " +
            "It should use IJobMappingService interface instead.");
    }

    [Fact]
    public void UpdateJobCommandHandler_Constructor_ShouldAcceptIJobMappingService()
    {
        // Arrange
        var handlerType = typeof(UpdateJobCommandHandler);
        var constructor = handlerType.GetConstructors().First();

        // Act
        var parameters = constructor.GetParameters();

        // Assert
        var mappingServiceParameter = parameters.FirstOrDefault(p => 
            p.ParameterType == typeof(IJobMappingService));

        mappingServiceParameter.Should().NotBeNull(
            "UpdateJobCommandHandler constructor should accept IJobMappingService parameter");
        mappingServiceParameter!.ParameterType.Should().Be(typeof(IJobMappingService),
            "UpdateJobCommandHandler should use IJobMappingService interface, not concrete JobMappingService");
    }

    [Fact]
    public void UpdateJobCommandHandler_Constructor_ShouldNotAcceptConcreteJobMappingService()
    {
        // Arrange
        var handlerType = typeof(UpdateJobCommandHandler);
        var constructor = handlerType.GetConstructors().First();

        // Act
        var parameters = constructor.GetParameters();

        // Assert
        var concreteMappingServiceParameter = parameters.FirstOrDefault(p => 
            p.ParameterType == typeof(JobMappingService));

        concreteMappingServiceParameter.Should().BeNull(
            "UpdateJobCommandHandler constructor should NOT accept concrete JobMappingService parameter. " +
            "It should use IJobMappingService interface instead.");
    }

    [Fact]
    public void IJobMappingService_ShouldBeDefined()
    {
        // Arrange & Act
        var interfaceType = typeof(IJobMappingService);

        // Assert
        interfaceType.Should().NotBeNull("IJobMappingService interface should be defined");
        interfaceType.IsInterface.Should().BeTrue("IJobMappingService should be an interface");
    }

    [Fact]
    public void JobMappingService_ShouldImplementIJobMappingService()
    {
        // Arrange
        var concreteType = typeof(JobMappingService);
        var interfaceType = typeof(IJobMappingService);

        // Act & Assert
        interfaceType.IsAssignableFrom(concreteType).Should().BeTrue(
            "JobMappingService should implement IJobMappingService interface");
    }

    [Fact]
    public void Handlers_ShouldHaveCorrectConstructorSignature()
    {
        // Arrange
        var searchHandlerType = typeof(SearchJobsQueryHandler);
        var updateHandlerType = typeof(UpdateJobCommandHandler);

        // Act
        var searchConstructor = searchHandlerType.GetConstructors().First();
        var updateConstructor = updateHandlerType.GetConstructors().First();

        // Assert
        var searchParams = searchConstructor.GetParameters();
        var updateParams = updateConstructor.GetParameters();

        // Both handlers should have similar constructor signatures
        searchParams.Length.Should().BeGreaterThan(0, "SearchJobsQueryHandler should have constructor parameters");
        updateParams.Length.Should().BeGreaterThan(0, "UpdateJobCommandHandler should have constructor parameters");

        // Both should have IJobMappingService parameter
        var searchMappingParam = searchParams.FirstOrDefault(p => p.ParameterType == typeof(IJobMappingService));
        var updateMappingParam = updateParams.FirstOrDefault(p => p.ParameterType == typeof(IJobMappingService));

        searchMappingParam.Should().NotBeNull("SearchJobsQueryHandler should have IJobMappingService parameter");
        updateMappingParam.Should().NotBeNull("UpdateJobCommandHandler should have IJobMappingService parameter");
    }
}
