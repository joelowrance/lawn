using FluentAssertions;
using FluentAssertions.Primitives;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using LawnCare.CoreApi.Infrastructure.Database;
using LawnCare.Shared;
using AutoFixture;

namespace LawnCare.CoreApi.Tests;

public class SampleTests
{
    [Fact]
    public void SampleTest_ShouldPass()
    {
        // Arrange
        var expected = true;

        // Act
        var actual = true;

        // Assert
        actual.Should().Be(expected);
    }

    [Fact]
    public void FluentAssertions_ShouldWork()
    {
        // Arrange
        var numbers = new[] { 1, 2, 3, 4, 5 };

        // Act & Assert
        numbers.Should().HaveCount(5);
        numbers.Should().Contain(3);
        numbers.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void AutoFixture_ShouldGenerateData()
    {
        // Arrange
        var fixture = new AutoFixture.Fixture();

        // Act
        var stringValue = fixture.Create<string>();
        var intValue = fixture.Create<int>();

        // Assert
        stringValue.Should().NotBeNullOrEmpty();
        intValue.Should().BeGreaterThan(0);
    }
}
