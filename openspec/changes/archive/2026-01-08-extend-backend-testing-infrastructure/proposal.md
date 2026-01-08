# Change: Extend Backend Testing Infrastructure

## Why

The backend testing infrastructure has Bogus and Testcontainers packages installed and partially configured, but:
1. **Respawn is unused** - Database cleanup uses raw SQL TRUNCATE instead of Respawn, which is error-prone and doesn't scale with schema changes
2. **Patterns are undocumented** - The `backend-testing` spec only covers CI/CD, not the actual test patterns developers should follow
3. **No shared test fixtures** - Each integration test class may start its own container, wasting resources

## What Changes

### Infrastructure Improvements
- Integrate Respawn for database cleanup in `ApiWebApplicationFactory.ResetDatabaseAsync()`
- Optimize container usage with xUnit collection fixtures to share containers across test classes

### Spec Expansion (backend-testing)
- Add requirement for test data generation patterns (Bogus-based builders)
- Add requirement for integration test patterns (Testcontainers + Respawn)
- Add requirement for mocking conventions (EF Core DbContext, services)

## Impact

- **Affected specs**: `backend-testing` (MODIFIED - expand with new requirements)
- **Affected code**:
  - `backend/src/JoiabagurPV.Tests/IntegrationTests/ApiWebApplicationFactory.cs` - Respawn integration
  - `backend/src/JoiabagurPV.Tests/IntegrationTests/TestDatabaseFixture.cs` - Respawn integration
  - `Documentos/Testing/Backend/` - Documentation updates (optional)

