# Change: Setup Backend Testing CI/CD

## Why

The backend has a comprehensive test suite (unit tests with xUnit/Moq, integration tests with Testcontainers) but lacks CI/CD integration. Without automated test execution and coverage reporting, regressions can slip into production and code quality cannot be objectively measured.

## What Changes

- Create GitHub Actions workflow for backend tests (`test-backend.yml`)
- Configure Coverlet for code coverage collection
- Set minimum coverage threshold (70%) to prevent quality degradation
- Generate HTML coverage reports as CI artifacts
- Add test and coverage status badges to backend README

## Impact

- Affected specs: `backend-testing` (new capability)
- Affected code:
  - `.github/workflows/test-backend.yml` - new CI workflow
  - `backend/src/JoiabagurPV.Tests/JoiabagurPV.Tests.csproj` - coverage settings (if needed)
  - `backend/README.md` - status badges
