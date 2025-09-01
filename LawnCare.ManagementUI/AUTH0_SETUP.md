# Auth0 Setup Instructions

This document provides step-by-step instructions for setting up Auth0 authentication for the Lawn Care Management UI.

## Prerequisites

1. An Auth0 account (sign up at [auth0.com](https://auth0.com))
2. A registered Auth0 application

## Auth0 Application Setup

### 1. Create a New Application

1. Log in to your Auth0 Dashboard
2. Navigate to **Applications** > **Applications**
3. Click **Create Application**
4. Choose **Regular Web Applications** as the application type
5. Click **Create**

### 2. Configure Application Settings

In your Auth0 application settings, configure the following:

#### Allowed Callback URLs
```
https://localhost:5001/callback
http://localhost:5000/callback
https://yourdomain.com/callback
```

#### Allowed Logout URLs
```
https://localhost:5001/logout
http://localhost:5000/logout
https://yourdomain.com/logout
```

#### Allowed Web Origins
```
https://localhost:5001
http://localhost:5000
https://yourdomain.com
```

### 3. Get Your Auth0 Credentials

From your Auth0 application settings, copy the following values:
- **Domain** (e.g., `your-tenant.auth0.com`)
- **Client ID**
- **Client Secret**

## Application Configuration

### 1. Update Configuration Files

Update the following files with your Auth0 credentials:

#### appsettings.json
```json
{
  "Auth0": {
    "Domain": "your-tenant.auth0.com",
    "ClientId": "your-client-id",
    "ClientSecret": "your-client-secret",
    "Audience": "your-api-identifier"
  }
}
```

#### appsettings.Development.json
```json
{
  "Auth0": {
    "Domain": "your-tenant.auth0.com",
    "ClientId": "your-client-id",
    "ClientSecret": "your-client-secret",
    "Audience": "your-api-identifier"
  }
}
```

### 2. Environment Variables (Optional)

For production deployments, consider using environment variables instead of storing secrets in configuration files:

```bash
export Auth0__Domain="your-tenant.auth0.com"
export Auth0__ClientId="your-client-id"
export Auth0__ClientSecret="your-client-secret"
export Auth0__Audience="your-api-identifier"
```

## User Management

### 1. Create Test Users

1. In your Auth0 Dashboard, navigate to **User Management** > **Users**
2. Click **Create User**
3. Fill in the user details
4. Set a temporary password
5. Click **Create**

### 2. Configure User Roles (Optional)

1. Navigate to **User Management** > **Roles**
2. Create roles such as:
   - `Admin` - Full system access
   - `Manager` - Management access
   - `Technician` - Field technician access
   - `Customer` - Customer portal access

3. Assign roles to users in **User Management** > **Users** > Select User > **Roles**

## API Configuration (Optional)

If you plan to use Auth0 for API authentication:

1. Navigate to **Applications** > **APIs**
2. Click **Create API**
3. Set the **Identifier** (this becomes your Audience)
4. Configure scopes and permissions as needed

## Testing the Integration

1. Start your application:
   ```bash
   dotnet run
   ```

2. Navigate to `https://localhost:5001`
3. You should be redirected to the login page
4. Click "Sign In with Auth0"
5. You'll be redirected to Auth0's login page
6. Enter your test user credentials
7. After successful authentication, you'll be redirected back to the application

## Troubleshooting

### Common Issues

1. **Redirect URI Mismatch**
   - Ensure the callback URLs in Auth0 match your application URLs
   - Check for trailing slashes and protocol (http vs https)

2. **Invalid Client Secret**
   - Verify the client secret is correctly copied from Auth0
   - Ensure no extra spaces or characters

3. **Domain Configuration**
   - Verify the domain is correctly formatted (e.g., `tenant.auth0.com`)
   - Don't include `https://` in the domain configuration

4. **CORS Issues**
   - Ensure Allowed Web Origins includes your application URL
   - Check that the protocol matches (http vs https)

### Debug Mode

To enable detailed Auth0 logging, add the following to your `appsettings.Development.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Auth0": "Debug",
      "Microsoft.AspNetCore.Authentication": "Debug"
    }
  }
}
```

## Security Considerations

1. **Never commit secrets to version control**
2. **Use environment variables in production**
3. **Regularly rotate client secrets**
4. **Enable MFA for Auth0 dashboard access**
5. **Configure proper session timeouts**
6. **Use HTTPS in production**

## Multi-Tenant Considerations

For multi-tenant applications:

1. Consider using Auth0 Organizations
2. Implement tenant-specific user management
3. Configure tenant-specific callback URLs
4. Use custom claims for tenant identification

## Support

For Auth0-specific issues:
- [Auth0 Documentation](https://auth0.com/docs)
- [Auth0 Community](https://community.auth0.com)
- [Auth0 Support](https://support.auth0.com)

For application-specific issues, refer to the main project documentation.
