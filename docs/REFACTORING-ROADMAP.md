# ShortLink - Roadmap de Refatoração

> **Documento gerado em**: Fevereiro 2025  
> **Status**: Pendente implementação  
> **Versão**: 1.0

Este documento contém uma análise completa do código e lista todas as correções, refatorações e melhorias necessárias para o projeto ShortLink.

---

## 🔴 **CRÍTICOS - Implementar IMEDIATAMENTE**

### 1. **Credenciais Hardcoded (SEGURANÇA CRÍTICA)**
- **Arquivo**: `ShortLink.WebAPI/appsettings.json:3-4`
- **Problema**: 
  ```json
  "DefaultConnection": "Server=localhost,1433;Database=ShortLinkDb;User Id=sa;Password=MinhaSenh@123;..."
  ```
- **Risco**: Credenciais expostas no controle de versão
- **Solução**:
  ```bash
  # Usar user secrets para desenvolvimento
  dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost,1433;Database=ShortLinkDb;User Id=sa;Password=YOUR_PASSWORD;TrustServerCertificate=true;"
  
  # Production: Usar environment variables
  export ConnectionStrings__DefaultConnection="..."
  ```
- **Prioridade**: 🔴 CRÍTICA

### 2. **Comando DELETE Faltando**
- **Arquivo**: `ShortLink.WebAPI/Controllers/LinksController.cs:147`
- **Problema**: Referencia `DeleteLinkCommand` que não existe
- **Erro**: `using ShortLink.Application.Commands.DeleteLink;` - namespace não existe
- **Arquivos faltando**:
  ```
  ShortLink.Application/Commands/DeleteLink/
  ├── DeleteLinkCommand.cs
  ├── DeleteLinkHandler.cs
  └── DeleteLinkResponse.cs
  ```
- **Prioridade**: 🔴 CRÍTICA

### 3. **Bug no Cache Redis**
- **Arquivo**: `ShortLink.Infrastructure/Cache/RedisCacheService.cs`
- **Problema**: Inconsistência nas chaves Redis
  ```csharp
  // Linha 25: Salva com padrão
  string urlKey = $"url{shortCode}";
  
  // Linha 31: Busca com padrão diferente  
  string urlKey = $"url:{shortCode}";
  ```
- **Impacto**: Cache miss sempre, performance degradada
- **Prioridade**: 🔴 CRÍTICA

### 4. **SQL Injection Potencial**
- **Arquivo**: `ShortLink.Infrastructure/Repositories/LinkRepository.cs:23,32`
- **Problema**: Uso desnecessário de `FromSqlRaw`
- **Solução**: Substituir por LINQ
  ```csharp
  // ❌ Atual (desnecessário)
  return await _context.Links.FromSqlRaw("SELECT * FROM Links WHERE ShortCode = {0}", codeValue)
  
  // ✅ Melhor
  return await _context.Links.FirstOrDefaultAsync(l => l.ShortCode.Value == codeValue);
  ```
- **Prioridade**: 🔴 CRÍTICA

---

## 🟡 **IMPORTANTES - Refatorações Arquiteturais**

### 5. **Arquitetura CQRS Inconsistente**
- **Problemas**:
  - Queries estão em `/Commands/Queries/` (estrutura errada)
  - Interfaces customizadas (`ICommandHandler`, `IQueryHandler`) não utilizadas
  - MediatR implementado mas contradiz documentação CLAUDE.md
  - Handler com nome inconsistente: `GetLinkCodeHandler` → `GetLinkByCodeHandler`

- **Estrutura atual**:
  ```
  ShortLink.Application/Commands/
  ├── CreateLink/
  ├── Queries/           ❌ Errado
  │   ├── GetLinkByCode/
  │   ├── GetRecentLinks/
  │   └── RedirectLink/  ❌ É command, não query
  ```

- **Estrutura correta**:
  ```
  ShortLink.Application/
  ├── Commands/
  │   ├── CreateLink/
  │   ├── DeleteLink/    (faltando)
  │   └── RedirectLink/  (mover)
  └── Queries/
      ├── GetLinkByCode/
      └── GetRecentLinks/
  ```
- **Prioridade**: 🟡 IMPORTANTE

### 6. **Value Object Incompleto**
- **Arquivo**: `ShortLink.Domain/ValueObject/ShortCode.cs`
- **Problema**: Não implementa padrão Value Object corretamente
- **Faltando**:
  ```csharp
  public bool Equals(ShortCode other)
  public override bool Equals(object obj)
  public override int GetHashCode()
  public static bool operator ==(ShortCode left, ShortCode right)
  public static bool operator !=(ShortCode left, ShortCode right)
  ```
- **Prioridade**: 🟡 IMPORTANTE

### 7. **DateTime.Now no Domain**
- **Arquivo**: `ShortLink.Domain/Entities/Link.cs:28,39`
- **Problema**: Viola testabilidade e princípios DDD
- **Solução**: Injetar `IDateTimeProvider`
  ```csharp
  public interface IDateTimeProvider 
  {
      DateTime UtcNow { get; }
  }
  
  public static Link Create(string originalUrl, ShortCode shortCode, 
      IDateTimeProvider dateTimeProvider, DateTime? expiresAt = null)
  ```
- **Prioridade**: 🟡 IMPORTANTE

### 8. **Middleware Duplica Lógica CQRS**
- **Arquivo**: `ShortLink.WebAPI/Middleware/RedirectMiddleware.cs`
- **Problema**: Implementa lógica de redirect que já existe em `RedirectLinkHandler`
- **Violação**: Bypassa padrão CQRS, duplica código
- **Solução**: Middleware deve usar `IMediator` para chamar `RedirectLinkHandler`
- **Prioridade**: 🟡 IMPORTANTE

---

## 🟢 **MELHORIAS - Performance e Qualidade**

### 9. **Performance Redis Crítica**
- **Arquivo**: `ShortLink.Infrastructure/Cache/RedisCacheService.cs:70`
- **Problema**: `Keys()` operation bloqueia Redis inteiro
  ```csharp
  var keys = await _database.KeysAsync(pattern: $"{_keyPrefix}:clicks:*");
  ```
- **Solução**: Usar Set ou Hash para tracking
- **Prioridade**: 🟢 MELHORIA

### 10. **Índices Faltando no Banco**
- **Arquivo**: `ShortLink.Infrastructure/Context/AppDbContext.cs`
- **Problema**: Queries lentas em `CreatedAt` e `ClickCount`
- **Solução**: Adicionar índices
  ```csharp
  entity.HasIndex(e => e.CreatedAt);
  entity.HasIndex(e => e.ClickCount);
  entity.HasIndex(e => new { e.CreatedAt, e.ClickCount });
  ```
- **Prioridade**: 🟢 MELHORIA

### 11. **CORS Muito Permissivo**
- **Arquivo**: `ShortLink.WebAPI/Extensions/ServiceCollectionExtensions.cs:42`
- **Problema**: `AllowAnyOrigin()` em todos os ambientes
- **Risco**: Vulnerabilidade de segurança
- **Solução**: Configurar domínios específicos para produção
- **Prioridade**: 🟢 MELHORIA

### 12. **Exception Handling Inconsistente**
- **Arquivo**: `ShortLink.Infrastructure/Cache/RedisCacheService.cs:88-92`
- **Problema**: Captura todas exceções e retorna vazio silenciosamente
- **Impacto**: Debug difícil, falhas silenciosas
- **Prioridade**: 🟢 MELHORIA

### 13. **Repository Pattern Incompleto**
- **Arquivo**: `ShortLink.Domain/Interfaces/ILinkRepository.cs`
- **Problema**: Faltam métodos padrão
- **Faltando**:
  ```csharp
  Task<Link> GetByIdAsync(Guid id);
  Task<bool> ExistsAsync(ShortCode shortCode);
  Task UpdateAsync(Link link);
  Task DeleteAsync(Link link);
  ```
- **Prioridade**: 🟢 MELHORIA

---

## 📋 **FUNCIONALIDADES FALTANDO**

### 14. **Validação de URL no Domain**
- **Problema**: Validação só na Application layer
- **Solução**: Mover para `Link.Create()` method
- **Benefício**: Garantias de domínio

### 15. **Domain Events**
- **Faltando**: 
  ```csharp
  public class LinkCreatedEvent : DomainEvent
  public class LinkExpiredEvent : DomainEvent  
  public class LinkClickedEvent : DomainEvent
  ```
- **Benefício**: Auditoria, notificações, analytics

### 16. **Circuit Breaker para Redis**
- **Problema**: Sem fallback quando Redis falha
- **Risco**: Cascata de falhas
- **Solução**: Implementar Polly circuit breaker

### 17. **Health Checks Detalhados**
- **Atual**: Básico para DB e Redis
- **Melhoria**: Status detalhado, métricas, dependências

### 18. **Logging Estruturado**
- **Problema**: Logs inconsistentes entre camadas
- **Solução**: Serilog com structured logging

---

## 🛠 **PLANO DE IMPLEMENTAÇÃO**

### **🔥 Fase 1 - Correções Críticas (1-2 dias)**
**Objetivo**: Corrigir problemas de segurança e bugs críticos

- [ ] **1.1** Mover credenciais para user secrets/environment variables
- [ ] **1.2** Implementar `DeleteLinkCommand` completo
- [ ] **1.3** Corrigir bug de chaves inconsistentes no Redis
- [ ] **1.4** Substituir `FromSqlRaw` por LINQ expressions
- [ ] **1.5** Testes básicos para validar correções

**Estimativa**: 12-16 horas

### **🏗 Fase 2 - Refatoração Arquitetural (3-5 dias)**
**Objetivo**: Corrigir inconsistências arquiteturais

- [ ] **2.1** Reorganizar estrutura CQRS (mover Queries para pasta correta)
- [ ] **2.2** Implementar Value Objects corretamente (ShortCode)
- [ ] **2.3** Adicionar `IDateTimeProvider` e remover `DateTime.Now`
- [ ] **2.4** Refatorar middleware para usar CQRS handlers
- [ ] **2.5** Padronizar responses entre handlers
- [ ] **2.6** Corrigir nomes inconsistentes (GetLinkCodeHandler)

**Estimativa**: 24-30 horas

### **⚡ Fase 3 - Performance (2-3 dias)**
**Objetivo**: Otimizar performance e escalabilidade

- [ ] **3.1** Adicionar índices no banco de dados
- [ ] **3.2** Otimizar operações Redis (remover Keys() scan)
- [ ] **3.3** Implementar circuit breaker para Redis
- [ ] **3.4** Melhorar exception handling
- [ ] **3.5** Configurar CORS adequadamente
- [ ] **3.6** Otimizar queries Entity Framework

**Estimativa**: 16-20 horas

### **🚀 Fase 4 - Funcionalidades e Qualidade (5-7 dias)**
**Objetivo**: Adicionar funcionalidades faltantes e melhorar qualidade

- [ ] **4.1** Implementar Domain Events
- [ ] **4.2** Adicionar validações robustas no Domain
- [ ] **4.3** Implementar logging estruturado (Serilog)
- [ ] **4.4** Health checks detalhados
- [ ] **4.5** Testes unitários abrangentes
- [ ] **4.6** Documentação API (Swagger/OpenAPI)
- [ ] **4.7** Performance monitoring (métricas)

**Estimativa**: 32-40 horas

---

## 📊 **MÉTRICAS DE SUCESSO**

### **Segurança**
- [ ] Zero credenciais hardcoded
- [ ] CORS configurado corretamente
- [ ] SQL injection eliminado

### **Performance**
- [ ] Cache hit rate > 90%
- [ ] Response time médio < 100ms
- [ ] Zero operações Redis bloqueantes

### **Qualidade de Código**
- [ ] Cobertura de testes > 80%
- [ ] Zero warnings de build
- [ ] Arquitetura CQRS consistente

### **Produção**
- [ ] Health checks funcionais
- [ ] Logging estruturado
- [ ] Monitoring implementado

---

## 🔧 **SCRIPTS ÚTEIS**

### **Verificar Credenciais Hardcoded**
```bash
# Buscar possíveis credenciais no código
grep -r "password\|senha\|secret" --include="*.cs" --include="*.json" .
```

### **Análise de Performance Redis**
```bash
# Monitorar operações Redis
redis-cli monitor | grep "KEYS\|SCAN"
```

### **Database Queries Análise**
```bash
# Ativar logging EF para ver queries geradas
dotnet ef dbcontext optimize --verbose
```

---

## 📝 **NOTAS IMPORTANTES**

1. **Backup**: Fazer backup antes de iniciar refatorações
2. **Testes**: Implementar testes antes de refatorar código crítico
3. **Incremental**: Implementar mudanças incrementalmente
4. **Review**: Code review obrigatório para mudanças arquiteturais
5. **Documentação**: Atualizar CLAUDE.md após mudanças significativas

---

**Última atualização**: Fevereiro 2025  
**Próxima revisão**: Após Fase 1 completa