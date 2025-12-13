# Testing Backend - Guía Completa

## Visión General

Stack de testing seleccionado para el backend .NET 10 del sistema de gestión de puntos de venta.

| Componente | Tecnología | Versión |
|------------|------------|---------|
| **Framework de Testing** | xUnit | 2.9.x |
| **Mocking** | Moq | 4.20.x |
| **Assertions** | FluentAssertions | 7.x |
| **CI/CD** | GitHub Actions | - |
| **Tests de Integración** | Testcontainers | 4.x |
| **Generación de Datos** | Bogus | 35.x |

---

## 📚 Índice de Documentación

### Configuración Inicial
| Documento | Descripción |
|-----------|-------------|
| [01 - Configuración](Testing/Backend/01-configuracion.md) | Stack tecnológico, paquetes NuGet, estructura de proyecto y convenciones |

### Tests Unitarios
| Documento | Descripción |
|-----------|-------------|
| [02 - Tests Unitarios](Testing/Backend/02-tests-unitarios.md) | Ejemplos básicos, [Theory], excepciones y generación de datos con Bogus |
| [04 - Mocking EF Core](Testing/Backend/04-mocking-efcore.md) | Mockear DbContext, DbSet, transacciones y uso de InMemory Database |
| [08 - Validaciones](Testing/Backend/08-validaciones.md) | FluentValidation.TestHelper, DataAnnotations y validaciones async |

### Tests de Integración
| Documento | Descripción |
|-----------|-------------|
| [03 - Testcontainers](Testing/Backend/03-testcontainers.md) | PostgreSQL en Docker, fixtures, Respawn y tests de repositorio/API |
| [05 - Autenticación JWT](Testing/Backend/05-autenticacion-jwt.md) | Generar tokens de test, roles, refresh tokens y endpoints protegidos |
| [09 - Archivos y Uploads](Testing/Backend/09-archivos-uploads.md) | Excel con ClosedXML, MockFileSystem y almacenamiento en la nube |

### CI/CD y Calidad
| Documento | Descripción |
|-----------|-------------|
| [06 - GitHub Actions](Testing/Backend/06-github-actions.md) | Workflows, matriz de tests, caché, artifacts y seguridad |
| [07 - Cobertura de Código](Testing/Backend/07-cobertura-codigo.md) | Coverlet, reportes HTML, umbrales mínimos y Codecov |

---

## 🚀 Inicio Rápido

### 1. Crear Proyectos de Test

```bash
# Tests unitarios
cd backend/tests
dotnet new xunit -n Joyeria.UnitTests
cd Joyeria.UnitTests
dotnet add package Moq
dotnet add package FluentAssertions
dotnet add package Bogus

# Tests de integración
cd ..
dotnet new xunit -n Joyeria.IntegrationTests
cd Joyeria.IntegrationTests
dotnet add package Microsoft.AspNetCore.Mvc.Testing
dotnet add package Testcontainers.PostgreSql
dotnet add package Respawn
```

### 2. Ejecutar Tests

```bash
# Todos los tests
dotnet test backend/Joyeria.sln

# Solo unitarios
dotnet test backend/tests/Joyeria.UnitTests

# Con cobertura
dotnet test --collect:"XPlat Code Coverage"

# Filtrar por nombre
dotnet test --filter "FullyQualifiedName~ProductService"
```

---

## 📋 Checklist de Implementación

### Fase 1: Setup Inicial
- [ ] Crear proyecto `Joyeria.UnitTests`
- [ ] Crear proyecto `Joyeria.IntegrationTests`
- [ ] Instalar paquetes NuGet
- [ ] Configurar referencias a proyectos
- [ ] Crear estructura de carpetas

### Fase 2: Tests Unitarios
- [ ] Crear `TestDataBuilder` con Bogus
- [ ] Tests de Services (ProductService, SaleService, etc.)
- [ ] Tests de Validators
- [ ] Tests de mappers/transformaciones

### Fase 3: Tests de Integración
- [ ] Configurar `DatabaseFixture` con Testcontainers
- [ ] Configurar `ApiFixture` con WebApplicationFactory
- [ ] Tests de Repositories
- [ ] Tests de Controllers
- [ ] Tests de autenticación

### Fase 4: CI/CD
- [ ] Crear workflow de GitHub Actions
- [ ] Configurar reporte de tests
- [ ] Configurar cobertura de código
- [ ] Verificar ejecución en PR

### Fase 5: Mantenimiento
- [ ] Añadir badge de tests en README
- [ ] Documentar cómo ejecutar tests localmente
- [ ] Establecer cobertura mínima requerida (70%)
- [ ] Revisar y actualizar tests regularmente

---

## 📖 Convenciones

### Nomenclatura de Tests

```
Método_Escenario_ResultadoEsperado
```

**Ejemplos:**
- `GetProductBySku_WhenProductExists_ShouldReturnProduct`
- `CreateSale_WithInsufficientStock_ShouldThrowException`
- `Login_WithValidCredentials_ShouldReturnToken`

### Estructura AAA

```csharp
[Fact]
public async Task NombreDelTest()
{
    // Arrange - Preparar datos y mocks
    
    // Act - Ejecutar la acción a testear
    
    // Assert - Verificar resultados
}
```

---

## 🔗 Recursos Externos

- [xUnit Documentation](https://xunit.net/docs/getting-started/netcore/cmdline)
- [FluentAssertions Documentation](https://fluentassertions.com/introduction)
- [Moq Quickstart](https://github.com/moq/moq4/wiki/Quickstart)
- [ASP.NET Core Integration Tests](https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests)
- [Testcontainers .NET](https://dotnet.testcontainers.org/)
- [Unit Testing Best Practices](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices)

---

## 🎯 Conclusión

Esta combinación de herramientas ofrece:

- ✅ **Productividad**: Sintaxis clara y herramientas bien documentadas
- ✅ **Confiabilidad**: Tests reproducibles con contenedores Docker
- ✅ **Escalabilidad**: Fácil de extender y mantener
- ✅ **Integración**: Compatible con GitHub Actions y free-tier
- ✅ **Comunidad**: Amplio soporte y ejemplos disponibles
