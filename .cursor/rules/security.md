# Security Rules

## Core Security Principles
- Always validate tenant access in controllers and services
- Sanitize all user inputs
- Use parameterized queries to prevent SQL injection
- Implement proper authorization checks
- Never log sensitive information (passwords, tokens, PII)
- Use HTTPS for all communications
- Implement rate limiting per tenant

## Input Validation
- Validate all input parameters
- Sanitize user inputs
- Use proper data validation attributes
- Implement custom validation rules
- Validate tenant access for all operations

## Authentication & Authorization
- Implement role-based authorization (Admin, Manager, Technician, Customer)
- Use proper authentication mechanisms
- Validate user permissions
- Implement tenant-specific access control
- Use secure session management

## Data Protection
- Never log sensitive information
- Use encryption for sensitive data
- Implement proper data masking
- Use secure communication protocols
- Implement proper key management

## API Security
- Include tenant validation in all endpoints
- Implement proper CORS policies
- Use API rate limiting
- Implement proper error handling
- Use secure headers

## Database Security
- Use parameterized queries
- Implement proper access controls
- Use connection encryption
- Implement audit logging
- Use proper backup encryption
