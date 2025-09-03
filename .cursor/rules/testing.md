# Testing Rules

## Testing Standards

- Write unit tests for all business logic
- Use AAA pattern (Arrange, Act, Assert)
- Mock external dependencies
- Test multi-tenant scenarios explicitly
- Aim for 80%+ code coverage on business logic

## AI Testing Assistance

- Generate test stubs with AAA pattern automatically
- Suggest test cases based on method behavior

- Help with mocking and test data setup
- Create comprehensive test coverage for all new code

## Test Structure

```csharp
[Fact]
public async Task MethodName_Scenario_ShouldExpectedResult()
{
    // Arrange
    // Setup test data and mocks
    
    // Act
    // Execute the method under test
    

    // Assert
    // Verify the results
}

```

## Multi-Tenant Testing

- Always mock ITenantContext in tests
- Test tenant isolation scenarios

- Verify tenant filtering in database queries
- Test tenant-specific configuration

## Mocking Guidelines

- Mock external dependencies (databases, APIs, services)
- Use Moq for mocking interfaces
- Create test data builders for complex objects
- Use AutoFixture for test data generation when appropriate
