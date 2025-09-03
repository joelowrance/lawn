using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Components;

namespace LawnCare.ManagementUI.Tests;

public class SampleBlazorTests : TestContext
{
    [Fact]
    public void SampleBlazorTest_ShouldPass()
    {
        // Arrange
        var expected = true;

        // Act
        var actual = true;

        // Assert
        actual.Should().Be(expected);
    }

    [Fact]
    public void RenderComponent_ShouldWork()
    {
        // Arrange
        Services.AddSingleton<TestService>();

        // Act
        var component = RenderComponent<TestComponent>();

        // Assert
        component.Should().NotBeNull();
    }

    [Fact]
    public void Bunit_ShouldRenderHtml()
    {
        // Arrange
        var cut = RenderComponent<TestComponent>();

        // Act & Assert
        cut.Markup.Should().Contain("Test Component");
    }
}

// Sample test component for demonstration
public class TestComponent : ComponentBase
{
    protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "div");
        builder.AddContent(1, "Test Component");
        builder.CloseElement();
    }
}

// Sample test service for demonstration
public class TestService
{
    public string GetMessage() => "Hello from test service";
}
