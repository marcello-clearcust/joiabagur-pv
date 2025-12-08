# GitHub Actions - CI/CD

[← Volver al índice](../../testing-backend.md)

## Workflow Principal

```yaml
# .github/workflows/backend-tests.yml
name: Backend Tests

on:
  push:
    branches: [main, develop]
    paths:
      - 'backend/**'
      - '.github/workflows/backend-tests.yml'
  pull_request:
    branches: [main, develop]
    paths:
      - 'backend/**'

env:
  DOTNET_VERSION: '10.0.x'

jobs:
  unit-tests:
    name: Unit Tests
    runs-on: ubuntu-latest
    
    steps:
      - name: Checkout código
        uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: ${{ env.DOTNET_VERSION }}

      - name: Restaurar dependencias
        run: dotnet restore backend/Joyeria.sln

      - name: Build
        run: dotnet build backend/Joyeria.sln --no-restore --configuration Release

      - name: Ejecutar tests unitarios
        run: |
          dotnet test backend/tests/Joyeria.UnitTests/Joyeria.UnitTests.csproj \
            --no-build \
            --configuration Release \
            --logger "trx;LogFileName=unit-test-results.trx" \
            --collect:"XPlat Code Coverage" \
            --results-directory ./TestResults

      - name: Publicar resultados de tests
        uses: dorny/test-reporter@v1
        if: always()
        with:
          name: Unit Test Results
          path: TestResults/*.trx
          reporter: dotnet-trx
          fail-on-error: true

      - name: Publicar cobertura de código
        uses: codecov/codecov-action@v4
        if: always()
        with:
          files: ./TestResults/**/coverage.cobertura.xml
          fail_ci_if_error: false

  integration-tests:
    name: Integration Tests
    runs-on: ubuntu-latest
    needs: unit-tests
    
    services:
      postgres:
        image: postgres:15-alpine
        env:
          POSTGRES_DB: joyeria_test
          POSTGRES_USER: test_user
          POSTGRES_PASSWORD: test_password
        ports:
          - 5432:5432
        options: >-
          --health-cmd pg_isready
          --health-interval 10s
          --health-timeout 5s
          --health-retries 5

    steps:
      - name: Checkout código
        uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: ${{ env.DOTNET_VERSION }}

      - name: Restaurar dependencias
        run: dotnet restore backend/Joyeria.sln

      - name: Build
        run: dotnet build backend/Joyeria.sln --no-restore --configuration Release

      - name: Ejecutar tests de integración
        run: |
          dotnet test backend/tests/Joyeria.IntegrationTests/Joyeria.IntegrationTests.csproj \
            --no-build \
            --configuration Release \
            --logger "trx;LogFileName=integration-test-results.trx" \
            --results-directory ./TestResults
        env:
          ConnectionStrings__DefaultConnection: "Host=localhost;Port=5432;Database=joyeria_test;Username=test_user;Password=test_password"

      - name: Publicar resultados de tests
        uses: dorny/test-reporter@v1
        if: always()
        with:
          name: Integration Test Results
          path: TestResults/*.trx
          reporter: dotnet-trx
          fail-on-error: true

  build-check:
    name: Build Check
    runs-on: ubuntu-latest
    needs: [unit-tests, integration-tests]
    if: github.ref == 'refs/heads/main'
    
    steps:
      - name: Checkout código
        uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: ${{ env.DOTNET_VERSION }}

      - name: Build Release
        run: |
          dotnet publish backend/Joyeria.API/Joyeria.API.csproj \
            --configuration Release \
            --output ./publish

      - name: Build Docker image
        run: |
          docker build -t joyeria-backend:${{ github.sha }} ./backend

      - name: Verificar imagen Docker
        run: |
          docker run --rm joyeria-backend:${{ github.sha }} --version || true
```

---

## Workflow de Pull Request

```yaml
# .github/workflows/pr-checks.yml
name: PR Checks

on:
  pull_request:
    branches: [main, develop]

jobs:
  lint-and-format:
    name: Lint & Format
    runs-on: ubuntu-latest
    
    steps:
      - uses: actions/checkout@v4
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'

      - name: Verificar formato
        run: dotnet format backend/Joyeria.sln --verify-no-changes --verbosity diagnostic

  security-scan:
    name: Security Scan
    runs-on: ubuntu-latest
    
    steps:
      - uses: actions/checkout@v4
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'

      - name: Restaurar dependencias
        run: dotnet restore backend/Joyeria.sln

      - name: Escanear vulnerabilidades
        run: dotnet list backend/Joyeria.sln package --vulnerable --include-transitive
```

---

## Workflow Avanzado con Matriz, Caché y Artifacts

```yaml
# .github/workflows/backend-tests-advanced.yml
name: Backend Tests (Advanced)

on:
  push:
    branches: [main, develop]
    paths:
      - 'backend/**'
      - '.github/workflows/backend-tests-advanced.yml'
  pull_request:
    branches: [main, develop]

env:
  DOTNET_VERSION: '10.0.x'
  DOTNET_SKIP_FIRST_TIME_EXPERIENCE: true
  DOTNET_CLI_TELEMETRY_OPTOUT: true

jobs:
  build:
    name: Build & Cache
    runs-on: ubuntu-latest
    
    outputs:
      cache-key: ${{ steps.cache-key.outputs.key }}
    
    steps:
      - name: Checkout
        uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: ${{ env.DOTNET_VERSION }}

      # Cache de paquetes NuGet
      - name: Cache NuGet packages
        uses: actions/cache@v4
        with:
          path: ~/.nuget/packages
          key: ${{ runner.os }}-nuget-${{ hashFiles('**/*.csproj') }}
          restore-keys: |
            ${{ runner.os }}-nuget-

      - name: Generate cache key
        id: cache-key
        run: echo "key=${{ runner.os }}-build-${{ github.sha }}" >> $GITHUB_OUTPUT

      - name: Restore dependencies
        run: dotnet restore backend/Joyeria.sln

      - name: Build
        run: dotnet build backend/Joyeria.sln --no-restore --configuration Release

      # Cache del build para jobs posteriores
      - name: Cache build output
        uses: actions/cache@v4
        with:
          path: |
            backend/**/bin
            backend/**/obj
          key: ${{ steps.cache-key.outputs.key }}

  unit-tests:
    name: Unit Tests
    runs-on: ubuntu-latest
    needs: build
    
    strategy:
      fail-fast: false
      matrix:
        project:
          - Joyeria.UnitTests
    
    steps:
      - name: Checkout
        uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: ${{ env.DOTNET_VERSION }}

      - name: Restore NuGet cache
        uses: actions/cache@v4
        with:
          path: ~/.nuget/packages
          key: ${{ runner.os }}-nuget-${{ hashFiles('**/*.csproj') }}

      - name: Restore build cache
        uses: actions/cache@v4
        with:
          path: |
            backend/**/bin
            backend/**/obj
          key: ${{ needs.build.outputs.cache-key }}

      - name: Run tests
        run: |
          dotnet test backend/tests/${{ matrix.project }}/${{ matrix.project }}.csproj \
            --no-build \
            --configuration Release \
            --logger "trx;LogFileName=${{ matrix.project }}-results.trx" \
            --logger "console;verbosity=detailed" \
            --collect:"XPlat Code Coverage" \
            --results-directory ./TestResults/${{ matrix.project }}

      - name: Upload test results
        uses: actions/upload-artifact@v4
        if: always()
        with:
          name: test-results-${{ matrix.project }}
          path: ./TestResults/${{ matrix.project }}
          retention-days: 7

      - name: Publish test report
        uses: dorny/test-reporter@v1
        if: always()
        with:
          name: ${{ matrix.project }} Results
          path: ./TestResults/${{ matrix.project }}/*.trx
          reporter: dotnet-trx
          fail-on-error: true

  integration-tests:
    name: Integration Tests
    runs-on: ubuntu-latest
    needs: build
    
    services:
      postgres:
        image: postgres:15-alpine
        env:
          POSTGRES_DB: joyeria_test
          POSTGRES_USER: test_user
          POSTGRES_PASSWORD: test_password
        ports:
          - 5432:5432
        options: >-
          --health-cmd pg_isready
          --health-interval 10s
          --health-timeout 5s
          --health-retries 5

    steps:
      - name: Checkout
        uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: ${{ env.DOTNET_VERSION }}

      - name: Restore NuGet cache
        uses: actions/cache@v4
        with:
          path: ~/.nuget/packages
          key: ${{ runner.os }}-nuget-${{ hashFiles('**/*.csproj') }}

      - name: Restore build cache
        uses: actions/cache@v4
        with:
          path: |
            backend/**/bin
            backend/**/obj
          key: ${{ needs.build.outputs.cache-key }}

      - name: Run integration tests
        run: |
          dotnet test backend/tests/Joyeria.IntegrationTests/Joyeria.IntegrationTests.csproj \
            --no-build \
            --configuration Release \
            --logger "trx;LogFileName=integration-results.trx" \
            --collect:"XPlat Code Coverage" \
            --results-directory ./TestResults/Integration
        env:
          ConnectionStrings__DefaultConnection: "Host=localhost;Port=5432;Database=joyeria_test;Username=test_user;Password=test_password"
          JWT__SecretKey: "test-secret-key-minimum-32-characters-long-for-tests"
          JWT__Issuer: "JoyeriaAPI-Test"
          JWT__Audience: "JoyeriaClient-Test"

      - name: Upload test results
        uses: actions/upload-artifact@v4
        if: always()
        with:
          name: test-results-integration
          path: ./TestResults/Integration
          retention-days: 7

  summary:
    name: Test Summary
    runs-on: ubuntu-latest
    needs: [unit-tests, integration-tests]
    if: always()
    
    steps:
      - name: Check all jobs
        run: |
          echo "## Test Results Summary" >> $GITHUB_STEP_SUMMARY
          echo "" >> $GITHUB_STEP_SUMMARY
          echo "| Job | Status |" >> $GITHUB_STEP_SUMMARY
          echo "|-----|--------|" >> $GITHUB_STEP_SUMMARY
          echo "| Unit Tests | ${{ needs.unit-tests.result }} |" >> $GITHUB_STEP_SUMMARY
          echo "| Integration Tests | ${{ needs.integration-tests.result }} |" >> $GITHUB_STEP_SUMMARY

      - name: Fail if any job failed
        if: |
          needs.unit-tests.result == 'failure' ||
          needs.integration-tests.result == 'failure'
        run: exit 1
```

---

## Badges para README

```markdown
<!-- Añadir al README.md del proyecto -->

# Joyería POS - Backend

![Build Status](https://github.com/tu-usuario/joyeria-pos/actions/workflows/backend-tests.yml/badge.svg)
![Coverage](https://codecov.io/gh/tu-usuario/joyeria-pos/branch/main/graph/badge.svg)
![.NET Version](https://img.shields.io/badge/.NET-10.0-purple)
![License](https://img.shields.io/badge/license-MIT-blue)

## Status de Tests

| Branch | Unit Tests | Integration | Coverage |
|--------|------------|-------------|----------|
| main   | ![Unit](https://github.com/tu-usuario/joyeria-pos/actions/workflows/backend-tests.yml/badge.svg?branch=main) | ![Integration](https://github.com/tu-usuario/joyeria-pos/actions/workflows/backend-tests.yml/badge.svg?branch=main) | ![Coverage](https://codecov.io/gh/tu-usuario/joyeria-pos/branch/main/graph/badge.svg) |
| develop | ![Unit](https://github.com/tu-usuario/joyeria-pos/actions/workflows/backend-tests.yml/badge.svg?branch=develop) | ![Integration](https://github.com/tu-usuario/joyeria-pos/actions/workflows/backend-tests.yml/badge.svg?branch=develop) | ![Coverage](https://codecov.io/gh/tu-usuario/joyeria-pos/branch/develop/graph/badge.svg) |
```

---

## Configuración de Secrets

```yaml
# Secrets necesarios en GitHub (Settings > Secrets and variables > Actions)

# Para Codecov (opcional pero recomendado)
CODECOV_TOKEN: "tu-token-de-codecov"

# Para notificaciones Slack (opcional)
SLACK_WEBHOOK_URL: "https://hooks.slack.com/services/..."

# Para deployment (si aplica)
AZURE_CREDENTIALS: "{ ... }"
AWS_ACCESS_KEY_ID: "..."
AWS_SECRET_ACCESS_KEY: "..."
```

