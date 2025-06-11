# ShortLink Architecture Diagram

## System Overview

```
┌─────────────────────────────────────────────────────────────────────────┐
│                           ShortLink System                               │
└─────────────────────────────────────────────────────────────────────────┘

┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│     Client      │    │   Load Balancer │    │     Docker      │
│   (Browser)     │◄──►│    (Traefik)    │◄──►│   Environment   │
└─────────────────┘    └─────────────────┘    └─────────────────┘
                                │
                                ▼
                    ┌─────────────────────────┐
                    │    ShortLink.WebAPI     │
                    │   (ASP.NET Core API)    │
                    └─────────────────────────┘
                                │
                    ┌───────────┼───────────┐
                    ▼           ▼           ▼
            ┌─────────────┐ ┌─────────────┐ ┌─────────────┐
            │Controllers  │ │ Middleware  │ │ Extensions  │
            │- Links      │ │- Redirect   │ │- Services   │
            │- Stats      │ │- Exception  │ │- Database   │
            └─────────────┘ └─────────────┘ └─────────────┘
                                │
                                ▼
                    ┌─────────────────────────┐
                    │  ShortLink.Application  │
                    │      (CQRS Layer)       │
                    └─────────────────────────┘
                                │
                    ┌───────────┼───────────┐
                    ▼           ▼           ▼
            ┌─────────────┐ ┌─────────────┐ ┌─────────────┐
            │  Commands   │ │   Queries   │ │    DTOs     │
            │- CreateLink │ │- GetByCode  │ │- LinkDto    │
            │- Redirect   │ │- GetRecent  │ │- Settings   │
            └─────────────┘ └─────────────┘ └─────────────┘
                                │
                                ▼
                    ┌─────────────────────────┐
                    │   ShortLink.Domain      │
                    │   (Business Logic)      │
                    └─────────────────────────┘
                                │
                    ┌───────────┼───────────┐
                    ▼           ▼           ▼
            ┌─────────────┐ ┌─────────────┐ ┌─────────────┐
            │  Entities   │ │Value Objects│ │ Interfaces  │
            │- Link       │ │- ShortCode  │ │- Repository │
            │             │ │             │ │- Cache      │
            └─────────────┘ └─────────────┘ └─────────────┘
                                │
                                ▼
                    ┌─────────────────────────┐
                    │ ShortLink.Infrastructure│
                    │   (Data & Services)     │
                    └─────────────────────────┘
                                │
                    ┌───────────┼───────────┐
                    ▼           ▼           ▼
            ┌─────────────┐ ┌─────────────┐ ┌─────────────┐
            │ Repositories│ │    Cache    │ │  Services   │
            │- Link Repo  │ │- Redis      │ │- CodeGen    │
            │- UnitOfWork │ │- Options    │ │- External   │
            └─────────────┘ └─────────────┘ └─────────────┘
                                │
                    ┌───────────┼───────────┐
                    ▼           ▼           ▼
            ┌─────────────┐ ┌─────────────┐ ┌─────────────┐
            │ SQL Server  │ │    Redis    │ │   Health    │
            │  Database   │ │    Cache    │ │   Checks    │
            │- Entity     │ │- Key/Value  │ │- DB Status  │
            │  Framework  │ │- Statistics │ │- Redis      │
            └─────────────┘ └─────────────┘ └─────────────┘
```

## Data Flow

### 1. Create Short Link
```
Client → API → CreateLinkCommand → Domain → Repository → Database
   ↓                                           ↓
   ↓                                        Cache
   ↓                                           ↓
   ← Response ← Handler ← Business Logic ← Storage
```

### 2. Redirect Short Link
```
Client → RedirectMiddleware → Cache (Redis) → Response
   ↓                             ↓
   ↓                          (if miss)
   ↓                             ↓
   ← Redirect ← Database ← Repository
```

### 3. Get Statistics
```
Client → StatsController → Cache → Statistics
   ↓                        ↓
   ↓                   (aggregated)
   ↓                        ↓
   ← JSON Response ← Click Counts
```

## Key Components

### Clean Architecture Layers
- **Presentation**: WebAPI Controllers, Middleware
- **Application**: CQRS Commands/Queries, Handlers
- **Domain**: Entities, Value Objects, Business Rules
- **Infrastructure**: Repositories, External Services, Cache

### CQRS Pattern
- **Commands**: CreateLink, RedirectLink (write operations)
- **Queries**: GetLinkByCode, GetRecentLinks (read operations)
- **Handlers**: Process commands/queries independently

### Caching Strategy
- **Redis**: Primary cache for performance
- **Keys**: Prefixed with "shortlink:"
- **TTL**: Configurable expiration times
- **Fallback**: Database when cache misses

### Middleware Pipeline
1. **Exception Handling**: Global error management
2. **Redirect**: Short URL processing
3. **CORS**: Cross-origin request handling
4. **Routing**: API endpoint resolution

## Technology Stack

### Backend
- **.NET 10**: Runtime and framework
- **ASP.NET Core**: Web API framework
- **Entity Framework Core**: ORM for database
- **Redis**: Caching and session storage

### Database
- **SQL Server**: Primary data storage
- **Entity Framework Migrations**: Schema management

### DevOps
- **Docker**: Containerization
- **Docker Compose**: Multi-container orchestration
- **Traefik**: Load balancing and reverse proxy

### Development
- **Clean Architecture**: Separation of concerns
- **CQRS**: Command Query Responsibility Segregation
- **Repository Pattern**: Data access abstraction
- **Dependency Injection**: Inversion of control