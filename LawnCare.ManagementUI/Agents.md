# Agents.MD - Lawn Care & Tree Service Management Platform

## Project Overview
This is a distributed, multi-tenant SaaS application serving lawn care and tree specialist companies. The system provides comprehensive business management tools including scheduling, customer management, invoicing, and service tracking.

## Architecture
- **Pattern**: Multi-tenant distributed architecture
- **Primary Language**: C# (.NET 8+)
- **Frontend**: Blazor Server/WebAssembly for Management UI
- **Database**: SQL Server with tenant isolation
- **Messaging**: Azure Service Bus / RabbitMQ
- **Caching**: Redis
- **Authentication**: Azure AD B2C / IdentityServer

## Tenant Architecture Guidelines

### Tenant Isolation Strategy
- Use tenant-per-schema approach for data isolation
- Implement tenant context middleware for request routing
- Ensure all database queries include tenant filtering
- Use tenant-specific configuration management

### Key Patterns
```csharp
// Always inject ITenantContext
public class ServiceController : ControllerBase
{
    private readonly ITenantContext _tenantContext;
    
    public ServiceController(ITenantContext tenantContext)
    {
        _tenantContext = tenantContext;
    }
}
```

## Domain Model Context

### Core Business Entities
- **Customer**: Property owners requiring lawn/tree services
- **Property**: Service locations with specific requirements
- **ServiceRequest**: Work orders for lawn care or tree services
- **Technician**: Field workers performing services
- **Route**: Optimized service scheduling and routing
- **Invoice**: Billing and payment tracking
- **Equipment**: Tools and machinery management

### Service Types
- **Lawn Care**: Mowing, fertilization, weed control, aeration
- **Tree Services**: Pruning, removal, disease treatment, planting
- **Seasonal Services**: Leaf cleanup, snow removal, holiday lighting

## Management UI Guidelines

### Component Architecture
- Use Blazor components with clear separation of concerns
- Implement base classes for common CRUD operations
- Follow the Repository + Unit of Work pattern
- Do not use AutoMapper for DTO mapping, find something else.

### UI/UX Patterns
- **Dashboard**: Real-time metrics, weather integration, daily schedules
- **Calendar Views**: Drag-drop scheduling, route optimization
- **Customer Management**: Property details, service history, preferences
- **Mobile-First**: Responsive design for field technicians
- **Offline Capability**: Essential for field operations

### State Management
```csharp
// Use Fluxor for complex state management
public class ScheduleState
{
    public ImmutableList<ServiceRequest> Requests { get; init; }
    public DateTime SelectedDate { get; init; }
    public bool IsLoading { get; init; }
}
```

## Coding Standards

### Naming Conventions
- Use PascalCase for public members
- Use camelCase for private fields and parameters
- Prefix interfaces with 'I'
- Use meaningful, domain-specific names

### Project Structure
```
src/
├── LawnCare.Core/              # Domain entities and interfaces
├── LawnCare.Infrastructure/    # Data access, external services
├── LawnCare.Application/       # Business logic, CQRS handlers
├── LawnCare.Web/               # Blazor UI components
├── LawnCare.CoreApi/           # REST API controllers
├── LawnCare.Shared/            # DTOs, shared models
└── LawnCare.Tests/             # Unit and integration tests
```

### Exception Handling
- Use domain-specific exceptions
- Implement global exception handling middleware
- Log tenant context with all errors
- Return user-friendly error messages

### Security Considerations
- Always validate tenant access in controllers
- Implement role-based authorization (Admin, Manager, Technician, Customer)
- Sanitize all user inputs
- Use HTTPS for all communications
- Implement rate limiting per tenant

## Common Development Tasks

### Adding New Management Features
1. Create domain entity in Core project
2. Add repository interface and implementation
3. Create CQRS commands/queries
4. Build Blazor components with proper validation
5. Add authorization policies
6. Write unit tests with tenant isolation

### Database Migrations
- Always include tenant context in migration scripts
- Test migrations against multiple tenant schemas
- Document breaking changes and rollback procedures

### API Development
- Follow RESTful conventions
- Include tenant validation in all endpoints
- Use consistent response formats
- Implement proper HTTP status codes
- Add Swagger documentation

## Testing Guidelines

### Unit Tests
- Mock tenant context in all tests
- Use AutoFixture for test data generation
- Test multi-tenant scenarios explicitly
- Achieve 80%+ code coverage

### Integration Tests
- Test with multiple tenant configurations
- Validate data isolation between tenants
- Test authentication and authorization flows

## Performance Considerations
- Implement caching strategies per tenant
- Use async/await consistently
- Optimize database queries with proper indexing
- Consider read replicas for reporting
- Monitor tenant resource usage

## Deployment & DevOps
- Use Docker containers for consistent environments
- Implement blue-green deployments
- Monitor tenant-specific metrics
- Automate database schema updates
- Use feature flags for gradual rollouts

## Business Logic Helpers

When implementing features, consider these domain-specific requirements:
- **Seasonal Variations**: Adjust service frequencies based on growing seasons
- **Weather Integration**: Cancel/reschedule based on weather conditions
- **Equipment Tracking**: Monitor maintenance schedules and availability
- **Route Optimization**: Minimize travel time between properties
- **Regulatory Compliance**: Track pesticide applications and certifications

## Emergency Procedures
- Implement circuit breakers for external service calls
- Have fallback procedures for critical operations
- Maintain separate monitoring per tenant
- Document incident response procedures

## Learning Resources
- Focus on distributed system patterns
- Study multi-tenancy best practices
- Learn about lawn care industry workflows
- Understand seasonal business operations
- Explore route optimization algorithms

---

*This file should be updated as the architecture evolves and new patterns are established.*