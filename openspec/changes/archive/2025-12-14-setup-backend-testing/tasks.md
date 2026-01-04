# Tasks: Setup Backend Testing CI/CD

## 1. GitHub Actions Workflow

- [x] 1.1 Create `.github/workflows/test-backend.yml` workflow file
- [x] 1.2 Configure workflow triggers (push to main/develop, pull requests)
- [x] 1.3 Set up .NET SDK installation step
- [x] 1.4 Configure dependency caching for NuGet packages
- [x] 1.5 Add PostgreSQL service container for integration tests
- [x] 1.6 Add test execution step with `dotnet test`
- [x] 1.7 Configure test result reporting (JUnit format for GitHub)

## 2. Code Coverage Configuration

- [x] 2.1 Add Coverlet MSBuild package reference (if not present)
- [x] 2.2 Configure coverage collection in test command (`--collect:"XPlat Code Coverage"`)
- [x] 2.3 Set minimum coverage threshold at 70%
- [x] 2.4 Generate HTML coverage report using ReportGenerator
- [x] 2.5 Upload coverage report as workflow artifact

## 3. Documentation Updates

- [x] 3.1 Add test status badge to `backend/README.md`
- [x] 3.2 Add coverage badge to `backend/README.md`
- [x] 3.3 Document how to run tests locally with coverage
- [x] 3.4 Document coverage threshold and expectations

## 4. Validation

- [x] 4.1 Verify workflow executes successfully on push
- [x] 4.2 Verify integration tests run with PostgreSQL container
- [x] 4.3 Verify coverage report is generated and accessible
- [x] 4.4 Verify workflow fails when coverage is below threshold

## Implementation Notes

### 1.5 PostgreSQL Container

The integration tests use **Testcontainers** which manages its own PostgreSQL container at runtime. No service container is needed in the GitHub Actions workflow - Docker is available by default on `ubuntu-latest` runners.

### 2.1 Coverlet Package

The `coverlet.collector` package was already present in `JoiabagurPV.Tests.csproj`.

### 4.x Validation

Workflow validation will occur on the first push/PR after merging. The workflow is configured to:
- Run tests with Testcontainers (Docker is available on GitHub runners)
- Collect coverage using Coverlet
- Generate HTML reports with ReportGenerator
- Enforce 70% minimum line coverage threshold
- Upload artifacts for 30 days
