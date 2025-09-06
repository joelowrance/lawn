using FluentAssertions;
using LawnCare.CoreApi.UseCases;
using Microsoft.Extensions.Logging;
using System.Reflection;
using Xunit;

namespace LawnCare.CoreApi.Tests;

/// <summary>
/// Simple tests to ensure that MediatR handlers use interface dependencies instead of concrete implementations.
/// This prevents the specific DI registration issue that occurred with SearchJobsQueryHandler.
/// </summary>
public class HandlerDependencyValidationSimpleTests
{
    [Fact]
    public void SearchJobsQueryHandler_ShouldUseIJobMappingServiceInterface()
    {
        // Arrange
        var handlerType = typeof(SearchJobsQueryHandler);
        var constructor = GetPrimaryConstructor(handlerType);

        // Act & Assert
        constructor.Should().NotBeNull("SearchJobsQueryHandler should have a constructor");

        var parameters = constructor!.GetParameters();
        var mappingServiceParameter = parameters.FirstOrDefault(p => 
            p.ParameterType == typeof(IJobMappingService) || 
            p.ParameterType == typeof(JobMappingService));

        mappingServiceParameter.Should().NotBeNull("SearchJobsQueryHandler should have a mapping service parameter");
        mappingServiceParameter!.ParameterType.Should().Be(typeof(IJobMappingService),
            "SearchJobsQueryHandler should inject IJobMappingService interface, not JobMappingService concrete class");
    }

    [Fact]
    public void UpdateJobCommandHandler_ShouldUseIJobMappingServiceInterface()
    {
        // Arrange
        var handlerType = typeof(UpdateJobCommandHandler);
        var constructor = GetPrimaryConstructor(handlerType);

        // Act & Assert
        constructor.Should().NotBeNull("UpdateJobCommandHandler should have a constructor");

        var parameters = constructor!.GetParameters();
        var mappingServiceParameter = parameters.FirstOrDefault(p => 
            p.ParameterType == typeof(IJobMappingService) || 
            p.ParameterType == typeof(JobMappingService));

        mappingServiceParameter.Should().NotBeNull("UpdateJobCommandHandler should have a mapping service parameter");
        mappingServiceParameter!.ParameterType.Should().Be(typeof(IJobMappingService),
            "UpdateJobCommandHandler should inject IJobMappingService interface, not JobMappingService concrete class");
    }

    [Fact]
    public void NoHandler_ShouldDependOnConcreteJobMappingService()
    {
        // Arrange
        var assembly = typeof(SearchJobsQueryHandler).Assembly;
        var handlerTypes = GetMediatRHandlerTypes(assembly);

        // Act & Assert
        foreach (var handlerType in handlerTypes)
        {
            var constructor = GetPrimaryConstructor(handlerType);
            if (constructor == null) continue;

            var parameters = constructor.GetParameters();
            var concreteMappingServiceParam = parameters.FirstOrDefault(p => 
                p.ParameterType == typeof(JobMappingService));

            concreteMappingServiceParam.Should().BeNull(
                $"Handler {handlerType.Name} should not depend on concrete JobMappingService. " +
                "Use IJobMappingService interface instead.");
        }
    }

    [Fact]
    public void AllHandlers_ShouldUseInterfaceDependencies()
    {
        // Arrange
        var assembly = typeof(SearchJobsQueryHandler).Assembly;
        var handlerTypes = GetMediatRHandlerTypes(assembly);

        // Act & Assert
        foreach (var handlerType in handlerTypes)
        {
            var constructor = GetPrimaryConstructor(handlerType);
            if (constructor == null) continue;

            var parameters = constructor.GetParameters();
            foreach (var parameter in parameters)
            {
                // Skip primitive types, strings, and built-in types
                if (IsPrimitiveOrBuiltInType(parameter.ParameterType)) continue;

                // Check if the parameter type is a concrete class that should be an interface
                if (ShouldBeInterface(parameter.ParameterType))
                {
                    // Find the corresponding interface
                    var interfaceType = FindInterfaceForConcreteType(parameter.ParameterType);
                    
                    interfaceType.Should().NotBeNull(
                        $"Handler {handlerType.Name} constructor parameter '{parameter.Name}' " +
                        $"of type {parameter.ParameterType.Name} should use an interface instead of concrete implementation. " +
                        $"Consider using {interfaceType?.Name ?? "appropriate interface"}.");
                }
            }
        }
    }

    [Fact]
    public void Handlers_ShouldHaveRequiredConstructorParameters()
    {
        // Arrange
        var assembly = typeof(SearchJobsQueryHandler).Assembly;
        var handlerTypes = GetMediatRHandlerTypes(assembly);

        // Act & Assert
        foreach (var handlerType in handlerTypes)
        {
            var constructor = GetPrimaryConstructor(handlerType);
            constructor.Should().NotBeNull($"Handler {handlerType.Name} should have a constructor");

            var parameters = constructor!.GetParameters();
            parameters.Should().NotBeEmpty($"Handler {handlerType.Name} should have constructor parameters");
        }
    }

    private static List<Type> GetMediatRHandlerTypes(Assembly assembly)
    {
        return assembly.GetTypes()
            .Where(t => !t.IsAbstract && !t.IsInterface)
            .Where(t => t.GetInterfaces().Any(i => 
                i.IsGenericType && 
                (i.GetGenericTypeDefinition() == typeof(MediatR.IRequestHandler<,>) ||
                 i.GetGenericTypeDefinition() == typeof(MediatR.IRequestHandler<>))))
            .ToList();
    }

    private static ConstructorInfo? GetPrimaryConstructor(Type type)
    {
        var constructors = type.GetConstructors();
        if (constructors.Length == 0) return null;

        // Return the constructor with the most parameters (usually the main one)
        return constructors.OrderByDescending(c => c.GetParameters().Length).First();
    }

    private static bool IsPrimitiveOrBuiltInType(Type type)
    {
        return type.IsPrimitive ||
               type == typeof(string) ||
               type == typeof(DateTime) ||
               type == typeof(DateTimeOffset) ||
               type == typeof(TimeSpan) ||
               type == typeof(Guid) ||
               type == typeof(decimal) ||
               type == typeof(CancellationToken) ||
               type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ILogger<>);
    }

    private static bool ShouldBeInterface(Type type)
    {
        // Check if this is a concrete class that has a corresponding interface
        if (type.IsInterface || type.IsAbstract) return false;
        
        // Check if there's a corresponding interface
        var interfaceName = "I" + type.Name;
        var assembly = type.Assembly;
        var interfaceType = assembly.GetType(type.Namespace + "." + interfaceName);
        
        return interfaceType != null;
    }

    private static Type? FindInterfaceForConcreteType(Type concreteType)
    {
        var interfaceName = "I" + concreteType.Name;
        var assembly = concreteType.Assembly;
        return assembly.GetType(concreteType.Namespace + "." + interfaceName);
    }
}
