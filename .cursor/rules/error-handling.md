# Error Handling Rules

## Core Principles
- NEVER catch generic exceptions (catch (Exception ex))
- Always use Result<T> pattern for business logic operations
- Use ProblemDetails middleware for Web API error responses
- Log all exceptions using Serilog with structured logging
- Include tenant context in all log entries
- Use specific exception types for different error scenarios

## AI Assistance
- Automatically suggest Result<T> wrappers for business logic methods
- Generate try-catch blocks with specific exception types for infrastructure code
- Be very proactive about warning about potential issues before they happen

## Result Pattern Implementation
```csharp
public class Result<T>
{
    public bool IsSuccess { get; }
    public T Value { get; }
    public string Error { get; }
    
    private Result(bool isSuccess, T value, string error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }
    
    public static Result<T> Success(T value) => new(true, value, null);
    public static Result<T> Failure(string error) => new(false, default, error);
}
```

## Exception Handling Patterns
```csharp
// Good: Specific exception handling
try
{
    var result = await _service.ProcessAsync(request);
    return result;
}
catch (ValidationException ex)
{
    _logger.LogWarning("Validation failed for tenant {TenantId}: {Error}", 
        _tenantContext.TenantId, ex.Message);
    return Result.Failure(ex.Message);
}

// Bad: Generic exception handling
try
{
    // code
}
catch (Exception ex) // DON'T DO THIS
{
    // handling
}
```

## Logging Pattern
```csharp
_logger.LogInformation("Processing service request {RequestId} for tenant {TenantId}", 
    request.Id, _tenantContext.TenantId);
```
