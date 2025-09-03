# Code Generation & Snippets Rules

## Code Generation
- Generate boilerplate code (constructors, properties, basic CRUD methods)
- Automatically implement interface methods
- Create unit test stubs and basic test cases
- Generate custom snippets for common patterns (Result<T>, tenant services, etc.)
- Use both custom and standard C# snippets
- Create service classes with proper dependency injection
- Generate repository patterns with tenant isolation

## Snippet Usage
Type these prefixes and press Tab:
- `result` → Result<T> pattern
- `tenantservice` → Tenant-aware service
- `tenantrepo` → Repository with tenant isolation
- `testaaa` → Unit test with AAA pattern
- `tenantcontroller` → API controller
- `seasonal` → Seasonal service logic
- `equipment` → Equipment management

## Service Generation
- Create service classes with proper dependency injection
- Include ITenantContext injection
- Add proper logging
- Implement Result<T> pattern
- Include error handling

## Repository Generation
- Generate repository patterns with tenant isolation
- Include proper query filtering
- Add CRUD operations
- Implement async patterns
- Include proper error handling

## Controller Generation
- Create API controllers with tenant context
- Include proper authorization
- Implement Result<T> pattern
- Add proper error responses
- Include Swagger documentation
