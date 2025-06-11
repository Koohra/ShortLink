# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

ShortLink is a URL shortening service built with .NET 10 following Clean Architecture principles. The application uses CQRS pattern with MediatR, implements caching with Redis, and provides a REST API for link management and statistics.

### Architecture Layers

- **ShortLink.Domain**: Core business entities, value objects, and interfaces
- **ShortLink.Application**: CQRS commands/queries, DTOs, and application logic
- **ShortLink.Infrastructure**: Data persistence, external services, caching implementation  
- **ShortLink.WebAPI**: REST API controllers, middleware, and configuration

## Key Architectural Patterns

### CQRS Implementation
- Commands and Queries are separated in `ShortLink.Application/Commands` and `ShortLink.Application/Queries`
- Uses custom `ICommandHandler<TCommand, TResponse>` and `IQueryHandler<TQuery, TResponse>` interfaces
- MediatR is configured but the project uses custom handler interfaces

### Domain-Driven Design
- `Link` entity in Domain layer with factory method `Create()` and business logic
- `ShortCode` value object with validation and creation logic
- Repository pattern with `ILinkRepository` and `IUnitOfWork` interfaces

### Caching Strategy
- Redis cache (`ILinkCache`) stores original URLs and click counts
- `RedirectMiddleware` checks cache first, falls back to database
- Click counting is primarily done in Redis for performance

## Development Commands

### Build and Run
```bash
# Build entire solution
dotnet build

# Run the API (from ShortLink.WebAPI directory)
dotnet run

# Run with specific environment
dotnet run --environment Development
```

### Database Management
```bash
# Add new migration (from ShortLink.WebAPI directory)
dotnet ef migrations add MigrationName

# Update database
dotnet ef database update

# Generate SQL script
dotnet ef migrations script
```

### Docker Development
```bash
# Build and run with Docker Compose
docker-compose up -d

# Build only the API container
docker build -t shortlink-api .

# View logs
docker-compose logs -f api
```

## Configuration

### Connection Strings
- **DefaultConnection**: SQL Server database connection
- **Redis**: Redis cache connection string

### AppSettings
- **BaseUrl**: Base URL for generating short links (e.g., "http://localhost:5062/")

### Redis Configuration
- **KeyPrefix**: "shortlink" - prefix for all Redis keys
- **DefaultExpiryHours**: 24 - default cache expiration
- **StatsExpiryDays**: 30 - statistics cache duration

## Key Components

### RedirectMiddleware
Custom middleware that handles short URL redirects:
- Intercepts requests to root paths (excludes /api/, /swagger, etc.)
- Checks Redis cache first for performance
- Falls back to database if not cached
- Automatically caches database results
- Handles link expiration and 404 scenarios

### Service Registration
Extension methods in `ShortLink.WebAPI/Extensions/` handle dependency injection:
- `AddShortLinkServices()`: Main service registration
- `AddDatabase()`: Entity Framework configuration
- `AddRedisCache()`: Redis cache setup
- `AddInfrastructure()`: Repository and service registration
- `AddApplication()`: MediatR and handlers

### Health Checks
Configured for both database and Redis with endpoints at `/health`

## API Endpoints

### Links Controller
- `POST /api/links` - Create shortened link
- `GET /api/links/{code}` - Get link details by short code
- `GET /api/links?count=n` - List recent links
- `DELETE /api/links/{code}` - Delete link (planned, not implemented)

### Stats Controller  
- `GET /api/stats/{code}` - Get link statistics including click counts

### Direct Redirects
- `GET /{shortCode}` - Redirect to original URL (handled by RedirectMiddleware)

## Important Notes

### Entity Framework
- Uses SQL Server with Entity Framework Core 10.0 preview
- Connection string includes `TrustServerCertificate=true` for development
- Database context is `AppDbContext` in Infrastructure layer

### Error Handling
- `ExceptionHandlingMiddleware` provides global exception handling
- Controllers return appropriate HTTP status codes
- Logging is configured with structured logging patterns

### CORS Policy
- Currently configured to allow all origins ("AllowedOrigins" policy)
- Should be restricted for production deployments

### Docker Configuration
- Multi-stage Dockerfile optimized for .NET 10
- Docker Compose includes Traefik reverse proxy, SQL Server, and Redis
- Production environment variables configured in compose.yaml