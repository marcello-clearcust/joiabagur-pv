# Change: Complete refactoring of existing tests

## Why
The `backend-testing` spec defines mandatory patterns for testing: Respawn for database cleanup, Bogus via `TestDataGenerator` for unit test data, Mother Objects for integration test setup, and a consistent test class structure. Several existing tests were written before these patterns were established and need to be updated for compliance and maintainability.

## What Changes

### 1. TestDataGenerator (Bogus) Compliance
- Refactor unit tests that use inline `new Entity { ... }` to use `TestDataGenerator.Create*()` methods instead
- **Affected**: `PaymentMethodServiceTests.cs` (uses `new PaymentMethod { ... }` in 20+ places)

### 2. Mother Object Pattern Implementation
- Create missing Mother Objects for all core domain entities
- Refactor integration tests to use Mother Objects for prerequisite data setup
- **New files**: `ProductMother`, `UserMother`, `PointOfSaleMother`, `PaymentMethodMother`, `InventoryMother`
- **Affected**: 11 integration test files

### 3. Respawn Compliance
- Already implemented ✅ - all integration tests use `ResetDatabaseAsync()` instead of manual TRUNCATEs
- Validation only required

### 4. Integration Test Structure Compliance
- Refactor `RepositoryTests.cs` to follow standard integration test structure:
  - Implement `IAsyncLifetime`
  - Use `[Collection(IntegrationTestCollection.Name)]`
  - Call `_factory.ResetDatabaseAsync()` in `InitializeAsync()`

### 5. Mocking Conventions Compliance
- Already implemented ✅ - unit tests properly mock interfaces using Moq
- Validation only required

## Impact
- **Affected specs**: `backend-testing` (compliance verification + new scenarios)
- **Affected code**:
  - `backend/src/JoiabagurPV.Tests/UnitTests/Application/PaymentMethodServiceTests.cs`
  - `backend/src/JoiabagurPV.Tests/IntegrationTests/*.cs` (12 files)
  - `backend/src/JoiabagurPV.Tests/TestHelpers/Mothers/*.cs` (5 new files)
  - `backend/src/JoiabagurPV.Tests/TestHelpers/TestDataMother.cs` (update)

## Detailed Scope Analysis

### Unit Tests Needing Refactoring (Bogus Compliance)

| File | Issue | Lines Affected |
|------|-------|----------------|
| `PaymentMethodServiceTests.cs` | Uses `new PaymentMethod { ... }` instead of `TestDataGenerator.CreatePaymentMethod()` | ~25 inline constructors |

### Integration Tests - Structure Compliance

| File | IAsyncLifetime | Collection | ResetDatabase | Mother Objects |
|------|----------------|------------|---------------|----------------|
| `RepositoryTests.cs` | ❌ Missing | ❌ Wrong collection | ❌ Not called | ❌ Not used |
| Other 11 files | ✅ | ✅ | ✅ | ❌ Not used |

### Integration Tests - Mother Object Adoption

| File | Required Mothers | Current Setup |
|------|------------------|---------------|
| `AuthControllerTests.cs` | UserMother | API calls |
| `AuthorizationTests.cs` | UserMother | API calls |
| `ImageRecognitionControllerTests.cs` | ProductMother, UserMother | API calls |
| `InventoryIntegrationTests.cs` | ProductMother, PointOfSaleMother, UserMother | API calls |
| `PaymentMethodsControllerTests.cs` | UserMother | API calls |
| `PointOfSalesControllerTests.cs` | UserMother | API calls |
| `ProductsControllerTests.cs` | UserMother | API calls |
| `RateLimitingTests.cs` | UserMother | API calls |
| `RepositoryTests.cs` | ProductMother (or other) | TestDataGenerator direct |
| `SalesControllerTests.cs` | ProductMother, PointOfSaleMother, PaymentMethodMother, UserMother | API calls |
| `UsersControllerTests.cs` | UserMother (admin setup) | API calls |

### Already Compliant (1 file)

| File | Status |
|------|--------|
| `ReturnsControllerTests.cs` | ✅ Uses Mother Objects, Respawn, IAsyncLifetime |

## Benefits

1. **Spec Compliance**: All tests follow the mandatory patterns defined in `backend-testing` spec
2. **Consistency**: Single source of truth for test data generation (`TestDataGenerator`)
3. **Maintainability**: Mother Objects centralize persistence logic for integration tests
4. **Speed**: Direct DB inserts via Mother Objects are faster than API calls for prerequisites
5. **Resilience**: Tests don't break when unrelated API validation changes
6. **Testability**: New developers can follow established patterns for new tests
