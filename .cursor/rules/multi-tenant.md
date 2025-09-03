# Multi-Tenant Development Rules

## Core Principles
- Always inject ITenantContext in services
- Include tenant filtering in all database queries
- Use tenant-specific configuration
- Implement tenant isolation at the data layer
- Log tenant context with all operations

## AI Assistance
- Help with tenant-specific configuration management
- Suggest tenant isolation patterns for new features
- Remind to include tenant context in all operations

## Tenant Context Pattern
```csharp
public class ServiceController : ControllerBase
{
    private readonly ITenantContext _tenantContext;
    
    public ServiceController(ITenantContext tenantContext)
    {
        _tenantContext = tenantContext;
    }
}
```

## Database Queries
- Always include tenant filtering in queries
- Use global query filters where possible
- Validate tenant access in repositories
- Include tenant context in audit information

## Configuration
- Use tenant-specific configuration values
- Implement tenant isolation at the data layer
- Use tenant-specific connection strings when needed
- Implement tenant-specific caching strategies

## Security
- Always validate tenant access in controllers
- Implement proper authorization checks
- Never allow cross-tenant data access
- Log tenant context with all operations
