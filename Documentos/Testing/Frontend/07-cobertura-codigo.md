# 07 - Cobertura de Código

Configuración de cobertura de código con Vitest y reportes en CI/CD.

---

## Configuración de Vitest Coverage

### vite.config.ts

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
    
    // Configuración de cobertura
    coverage: {
      // Proveedor de cobertura (v8 es más rápido, istanbul más preciso)
      provider: 'v8',
      
      // Habilitar cobertura
      enabled: true,
      
      // Reporters
      reporter: [
        'text',           // Muestra en consola
        'text-summary',   // Resumen en consola
        'html',           // Reporte HTML navegable
        'lcov',           // Para Codecov/Coveralls
        'json',           // Para procesamiento
        'json-summary',   // Resumen en JSON
      ],
      
      // Directorio de salida
      reportsDirectory: './coverage',
      
      // Archivos a incluir
      include: [
        'src/**/*.{ts,tsx}',
      ],
      
      // Archivos a excluir
      exclude: [
        'node_modules/',
        'src/test/**',
        'src/**/*.test.{ts,tsx}',
        'src/**/*.spec.{ts,tsx}',
        'src/**/*.d.ts',
        'src/main.tsx',
        'src/vite-env.d.ts',
        '**/*.config.{ts,tsx}',
        '**/types.ts',
        '**/index.ts', // Archivos de re-export
      ],
      
      // Umbrales mínimos de cobertura
      thresholds: {
        // Umbrales globales
        lines: 70,
        functions: 70,
        branches: 70,
        statements: 70,
        
        // Umbrales específicos por archivo/carpeta
        // 'src/components/**/*.tsx': {
        //   lines: 80,
        //   functions: 80,
        // },
        // 'src/services/**/*.ts': {
        //   lines: 90,
        //   functions: 90,
        // },
      },
      
      // Limpiar reporte anterior
      clean: true,
      
      // Generar reporte aunque fallen tests
      reportOnFailure: true,
      
      // Incluir archivos sin tests en el reporte
      all: true,
      
      // Skip archivos completamente cubiertos en el output
      skipFull: false,
    },
  },
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
    "test:coverage:watch": "vitest --coverage",
    "test:coverage:html": "vitest run --coverage && open coverage/index.html",
    "test:coverage:check": "vitest run --coverage --coverage.thresholds.100"
  }
}
```

---

## Ejecutar Tests con Cobertura

### Comando Básico

```bash
npm run test:coverage
```

### Salida en Consola

```
 ✓ src/components/ui/button.test.tsx (5 tests) 23ms
 ✓ src/hooks/use-auth.test.ts (8 tests) 45ms
 ✓ src/services/products.service.test.ts (6 tests) 32ms

 Test Files  3 passed (3)
      Tests  19 passed (19)
   Start at  10:30:45
   Duration  2.34s (transform 234ms, setup 89ms, collect 456ms, tests 100ms)

 % Coverage report from v8
-----------------------|---------|----------|---------|---------|-------------------
File                   | % Stmts | % Branch | % Funcs | % Lines | Uncovered Line #s
-----------------------|---------|----------|---------|---------|-------------------
All files              |   78.45 |    72.33 |   81.25 |   78.45 |
 components/ui         |   85.71 |    80.00 |   90.00 |   85.71 |
  button.tsx           |   85.71 |    80.00 |   90.00 |   85.71 | 45-48
 hooks                 |   72.00 |    65.00 |   75.00 |   72.00 |
  use-auth.ts          |   72.00 |    65.00 |   75.00 |   72.00 | 23-25,67-72
 services              |   80.00 |    75.00 |   83.33 |   80.00 |
  products.service.ts  |   80.00 |    75.00 |   83.33 |   80.00 | 89-95
-----------------------|---------|----------|---------|---------|-------------------
```

---

## Reporte HTML

### Generar y Abrir

```bash
# Generar reporte
npm run test:coverage

# Abrir en navegador (macOS)
open coverage/index.html

# Abrir en navegador (Windows)
start coverage/index.html

# Abrir en navegador (Linux)
xdg-open coverage/index.html
```

### Estructura del Reporte

```
coverage/
├── index.html              # Página principal
├── lcov.info               # Formato LCOV (para CI)
├── coverage-summary.json   # Resumen en JSON
├── coverage-final.json     # Datos completos
└── src/
    ├── components/
    │   └── ui/
    │       └── button.tsx.html
    ├── hooks/
    │   └── use-auth.ts.html
    └── services/
        └── products.service.ts.html
```

---

## Integración con Codecov

### GitHub Actions

```yaml
- name: Upload coverage to Codecov
  uses: codecov/codecov-action@v4
  with:
    # Archivo de cobertura
    file: frontend/coverage/lcov.info
    
    # Flags para identificar el tipo de cobertura
    flags: frontend-unit
    
    # Nombre del reporte
    name: frontend-coverage
    
    # No fallar si Codecov falla
    fail_ci_if_error: false
    
    # Verbose para debugging
    verbose: true
  env:
    CODECOV_TOKEN: ${{ secrets.CODECOV_TOKEN }}
```

### codecov.yml

Crear en la raíz del repositorio:

```yaml
codecov:
  require_ci_to_pass: true

coverage:
  precision: 2
  round: down
  range: "60...100"
  
  status:
    project:
      default:
        target: 70%
        threshold: 2%
        
    patch:
      default:
        target: 80%
        threshold: 5%

parsers:
  gcov:
    branch_detection:
      conditional: yes
      loop: yes
      method: no
      macro: no

comment:
  layout: "reach,diff,flags,files,footer"
  behavior: default
  require_changes: true
  require_base: false
  require_head: true

flags:
  frontend-unit:
    paths:
      - frontend/src/
    carryforward: true
    
  frontend-e2e:
    paths:
      - frontend/src/
    carryforward: true
```

### Badge en README

```markdown
[![codecov](https://codecov.io/gh/OWNER/REPO/branch/main/graph/badge.svg)](https://codecov.io/gh/OWNER/REPO)
```

---

## Umbrales de Cobertura

### Configuración de Umbrales

```typescript
// vite.config.ts
coverage: {
  thresholds: {
    // Umbrales globales (todo el proyecto)
    lines: 70,
    functions: 70,
    branches: 70,
    statements: 70,
    
    // Umbrales por carpeta
    'src/components/**/*.tsx': {
      lines: 80,
      functions: 80,
      branches: 75,
      statements: 80,
    },
    
    // Umbrales para servicios (más estrictos)
    'src/services/**/*.ts': {
      lines: 90,
      functions: 90,
      branches: 85,
      statements: 90,
    },
    
    // Umbrales para hooks
    'src/hooks/**/*.ts': {
      lines: 85,
      functions: 85,
      branches: 80,
      statements: 85,
    },
  },
}
```

### Fallar CI si no se cumple umbral

```bash
# El test fallará si no se cumplen los umbrales
npm run test:coverage
```

Salida cuando no se cumple:

```
ERROR: Coverage for lines (65.00%) does not meet global threshold (70%)
ERROR: Coverage for functions (60.00%) does not meet global threshold (70%)
```

---

## Excluir Código de Cobertura

### Comentarios de Exclusión

```typescript
// Excluir siguiente línea
/* v8 ignore next */
console.log('Debug')

// Excluir siguiente N líneas
/* v8 ignore next 3 */
if (process.env.NODE_ENV === 'development') {
  console.log('Dev only')
}

// Excluir bloque
/* v8 ignore start */
function devOnlyFunction() {
  // Este código no se contará en cobertura
}
/* v8 ignore stop */
```

### Excluir Archivos en Configuración

```typescript
// vite.config.ts
coverage: {
  exclude: [
    'node_modules/',
    'src/test/**',
    'src/**/*.test.{ts,tsx}',
    'src/**/*.d.ts',
    'src/main.tsx',
    'src/vite-env.d.ts',
    
    // Archivos de configuración
    '**/*.config.{ts,tsx}',
    
    // Tipos y constantes
    '**/types.ts',
    '**/constants.ts',
    
    // Re-exports
    '**/index.ts',
    
    // Mocks
    'src/test/mocks/**',
  ],
}
```

---

## Reporte en Pull Requests

### GitHub Actions con Comentario

```yaml
- name: Coverage Report
  uses: davelosert/vitest-coverage-report-action@v2
  with:
    json-summary-path: frontend/coverage/coverage-summary.json
    json-final-path: frontend/coverage/coverage-final.json
    vite-config-path: frontend/vite.config.ts
    github-token: ${{ secrets.GITHUB_TOKEN }}
```

### Ejemplo de Comentario en PR

```
## Coverage Report 📊

| Category | Coverage | Threshold |
|----------|----------|-----------|
| Lines | 78.45% | 70% ✅ |
| Functions | 81.25% | 70% ✅ |
| Branches | 72.33% | 70% ✅ |
| Statements | 78.45% | 70% ✅ |

### Changed Files

| File | Lines | Functions |
|------|-------|-----------|
| `src/hooks/use-auth.ts` | 72% → 85% | 75% → 90% |
| `src/services/api.ts` | 80% | 83% |
```

---

## Métricas de Cobertura

### Tipos de Cobertura

| Métrica | Descripción | Importancia |
|---------|-------------|-------------|
| **Lines** | Líneas de código ejecutadas | Alta |
| **Statements** | Declaraciones ejecutadas | Alta |
| **Branches** | Ramas condicionales (if/else) | Muy Alta |
| **Functions** | Funciones llamadas | Alta |

### Interpretar Resultados

```
-----------------------|---------|----------|---------|---------|-------------------
File                   | % Stmts | % Branch | % Funcs | % Lines | Uncovered Line #s
-----------------------|---------|----------|---------|---------|-------------------
  use-auth.ts          |   72.00 |    65.00 |   75.00 |   72.00 | 23-25,67-72
```

- **72% Statements**: 72% de las declaraciones se ejecutaron
- **65% Branches**: Solo 65% de los caminos condicionales se probaron ⚠️
- **75% Functions**: 75% de las funciones se llamaron
- **Lines 23-25,67-72**: Líneas específicas sin cobertura

---

## Mejorar Cobertura

### Identificar Código Sin Cubrir

```bash
# Ver reporte detallado en consola
npm run test:coverage -- --coverage.reporter=text

# Ver archivos con baja cobertura
npm run test:coverage -- --coverage.reporter=text-summary
```

### Estrategias para Mejorar

```typescript
// ❌ Código difícil de testear
function processData(data: unknown) {
  try {
    // Lógica compleja
    if (data && typeof data === 'object') {
      if ('items' in data && Array.isArray(data.items)) {
        return data.items.map(item => /* ... */)
      }
    }
    return []
  } catch (error) {
    console.error(error)
    return []
  }
}

// ✅ Código fácil de testear (separar responsabilidades)
function isValidData(data: unknown): data is { items: unknown[] } {
  return data !== null && 
         typeof data === 'object' && 
         'items' in data && 
         Array.isArray((data as { items: unknown[] }).items)
}

function processItems(items: unknown[]) {
  return items.map(item => /* ... */)
}

function processData(data: unknown) {
  if (!isValidData(data)) return []
  return processItems(data.items)
}
```

---

## Cobertura por Componente

### Verificar Cobertura de un Archivo

```bash
# Ver cobertura de un archivo específico
npm run test:coverage -- --coverage.include="src/hooks/use-auth.ts"
```

### Script para Verificar Archivos Nuevos

```bash
#!/bin/bash
# scripts/check-coverage.sh

# Obtener archivos modificados
CHANGED_FILES=$(git diff --name-only HEAD~1 | grep -E '\.(ts|tsx)$' | grep -v '.test.')

for file in $CHANGED_FILES; do
  echo "Checking coverage for: $file"
  npm run test:coverage -- --coverage.include="$file"
done
```

---

## Cobertura Mínima Recomendada

### Por Tipo de Código

| Tipo de Código | Cobertura Recomendada |
|----------------|----------------------|
| **Servicios/API** | 90%+ |
| **Hooks** | 85%+ |
| **Utilities** | 95%+ |
| **Componentes UI** | 75%+ |
| **Componentes Página** | 70%+ |
| **Configuración** | 50%+ (o excluir) |

### Configuración Recomendada

```typescript
coverage: {
  thresholds: {
    // Global (mínimo aceptable)
    lines: 70,
    functions: 70,
    branches: 65,
    statements: 70,
    
    // Código crítico
    'src/services/**/*.ts': {
      lines: 90,
      functions: 90,
      branches: 85,
    },
    
    'src/hooks/**/*.ts': {
      lines: 85,
      functions: 85,
      branches: 80,
    },
    
    'src/lib/**/*.ts': {
      lines: 95,
      functions: 95,
      branches: 90,
    },
  },
}
```

---

## Troubleshooting

### Cobertura muestra 0% para archivos

**Causa**: El archivo no tiene tests o está en la lista de exclusión.

**Solución**:
```typescript
coverage: {
  all: true, // Incluir archivos sin tests
}
```

### Líneas no cubiertas incorrectas

**Causa**: Source maps incorrectos.

**Solución**:
```typescript
coverage: {
  provider: 'v8', // Usar v8 en lugar de istanbul
}
```

### Cobertura no se genera

**Causa**: Tests fallan antes de generar cobertura.

**Solución**:
```typescript
coverage: {
  reportOnFailure: true, // Generar aunque fallen tests
}
```

---

## Conclusión

| Métrica | Umbral Mínimo | Umbral Ideal |
|---------|---------------|--------------|
| Lines | 70% | 85%+ |
| Functions | 70% | 85%+ |
| Branches | 65% | 80%+ |
| Statements | 70% | 85%+ |

**Recomendaciones**:
- ✅ Empezar con umbrales bajos (60-70%) e incrementar gradualmente
- ✅ Enfocarse en cobertura de branches (más importante que lines)
- ✅ Priorizar código crítico (servicios, hooks, utilities)
- ✅ No buscar 100% - genera tests de bajo valor
- ✅ Revisar reporte HTML para entender qué falta

