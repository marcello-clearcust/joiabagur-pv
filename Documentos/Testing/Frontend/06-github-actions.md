# 06 - GitHub Actions para Testing Frontend

Configuración de CI/CD para ejecutar tests de frontend en cada PR y push.

---

## Workflow Básico

### .github/workflows/frontend-tests.yml

```yaml
name: Frontend Tests

on:
  push:
    branches: [main, develop]
    paths:
      - 'frontend/**'
      - '.github/workflows/frontend-tests.yml'
  pull_request:
    branches: [main, develop]
    paths:
      - 'frontend/**'
      - '.github/workflows/frontend-tests.yml'

defaults:
  run:
    working-directory: frontend

jobs:
  # ==================== LINT Y TYPE CHECK ====================
  lint:
    name: Lint & Type Check
    runs-on: ubuntu-latest
    steps:
      - name: Checkout code
        uses: actions/checkout@v4

      - name: Setup Node.js
        uses: actions/setup-node@v4
        with:
          node-version: '20'
          cache: 'npm'
          cache-dependency-path: frontend/package-lock.json

      - name: Install dependencies
        run: npm ci

      - name: Run ESLint
        run: npm run lint

      - name: Run TypeScript type check
        run: npm run typecheck

  # ==================== TESTS UNITARIOS ====================
  unit-tests:
    name: Unit & Component Tests
    runs-on: ubuntu-latest
    needs: lint
    steps:
      - name: Checkout code
        uses: actions/checkout@v4

      - name: Setup Node.js
        uses: actions/setup-node@v4
        with:
          node-version: '20'
          cache: 'npm'
          cache-dependency-path: frontend/package-lock.json

      - name: Install dependencies
        run: npm ci

      - name: Run unit tests with coverage
        run: npm run test:coverage

      - name: Upload coverage report
        uses: actions/upload-artifact@v4
        with:
          name: coverage-report
          path: frontend/coverage/
          retention-days: 7

      - name: Upload coverage to Codecov
        uses: codecov/codecov-action@v4
        with:
          file: frontend/coverage/lcov.info
          flags: frontend-unit
          fail_ci_if_error: false
        env:
          CODECOV_TOKEN: ${{ secrets.CODECOV_TOKEN }}

  # ==================== TESTS E2E ====================
  e2e-tests:
    name: E2E Tests (${{ matrix.browser }})
    runs-on: ubuntu-latest
    needs: unit-tests
    strategy:
      fail-fast: false
      matrix:
        browser: [chromium, firefox, webkit]
    steps:
      - name: Checkout code
        uses: actions/checkout@v4

      - name: Setup Node.js
        uses: actions/setup-node@v4
        with:
          node-version: '20'
          cache: 'npm'
          cache-dependency-path: frontend/package-lock.json

      - name: Install dependencies
        run: npm ci

      - name: Install Playwright browsers
        run: npx playwright install --with-deps ${{ matrix.browser }}

      - name: Build frontend
        run: npm run build

      - name: Run E2E tests
        run: npx playwright test --project=${{ matrix.browser }}
        env:
          CI: true

      - name: Upload test results
        if: always()
        uses: actions/upload-artifact@v4
        with:
          name: playwright-report-${{ matrix.browser }}
          path: frontend/playwright-report/
          retention-days: 7

      - name: Upload test artifacts
        if: failure()
        uses: actions/upload-artifact@v4
        with:
          name: playwright-artifacts-${{ matrix.browser }}
          path: |
            frontend/test-results/
            frontend/playwright-report/
          retention-days: 7

  # ==================== BUILD ====================
  build:
    name: Build
    runs-on: ubuntu-latest
    needs: [unit-tests]
    steps:
      - name: Checkout code
        uses: actions/checkout@v4

      - name: Setup Node.js
        uses: actions/setup-node@v4
        with:
          node-version: '20'
          cache: 'npm'
          cache-dependency-path: frontend/package-lock.json

      - name: Install dependencies
        run: npm ci

      - name: Build for production
        run: npm run build
        env:
          VITE_API_BASE_URL: ${{ vars.VITE_API_BASE_URL }}

      - name: Upload build artifact
        uses: actions/upload-artifact@v4
        with:
          name: frontend-build
          path: frontend/dist/
          retention-days: 7
```

---

## Workflow Optimizado con Caché

### .github/workflows/frontend-tests-optimized.yml

```yaml
name: Frontend Tests (Optimized)

on:
  push:
    branches: [main, develop]
    paths:
      - 'frontend/**'
  pull_request:
    branches: [main, develop]
    paths:
      - 'frontend/**'

defaults:
  run:
    working-directory: frontend

env:
  NODE_VERSION: '20'

jobs:
  # ==================== SETUP COMPARTIDO ====================
  setup:
    name: Setup
    runs-on: ubuntu-latest
    outputs:
      cache-key: ${{ steps.cache-key.outputs.key }}
    steps:
      - name: Checkout code
        uses: actions/checkout@v4

      - name: Generate cache key
        id: cache-key
        run: echo "key=npm-${{ hashFiles('frontend/package-lock.json') }}" >> $GITHUB_OUTPUT

      - name: Setup Node.js
        uses: actions/setup-node@v4
        with:
          node-version: ${{ env.NODE_VERSION }}
          cache: 'npm'
          cache-dependency-path: frontend/package-lock.json

      - name: Cache node_modules
        id: cache-node-modules
        uses: actions/cache@v4
        with:
          path: frontend/node_modules
          key: ${{ runner.os }}-node-${{ hashFiles('frontend/package-lock.json') }}

      - name: Install dependencies
        if: steps.cache-node-modules.outputs.cache-hit != 'true'
        run: npm ci

  # ==================== LINT (PARALELO) ====================
  lint:
    name: Lint
    runs-on: ubuntu-latest
    needs: setup
    steps:
      - uses: actions/checkout@v4

      - name: Setup Node.js
        uses: actions/setup-node@v4
        with:
          node-version: ${{ env.NODE_VERSION }}

      - name: Restore node_modules cache
        uses: actions/cache@v4
        with:
          path: frontend/node_modules
          key: ${{ runner.os }}-node-${{ hashFiles('frontend/package-lock.json') }}

      - name: Run ESLint
        run: npm run lint

  # ==================== TYPE CHECK (PARALELO) ====================
  typecheck:
    name: Type Check
    runs-on: ubuntu-latest
    needs: setup
    steps:
      - uses: actions/checkout@v4

      - name: Setup Node.js
        uses: actions/setup-node@v4
        with:
          node-version: ${{ env.NODE_VERSION }}

      - name: Restore node_modules cache
        uses: actions/cache@v4
        with:
          path: frontend/node_modules
          key: ${{ runner.os }}-node-${{ hashFiles('frontend/package-lock.json') }}

      - name: Run TypeScript type check
        run: npm run typecheck

  # ==================== UNIT TESTS (PARALELO) ====================
  unit-tests:
    name: Unit Tests
    runs-on: ubuntu-latest
    needs: setup
    steps:
      - uses: actions/checkout@v4

      - name: Setup Node.js
        uses: actions/setup-node@v4
        with:
          node-version: ${{ env.NODE_VERSION }}

      - name: Restore node_modules cache
        uses: actions/cache@v4
        with:
          path: frontend/node_modules
          key: ${{ runner.os }}-node-${{ hashFiles('frontend/package-lock.json') }}

      - name: Run tests with coverage
        run: npm run test:coverage

      - name: Upload coverage
        uses: codecov/codecov-action@v4
        with:
          file: frontend/coverage/lcov.info
          flags: frontend
        env:
          CODECOV_TOKEN: ${{ secrets.CODECOV_TOKEN }}

  # ==================== E2E TESTS ====================
  e2e-tests:
    name: E2E Tests
    runs-on: ubuntu-latest
    needs: [lint, typecheck, unit-tests]
    steps:
      - uses: actions/checkout@v4

      - name: Setup Node.js
        uses: actions/setup-node@v4
        with:
          node-version: ${{ env.NODE_VERSION }}

      - name: Restore node_modules cache
        uses: actions/cache@v4
        with:
          path: frontend/node_modules
          key: ${{ runner.os }}-node-${{ hashFiles('frontend/package-lock.json') }}

      - name: Cache Playwright browsers
        uses: actions/cache@v4
        id: playwright-cache
        with:
          path: ~/.cache/ms-playwright
          key: ${{ runner.os }}-playwright-${{ hashFiles('frontend/package-lock.json') }}

      - name: Install Playwright browsers
        if: steps.playwright-cache.outputs.cache-hit != 'true'
        run: npx playwright install --with-deps

      - name: Install Playwright deps (if cached)
        if: steps.playwright-cache.outputs.cache-hit == 'true'
        run: npx playwright install-deps

      - name: Build frontend
        run: npm run build

      - name: Run E2E tests
        run: npx playwright test
        env:
          CI: true

      - name: Upload Playwright report
        if: always()
        uses: actions/upload-artifact@v4
        with:
          name: playwright-report
          path: frontend/playwright-report/
          retention-days: 7
```

---

## Scripts de package.json

```json
{
  "scripts": {
    "dev": "vite",
    "build": "tsc && vite build",
    "preview": "vite preview",
    "lint": "eslint src --ext .ts,.tsx --report-unused-disable-directives --max-warnings 0",
    "lint:fix": "eslint src --ext .ts,.tsx --fix",
    "typecheck": "tsc --noEmit",
    "test": "vitest run",
    "test:watch": "vitest",
    "test:coverage": "vitest run --coverage",
    "test:ui": "vitest --ui",
    "test:e2e": "playwright test",
    "test:e2e:ui": "playwright test --ui",
    "test:e2e:headed": "playwright test --headed",
    "test:all": "npm run test && npm run test:e2e"
  }
}
```

---

## Configuración de Caché

### Caché de Node Modules

```yaml
- name: Cache node_modules
  uses: actions/cache@v4
  with:
    path: frontend/node_modules
    key: ${{ runner.os }}-node-${{ hashFiles('frontend/package-lock.json') }}
    restore-keys: |
      ${{ runner.os }}-node-
```

### Caché de Playwright

```yaml
- name: Cache Playwright browsers
  uses: actions/cache@v4
  id: playwright-cache
  with:
    path: ~/.cache/ms-playwright
    key: ${{ runner.os }}-playwright-${{ hashFiles('frontend/package-lock.json') }}
    restore-keys: |
      ${{ runner.os }}-playwright-
```

### Caché de Vite Build

```yaml
- name: Cache Vite build
  uses: actions/cache@v4
  with:
    path: frontend/node_modules/.vite
    key: ${{ runner.os }}-vite-${{ hashFiles('frontend/src/**') }}
    restore-keys: |
      ${{ runner.os }}-vite-
```

---

## Reporte de Tests en PR

### Comentario automático en PR

```yaml
- name: Publish Test Results
  uses: EnricoMi/publish-unit-test-result-action@v2
  if: always()
  with:
    files: |
      frontend/test-results/**/*.xml
    check_name: 'Frontend Test Results'
    comment_title: '🧪 Frontend Test Results'
```

### Badge de Estado

Agregar al README.md:

```markdown
![Frontend Tests](https://github.com/OWNER/REPO/actions/workflows/frontend-tests.yml/badge.svg)
```

---

## Protección de Ramas

### Configurar en GitHub

1. Settings → Branches → Add rule
2. Branch name pattern: `main`
3. Require status checks to pass:
   - `lint`
   - `typecheck`
   - `unit-tests`
   - `e2e-tests`
4. Require branches to be up to date

---

## Variables y Secrets

### Variables de Entorno

En GitHub: Settings → Secrets and variables → Actions → Variables

| Variable | Valor | Descripción |
|----------|-------|-------------|
| `VITE_API_BASE_URL` | `https://api.joyeria.com/api` | URL del API para builds |

### Secrets

En GitHub: Settings → Secrets and variables → Actions → Secrets

| Secret | Descripción |
|--------|-------------|
| `CODECOV_TOKEN` | Token para subir cobertura a Codecov |

---

## Workflow para Deploy

**JoiabagurPV (producción):** el build del frontend va en **`Dockerfile.bundled`** y el despliegue activo es **`deploy-aws-ec2.yml`** (OIDC). El YAML siguiente es un **ejemplo genérico** S3+CloudFront, no el pipeline actual del repo.

### .github/workflows/frontend-deploy.yml (ejemplo genérico)

```yaml
name: Frontend Deploy

on:
  push:
    branches: [main]
    paths:
      - 'frontend/**'
  workflow_dispatch:

defaults:
  run:
    working-directory: frontend

jobs:
  deploy:
    name: Build and Deploy
    runs-on: ubuntu-latest
    environment: production
    steps:
      - uses: actions/checkout@v4

      - name: Setup Node.js
        uses: actions/setup-node@v4
        with:
          node-version: '20'
          cache: 'npm'
          cache-dependency-path: frontend/package-lock.json

      - name: Install dependencies
        run: npm ci

      - name: Build
        run: npm run build
        env:
          VITE_API_BASE_URL: ${{ vars.VITE_API_BASE_URL }}
          VITE_ENVIRONMENT: production

      - name: Deploy to S3/CloudFront
        uses: aws-actions/configure-aws-credentials@v4
        with:
          aws-access-key-id: ${{ secrets.AWS_ACCESS_KEY_ID }}
          aws-secret-access-key: ${{ secrets.AWS_SECRET_ACCESS_KEY }}
          aws-region: us-east-1

      - name: Sync to S3
        run: aws s3 sync dist/ s3://${{ vars.S3_BUCKET }} --delete

      - name: Invalidate CloudFront
        run: |
          aws cloudfront create-invalidation \
            --distribution-id ${{ vars.CLOUDFRONT_DISTRIBUTION_ID }} \
            --paths "/*"
```

---

## Tiempos Estimados

| Job | Tiempo Estimado | Con Caché |
|-----|-----------------|-----------|
| Lint | ~30s | ~15s |
| Type Check | ~20s | ~10s |
| Unit Tests | ~1min | ~45s |
| E2E Tests (1 browser) | ~3min | ~2min |
| E2E Tests (3 browsers) | ~5min | ~3min |
| Build | ~1min | ~30s |
| **Total (paralelo)** | **~6min** | **~4min** |

---

## Troubleshooting

### Tests E2E fallan en CI pero no en local

```yaml
# Agregar más información de debug
- name: Run E2E tests
  run: npx playwright test --reporter=list
  env:
    CI: true
    DEBUG: pw:api
```

### Timeouts en CI

```typescript
// playwright.config.ts
export default defineConfig({
  timeout: process.env.CI ? 60000 : 30000,
  expect: {
    timeout: process.env.CI ? 10000 : 5000,
  },
})
```

### Screenshots de fallos

```yaml
- name: Upload screenshots on failure
  if: failure()
  uses: actions/upload-artifact@v4
  with:
    name: test-screenshots
    path: frontend/test-results/**/*.png
```

---

## Próximos Pasos

1. → [07 - Cobertura de Código](07-cobertura-codigo.md) - Coverage

