# Test Projects

This folder contains test projects for the FictionalLawnCare solution.

## Test Projects

### LawnCare.CoreApi.Tests
- **Purpose**: Unit and integration tests for the Core API
- **Framework**: xUnit
- **Key Dependencies**:
  - `Microsoft.AspNetCore.Mvc.Testing` - For integration testing
  - `Microsoft.EntityFrameworkCore.InMemory` - For database testing
  - `Moq` - For mocking dependencies
  - `FluentAssertions` - For readable assertions
  - `AutoFixture` - For test data generation

### LawnCare.ManagementUI.Tests
- **Purpose**: Unit and integration tests for the Blazor Management UI
- **Framework**: xUnit
- **Key Dependencies**:
  - `bunit` - For Blazor component testing
  - `bunit.web` - For web-specific Blazor testing
  - `Microsoft.AspNetCore.Mvc.Testing` - For integration testing
  - `Moq` - For mocking dependencies
  - `FluentAssertions` - For readable assertions
  - `AutoFixture` - For test data generation

## Running Tests

### Run all tests
```bash
dotnet test
```

### Run specific test project
```bash
dotnet test tests/LawnCare.CoreApi.Tests/
dotnet test tests/LawnCare.ManagementUI.Tests/
```

### Run tests with coverage
```bash
dotnet test --collect:"XPlat Code Coverage"
```

## Testing Guidelines

### Multi-Tenant Testing
- Always mock `ITenantContext` in unit tests
- Use tenant-specific test data
- Validate tenant isolation in integration tests

### API Testing
- Use `WebApplicationFactory<T>` for integration tests
- Test authentication and authorization
- Validate tenant context in API endpoints

### Blazor Component Testing
- Use `bunit` for component testing
- Mock services and dependencies
- Test component interactions and state changes

### Database Testing
- Use `Microsoft.EntityFrameworkCore.InMemory` for unit tests
- Use test databases for integration tests
- Clean up test data after each test

## Sample Test Structure

```csharp
public class SampleTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public SampleTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task ApiEndpoint_ShouldReturnExpectedResult()
    {
        // Arrange
        var client = _factory.CreateClient();
        
        // Act
        var response = await client.GetAsync("/api/endpoint");
        
        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
    }
}
```

