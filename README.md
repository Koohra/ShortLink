# ShortLink API

[![.NET](https://img.shields.io/badge/.NET-10-blue)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

## Descrição

ShortLink é um serviço de encurtamento de URLs construído com .NET 10, seguindo os princípios da Clean Architecture. A aplicação utiliza Redis para cache de alta performance, Entity Framework para persistência de dados, e implementa redirecionamentos automáticos através de middleware customizado.

## Arquitetura

O projeto segue a Clean Architecture com separação clara de responsabilidades:

- **ShortLink.Domain**: Entidades, Value Objects e interfaces de domínio
- **ShortLink.Application**: CQRS commands/queries, DTOs e lógica de aplicação
- **ShortLink.Infrastructure**: Repositórios, cache, contexto de dados e serviços externos
- **ShortLink.WebAPI**: Controllers REST, middleware e configurações

## Funcionalidades

### Redirecionamento Automático
- Acesse `/{codigo}` para redirecionamento direto
- Cache em Redis para performance otimizada
- Fallback automático para banco de dados
- Tratamento de links expirados

### API REST

#### Links
- **POST /api/links**: Cria um novo link encurtado
- **GET /api/links/{code}**: Busca detalhes de um link pelo código curto
- **GET /api/links?count=n**: Lista os links mais recentes
- **DELETE /api/links/{code}**: Remove um link (planejado)

#### Estatísticas
- **GET /api/stats/{code}**: Estatísticas completas do link (cliques, datas, etc.)

#### Monitoramento
- **GET /health**: Health checks da aplicação (banco + Redis)

## Tecnologias

- **.NET 10** com C# 13
- **Entity Framework Core 10** (SQL Server)
- **Redis** para cache e contagem de cliques
- **CQRS Pattern** com handlers customizados
- **Clean Architecture** com DDD
- **Docker** e **Docker Compose** para containerização
- **Traefik** como reverse proxy

## Design Patterns Implementados

### **🏗️ Arquiteturais**
- **Clean Architecture**: Dependências fluindo para o domínio
- **CQRS**: Separação completa entre Commands e Queries
- **DDD**: Entidades ricas com lógica de negócio encapsulada
- **Hexagonal Architecture**: Ports & Adapters para independência tecnológica

### **🔧 Comportamentais**
- **Repository Pattern**: Abstração de persistência de dados
- **Unit of Work**: Coordenação transacional entre repositórios
- **Strategy Pattern**: Algoritmos intercambiáveis (geração de códigos)
- **Middleware Pattern**: Pipeline de processamento de requests
- **Mediator Pattern**: Desacoplamento com MediatR

### **🏭 Criativos**
- **Factory Method**: Criação controlada de entidades (`Link.Create()`)
- **Dependency Injection**: IoC container nativo do .NET

### **💾 Integração**
- **Cache-Aside**: Performance otimizada com Redis
- **Gateway Pattern**: Controllers como pontos de entrada unificados

> **📖 Documentação Completa**: Veja `contexto.md` para análise detalhada de todos os 15+ patterns com exemplos de código

## Configuração e Execução

### Desenvolvimento Local

```bash
# 1. Clone o repositório
git clone <url-do-repositorio>
cd ShortLink

# 2. Configure as connection strings em appsettings.json
# - DefaultConnection (SQL Server)
# - Redis connection

# 3. Execute as migrations
cd ShortLink.WebAPI
dotnet ef database update

# 4. Execute a aplicação
dotnet run
```

### Docker Compose (Recomendado)

```bash
# Inicia todos os serviços (API, SQL Server, Redis, Traefik)
docker-compose up -d

# Acompanha os logs
docker-compose logs -f api

# Para os serviços
docker-compose down
```

## Configuração

### Variáveis de Ambiente

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=ShortLinkDb;User Id=sa;Password=MinhaSenh@123;TrustServerCertificate=true;",
    "Redis": "localhost:6379"
  },
  "AppSettings": {
    "BaseUrl": "http://localhost:5062/"
  },
  "Redis": {
    "KeyPrefix": "shortlink",
    "DefaultExpiryHours": 24,
    "StatsExpiryDays": 30
  }
}
```

### Docker Compose Services

- **API**: Porta 8080 (através do Traefik)
- **SQL Server**: Dados persistidos em volume
- **Redis**: Cache em memória com persistência
- **Traefik**: Dashboard em http://localhost:8080

## Exemplos de Uso

### Criar Link Encurtado
```http
POST /api/links
Content-Type: application/json

{
  "url": "https://www.exemplo.com/pagina-muito-longa",
  "expiresAt": "2024-12-31T23:59:59Z"
}
```

### Acessar Link
```http
GET /abc123
# Redireciona automaticamente para a URL original
```

### Obter Estatísticas
```http
GET /api/stats/abc123
# Retorna cliques, datas de criação/expiração, etc.
```

## Observações Técnicas

- **Performance**: Cache Redis para redirecionamentos rápidos
- **Escalabilidade**: Arquitetura preparada para múltiplas instâncias
- **Monitoramento**: Health checks e logging estruturado
- **Segurança**: Validação de URLs e tratamento de erros
- **Expiração**: Links podem ter data de expiração configurável

## Desenvolvimento

Para contribuir com o projeto, consulte o arquivo `CLAUDE.md` que contém informações detalhadas sobre a arquitetura e padrões utilizados.

## Licença

Este projeto está licenciado sob a licença MIT - veja o arquivo [LICENSE](LICENSE) para mais detalhes. 