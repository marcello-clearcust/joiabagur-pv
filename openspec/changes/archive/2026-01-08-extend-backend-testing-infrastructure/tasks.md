# Tasks: Extend Backend Testing Infrastructure

## 1. Respawn Integration
- [x] 1.1 Add Respawn checkpoint initialization in `ApiWebApplicationFactory`
- [x] 1.2 Replace raw SQL TRUNCATE with `Respawner.ResetAsync()` in `ResetDatabaseAsync()`
- [x] 1.3 Add Respawn to `TestDatabaseFixture` if used for repository tests
- [x] 1.4 Verify all integration tests pass with Respawn-based cleanup

## 2. Collection Fixtures (Optional Optimization)
- [x] 2.1 Create `IntegrationTestCollection` to share `ApiWebApplicationFactory` across test classes
- [x] 2.2 Update integration test classes to use collection fixture
- [x] 2.3 Verify container reuse reduces test execution time

## 3. Documentation Updates (Optional)
- [x] 3.1 Update `Documentos/Testing/Backend/03-testcontainers.md` with Respawn integration examples
- [x] 3.2 Document collection fixture pattern for shared containers

