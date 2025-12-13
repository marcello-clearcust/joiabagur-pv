# 01 - Configuración del Entorno de Testing Frontend

## Stack Tecnológico

| Herramienta | Propósito | Versión |
|-------------|-----------|---------|
| **Vitest** | Test runner nativo para Vite | 2.x |
| **React Testing Library** | Testing de componentes React | 16.x |
| **@testing-library/user-event** | Simulación de interacciones de usuario | 14.x |
| **@testing-library/jest-dom** | Matchers adicionales para DOM | 6.x |
| **jsdom** | Entorno DOM para Node.js | 25.x |
| **MSW** | Mock Service Worker para API | 2.x |
| **Playwright** | Tests End-to-End multi-navegador | 1.x |

---

## Instalación de Dependencias

### Dependencias de Desarrollo

```bash
cd frontend

# Core de Vitest
npm install -D vitest

# React Testing Library
npm install -D @testing-library/react @testing-library/jest-dom @testing-library/user-event

# Entorno DOM
npm install -D jsdom

# MSW para mocking de API
npm install -D msw

# Playwright para E2E
npm install -D @playwright/test

# Instalar navegadores de Playwright
npx playwright install
```

### Todas las dependencias en un comando

```bash
npm install -D vitest @testing-library/react @testing-library/jest-dom @testing-library/user-event jsdom msw @playwright/test
```

---

## Configuración de Vitest

### vite.config.ts

```typescript
/// <reference types="vitest" />
import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import path from 'path'

export default defineConfig({
  plugins: [react()],
  resolve: {
    alias: {
      '@': path.resolve(__dirname, './src'),
    },
  },
  test: {
    // Habilita APIs globales (describe, it, expect, vi)
    globals: true,
    
    // Entorno de simulación del DOM
    environment: 'jsdom',
    
    // Archivo de setup que se ejecuta antes de cada test
    setupFiles: ['./src/test/setup.ts'],
    
    // Patrón de archivos de test
    include: ['src/**/*.{test,spec}.{ts,tsx}'],
    
    // Excluir carpetas
    exclude: ['node_modules', 'e2e'],
    
    // Configuración de cobertura
    coverage: {
      provider: 'v8',
      reporter: ['text', 'html', 'lcov', 'json'],
      reportsDirectory: './coverage',
      exclude: [
        'node_modules/',
        'src/test/',
        '**/*.d.ts',
        '**/*.config.ts',
        '**/types.ts',
      ],
      // Umbrales mínimos de cobertura
      thresholds: {
        lines: 70,
        functions: 70,
        branches: 70,
        statements: 70,
      },
    },
    
    // Configuración de reporters
    reporters: ['verbose', 'html'],
    outputFile: {
      html: './test-results/index.html',
    },
  },
})
```

### tsconfig.json (ajustes para testing)

```json
{
  "compilerOptions": {
    "types": ["vitest/globals", "@testing-library/jest-dom"]
  }
}
```

---

## Archivo de Setup Global

### src/test/setup.ts

```typescript
import '@testing-library/jest-dom'
import { cleanup } from '@testing-library/react'
import { afterEach, beforeAll, afterAll } from 'vitest'
import { server } from './mocks/server'

// Limpieza automática después de cada test
afterEach(() => {
  cleanup()
})

// Configuración de MSW
beforeAll(() => {
  // Iniciar el servidor de MSW antes de todos los tests
  server.listen({ onUnhandledRequest: 'error' })
})

afterEach(() => {
  // Resetear handlers después de cada test
  server.resetHandlers()
})

afterAll(() => {
  // Cerrar servidor después de todos los tests
  server.close()
})

// Silenciar warnings de React 18 en tests (opcional)
const originalError = console.error
beforeAll(() => {
  console.error = (...args: unknown[]) => {
    if (
      typeof args[0] === 'string' &&
      args[0].includes('Warning: ReactDOM.render is no longer supported')
    ) {
      return
    }
    originalError.call(console, ...args)
  }
})

afterAll(() => {
  console.error = originalError
})
```

---

## Scripts de package.json

```json
{
  "scripts": {
    "test": "vitest run",
    "test:watch": "vitest",
    "test:coverage": "vitest run --coverage",
    "test:ui": "vitest --ui",
    "test:e2e": "playwright test",
    "test:e2e:ui": "playwright test --ui",
    "test:e2e:headed": "playwright test --headed",
    "test:e2e:debug": "playwright test --debug",
    "test:all": "npm run test && npm run test:e2e"
  }
}
```

---

## Configuración de Playwright

### playwright.config.ts

```typescript
import { defineConfig, devices } from '@playwright/test'

export default defineConfig({
  // Directorio de tests E2E
  testDir: './e2e',
  
  // Ejecutar tests en paralelo
  fullyParallel: true,
  
  // Fallar el build si hay tests con .only
  forbidOnly: !!process.env.CI,
  
  // Reintentos en CI
  retries: process.env.CI ? 2 : 0,
  
  // Workers paralelos
  workers: process.env.CI ? 1 : undefined,
  
  // Reporter
  reporter: [
    ['html', { outputFolder: 'playwright-report' }],
    ['list'],
  ],
  
  // Configuración compartida para todos los tests
  use: {
    // URL base de la aplicación
    baseURL: 'http://localhost:5173',
    
    // Capturar trace en caso de fallo
    trace: 'on-first-retry',
    
    // Capturar screenshot en caso de fallo
    screenshot: 'only-on-failure',
    
    // Capturar video en caso de fallo
    video: 'on-first-retry',
  },

  // Configuración de proyectos (navegadores)
  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] },
    },
    {
      name: 'firefox',
      use: { ...devices['Desktop Firefox'] },
    },
    {
      name: 'webkit',
      use: { ...devices['Desktop Safari'] },
    },
    // Tests en móvil
    {
      name: 'mobile-chrome',
      use: { ...devices['Pixel 5'] },
    },
    {
      name: 'mobile-safari',
      use: { ...devices['iPhone 12'] },
    },
  ],

  // Servidor de desarrollo para tests
  webServer: {
    command: 'npm run dev',
    url: 'http://localhost:5173',
    reuseExistingServer: !process.env.CI,
    timeout: 120 * 1000,
  },
})
```

---

## Estructura de Carpetas de Testing

```
frontend/
├── src/
│   ├── components/
│   │   └── ui/
│   │       ├── button.tsx
│   │       └── button.test.tsx      # ✅ Test junto al componente
│   ├── hooks/
│   │   ├── use-auth.ts
│   │   └── use-auth.test.ts         # ✅ Test junto al hook
│   ├── services/
│   │   ├── auth.service.ts
│   │   └── auth.service.test.ts     # ✅ Test junto al servicio
│   ├── lib/
│   │   ├── utils.ts
│   │   └── utils.test.ts            # ✅ Test junto a utilities
│   └── test/
│       ├── setup.ts                  # Setup global de Vitest
│       ├── mocks/
│       │   ├── handlers.ts           # MSW request handlers
│       │   ├── server.ts             # MSW server config
│       │   └── browser.ts            # MSW browser config (dev)
│       ├── utils/
│       │   ├── render.tsx            # Custom render con providers
│       │   ├── test-data.ts          # Factories de datos
│       │   └── test-utils.ts         # Helpers de testing
│       └── __fixtures__/
│           ├── products.json
│           ├── users.json
│           └── sales.json
├── e2e/
│   ├── auth.spec.ts
│   ├── products.spec.ts
│   ├── sales.spec.ts
│   ├── inventory.spec.ts
│   └── fixtures/
│       └── test-user.ts
├── playwright.config.ts
├── vite.config.ts
└── package.json
```

---

## Convenciones de Nomenclatura

### Archivos de Test

| Patrón | Uso |
|--------|-----|
| `*.test.ts` | Tests unitarios de funciones/utilities |
| `*.test.tsx` | Tests de componentes React |
| `*.spec.ts` | Tests E2E con Playwright |

### Ubicación de Tests

**Recomendación**: Colocar tests junto al código que testean (colocation).

```
✅ Recomendado:
src/components/ui/button.tsx
src/components/ui/button.test.tsx

❌ Evitar:
src/components/ui/button.tsx
src/__tests__/components/ui/button.test.tsx
```

**Ventajas de colocation:**
- Fácil encontrar tests relacionados
- Refactoring más sencillo (mover componente = mover test)
- Visibilidad de qué código tiene tests

---

## Configuración de VS Code

### .vscode/settings.json

```json
{
  "vitest.enable": true,
  "vitest.commandLine": "npx vitest",
  "testing.automaticallyOpenPeekView": "never",
  "editor.codeActionsOnSave": {
    "source.organizeImports": "explicit"
  }
}
```

### Extensiones Recomendadas

- **Vitest** - ZixuanChen.vitest-explorer
- **Playwright Test for VSCode** - ms-playwright.playwright
- **Testing Library Snippets** - afzalsayed96.testing-library-snippets

---

## Variables de Entorno para Testing

### .env.test

```env
VITE_API_BASE_URL=http://localhost:5000/api
VITE_ENVIRONMENT=test
VITE_ENABLE_MSW=true
```

### Uso en tests

```typescript
// vitest.config.ts
export default defineConfig({
  test: {
    env: {
      VITE_API_BASE_URL: 'http://localhost:5000/api',
      VITE_ENVIRONMENT: 'test',
    },
  },
})
```

---

## Verificación de Instalación

### Test de verificación

Crear `src/test/smoke.test.ts`:

```typescript
import { describe, it, expect } from 'vitest'

describe('Test Setup', () => {
  it('should run tests correctly', () => {
    expect(true).toBe(true)
  })

  it('should have access to DOM matchers', () => {
    const element = document.createElement('div')
    element.textContent = 'Hello'
    document.body.appendChild(element)
    
    expect(element).toBeInTheDocument()
    expect(element).toHaveTextContent('Hello')
  })
})
```

### Ejecutar verificación

```bash
npm run test -- src/test/smoke.test.ts
```

**Salida esperada:**
```
✓ Test Setup
  ✓ should run tests correctly
  ✓ should have access to DOM matchers

Test Files  1 passed (1)
Tests       2 passed (2)
```

---

## Troubleshooting

### Error: "document is not defined"

**Causa**: Falta configurar el entorno jsdom.

**Solución**:
```typescript
// vite.config.ts
test: {
  environment: 'jsdom',
}
```

### Error: "toBeInTheDocument is not a function"

**Causa**: Falta importar jest-dom matchers.

**Solución**:
```typescript
// src/test/setup.ts
import '@testing-library/jest-dom'
```

### Error: "Cannot find module '@/...'"

**Causa**: Alias no configurado para tests.

**Solución**:
```typescript
// vite.config.ts
resolve: {
  alias: {
    '@': path.resolve(__dirname, './src'),
  },
},
```

### MSW no intercepta requests

**Causa**: Servidor no iniciado o handlers no configurados.

**Solución**: Verificar que `server.listen()` se llama en setup.ts.

---

## Próximos Pasos

Una vez completada la configuración:

1. → [02 - Tests Unitarios](02-tests-unitarios.md)
2. → [03 - Tests de Componentes](03-tests-componentes.md)
3. → [04 - Mocking de API](04-mocking-api.md)
