# Testing Frontend - Guía Completa

## Visión General

Stack de testing seleccionado para el frontend React 19 + TypeScript + Vite del sistema de gestión de puntos de venta.

| Componente | Tecnología | Versión |
|------------|------------|---------|
| **Test Runner** | Vitest | 2.x |
| **Testing de Componentes** | React Testing Library | 16.x |
| **Simulación de Usuario** | @testing-library/user-event | 14.x |
| **Matchers DOM** | @testing-library/jest-dom | 6.x |
| **Mocking de API** | MSW (Mock Service Worker) | 2.x |
| **Tests E2E** | Playwright | 1.x |
| **Entorno DOM** | jsdom | 25.x |

---

## 📚 Índice de Documentación

### Configuración Inicial
| Documento | Descripción |
|-----------|-------------|
| [01 - Configuración](Testing/Frontend/01-configuracion.md) | Stack tecnológico, instalación, estructura de proyecto y convenciones |

### Tests Unitarios y de Componentes
| Documento | Descripción |
|-----------|-------------|
| [02 - Tests Unitarios](Testing/Frontend/02-tests-unitarios.md) | Tests de hooks, utilities, helpers y funciones puras |
| [03 - Tests de Componentes](Testing/Frontend/03-tests-componentes.md) | React Testing Library, queries, user events, formularios (React Hook Form + Zod) y accesibilidad |
| [04 - Mocking de API](Testing/Frontend/04-mocking-api.md) | MSW handlers, escenarios de error, interceptores y estados de carga |

### Tests End-to-End
| Documento | Descripción |
|-----------|-------------|
| [05 - Tests E2E](Testing/Frontend/05-tests-e2e.md) | Playwright, navegación, formularios, autenticación y multi-navegador |

### CI/CD y Calidad
| Documento | Descripción |
|-----------|-------------|
| [06 - GitHub Actions](Testing/Frontend/06-github-actions.md) | Workflows, caché de dependencias, artifacts y reportes |
| [07 - Cobertura de Código](Testing/Frontend/07-cobertura-codigo.md) | Configuración de coverage, umbrales mínimos y reportes HTML |

---

## 🚀 Inicio Rápido

### 1. Instalar Dependencias

```bash
cd frontend

# Vitest y Testing Library
npm install -D vitest @testing-library/react @testing-library/jest-dom @testing-library/user-event jsdom

# MSW para mocking de API
npm install -D msw

# Playwright para E2E
npm install -D @playwright/test
npx playwright install
```

### 2. Configurar Vitest

**vite.config.ts**
```typescript
/// <reference types="vitest" />
import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

export default defineConfig({
  plugins: [react()],
  test: {
    globals: true,
    environment: 'jsdom',
    setupFiles: ['./src/test/setup.ts'],
    include: ['src/**/*.{test,spec}.{ts,tsx}'],
    coverage: {
      provider: 'v8',
      reporter: ['text', 'html', 'lcov'],
      exclude: ['node_modules/', 'src/test/'],
    },
  },
})
```

**src/test/setup.ts**
```typescript
import '@testing-library/jest-dom'
import { cleanup } from '@testing-library/react'
import { afterEach } from 'vitest'

// Limpieza automática después de cada test
afterEach(() => {
  cleanup()
})
```

### 3. Ejecutar Tests

```bash
# Tests unitarios y de componentes
npm run test

# Tests en modo watch
npm run test:watch

# Tests con cobertura
npm run test:coverage

# Tests E2E con Playwright
npm run test:e2e

# Tests E2E con UI de Playwright
npm run test:e2e:ui
```

### 4. Scripts en package.json

```json
{
  "scripts": {
    "test": "vitest run",
    "test:watch": "vitest",
    "test:coverage": "vitest run --coverage",
    "test:ui": "vitest --ui",
    "test:e2e": "playwright test",
    "test:e2e:ui": "playwright test --ui",
    "test:e2e:headed": "playwright test --headed"
  }
}
```

---

## 📁 Estructura de Proyecto

```
frontend/
├── src/
│   ├── components/
│   │   ├── ui/
│   │   │   ├── button.tsx
│   │   │   └── button.test.tsx          # Test junto al componente
│   │   └── layouts/
│   ├── hooks/
│   │   ├── use-auth.ts
│   │   └── use-auth.test.ts             # Test junto al hook
│   ├── services/
│   │   ├── api.service.ts
│   │   └── api.service.test.ts
│   ├── lib/
│   │   ├── utils.ts
│   │   └── utils.test.ts
│   └── test/
│       ├── setup.ts                      # Setup global de Vitest
│       ├── mocks/
│       │   ├── handlers.ts               # MSW handlers
│       │   └── server.ts                 # MSW server
│       ├── utils/
│       │   ├── render.tsx                # Custom render con providers
│       │   └── test-data.ts              # Factories de datos de test
│       └── __fixtures__/
│           └── products.json             # Datos de prueba
├── e2e/
│   ├── auth.spec.ts                      # Tests E2E de autenticación
│   ├── products.spec.ts                  # Tests E2E de productos
│   ├── sales.spec.ts                     # Tests E2E de ventas
│   └── fixtures/
│       └── test-user.json
├── playwright.config.ts
├── vite.config.ts
└── package.json
```

---

## 📋 Checklist de Implementación

### Fase 1: Setup Inicial
- [ ] Instalar dependencias de testing
- [ ] Configurar Vitest en `vite.config.ts`
- [ ] Crear archivo `src/test/setup.ts`
- [ ] Configurar scripts en `package.json`
- [ ] Crear estructura de carpetas de test

### Fase 2: Tests Unitarios
- [ ] Crear custom render con providers
- [ ] Tests de hooks personalizados (`use-auth`, `use-menu`, etc.)
- [ ] Tests de utilities (`lib/utils.ts`, `lib/helpers.ts`)
- [ ] Tests de servicios de API

### Fase 3: Tests de Componentes
- [ ] Configurar MSW handlers base
- [ ] Tests de componentes UI básicos (Button, Input, Select)
- [ ] Tests de componentes de formulario
- [ ] Tests de componentes con estado
- [ ] Tests de accesibilidad (a11y)

### Fase 4: Tests E2E
- [ ] Configurar Playwright (`playwright.config.ts`)
- [ ] Tests de flujo de autenticación
- [ ] Tests de CRUD de productos
- [ ] Tests de registro de ventas
- [ ] Tests responsive (móvil/desktop)

### Fase 5: CI/CD
- [ ] Crear workflow de GitHub Actions
- [ ] Configurar caché de dependencias
- [ ] Configurar reporte de tests
- [ ] Configurar cobertura de código
- [ ] Verificar ejecución en PR

### Fase 6: Mantenimiento
- [ ] Añadir badge de tests en README
- [ ] Documentar cómo ejecutar tests localmente
- [ ] Establecer cobertura mínima requerida (70%)
- [ ] Revisar y actualizar tests regularmente

---

## 📖 Convenciones

### Nomenclatura de Tests

```
describe('NombreComponente/Hook/Función', () => {
  it('should [comportamiento esperado] when [condición]', () => {})
})
```

**Ejemplos:**
- `should render product name when product is provided`
- `should call onSubmit when form is valid`
- `should show error message when API returns 401`
- `should disable button when loading is true`

### Estructura de Test

```typescript
import { describe, it, expect, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'

describe('ProductCard', () => {
  it('should render product information correctly', async () => {
    // Arrange - Preparar datos y renderizar
    const product = { id: '1', name: 'Anillo Oro', sku: 'ANI-001' }
    render(<ProductCard product={product} />)

    // Act - Ejecutar acciones (si las hay)
    // En este caso no hay acciones

    // Assert - Verificar resultados
    expect(screen.getByText('Anillo Oro')).toBeInTheDocument()
    expect(screen.getByText('ANI-001')).toBeInTheDocument()
  })

  it('should call onClick when card is clicked', async () => {
    // Arrange
    const user = userEvent.setup()
    const handleClick = vi.fn()
    const product = { id: '1', name: 'Anillo Oro', sku: 'ANI-001' }
    render(<ProductCard product={product} onClick={handleClick} />)

    // Act
    await user.click(screen.getByRole('article'))

    // Assert
    expect(handleClick).toHaveBeenCalledWith(product)
  })
})
```

### Queries de Testing Library (Orden de Prioridad)

| Prioridad | Query | Uso |
|-----------|-------|-----|
| 1️⃣ | `getByRole` | Elementos accesibles (botones, links, etc.) |
| 2️⃣ | `getByLabelText` | Inputs de formulario |
| 3️⃣ | `getByPlaceholderText` | Inputs sin label visible |
| 4️⃣ | `getByText` | Texto visible |
| 5️⃣ | `getByDisplayValue` | Valor actual de inputs |
| 6️⃣ | `getByAltText` | Imágenes |
| 7️⃣ | `getByTitle` | Elementos con title |
| 8️⃣ | `getByTestId` | Último recurso (data-testid) |

---

## 🔗 Recursos Externos

- [Vitest Documentation](https://vitest.dev/)
- [React Testing Library](https://testing-library.com/docs/react-testing-library/intro/)
- [Testing Library Cheatsheet](https://testing-library.com/docs/react-testing-library/cheatsheet/)
- [MSW Documentation](https://mswjs.io/docs/)
- [Playwright Documentation](https://playwright.dev/docs/intro)
- [Testing Library - Which Query](https://testing-library.com/docs/queries/about#priority)
- [Kent C. Dodds - Testing JavaScript](https://testingjavascript.com/)

---

## 🎯 Conclusión

Esta combinación de herramientas ofrece:

- ✅ **Velocidad**: Vitest aprovecha Vite para tests ultra-rápidos
- ✅ **Confiabilidad**: Tests basados en comportamiento del usuario real
- ✅ **Multi-navegador**: Playwright testea en Chromium, Firefox y WebKit
- ✅ **Integración**: Compatible con GitHub Actions y free-tier
- ✅ **DX**: Excelente experiencia de desarrollo con hot reload y UI interactiva
- ✅ **Accesibilidad**: Testing Library promueve queries accesibles por defecto
