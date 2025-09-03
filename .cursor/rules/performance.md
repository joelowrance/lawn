# Performance Optimization Rules

## Core Principles
- Use async/await consistently for I/O operations
- Implement caching strategies per tenant
- Optimize database queries with proper indexing
- Use connection pooling for database access
- Monitor tenant resource usage

## AI Performance Assistance
- Automatically suggest async/await patterns for I/O operations
- Recommend caching strategies per tenant
- Help optimize database queries and suggest indexing

## Async/Await Patterns
- Use async/await for all I/O operations
- Avoid blocking calls in async methods
- Use ConfigureAwait(false) in library code
- Implement proper cancellation token support

## Caching Strategies
- Implement tenant-specific caching
- Use Redis for distributed caching
- Cache frequently accessed data
- Implement cache invalidation strategies

## Database Optimization
- Use proper indexing for performance
- Optimize queries with proper filtering
- Use connection pooling
- Implement read replicas for reporting

## Monitoring
- Monitor tenant resource usage
- Track performance metrics per tenant
- Implement health checks
- Use structured logging for performance data
