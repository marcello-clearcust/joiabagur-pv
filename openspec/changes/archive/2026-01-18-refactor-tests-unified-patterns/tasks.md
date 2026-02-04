# Tasks: Complete refactoring of existing tests

## 1. Create Missing Mother Objects
- [x] 1.1 Create ProductMother.cs with fluent builder methods
- [x] 1.2 Create UserMother.cs with fluent builder methods
- [x] 1.3 Create PointOfSaleMother.cs with fluent builder methods
- [x] 1.4 Create PaymentMethodMother.cs with fluent builder methods
- [x] 1.5 Create InventoryMother.cs with fluent builder methods
- [x] 1.6 Update TestDataMother.cs to register all new Mother Objects

## 2. Refactor Unit Tests for Bogus Compliance
- [x] 2.1 Refactor PaymentMethodServiceTests.cs to use TestDataGenerator
- [x] 2.2 Verify all unit tests use TestDataGenerator for entity creation

## 3. Refactor Integration Tests - Key Files
- [x] 3.1 AuthControllerTests.cs uses UserMother
- [x] 3.2 SalesControllerTests.cs uses Mother Objects
- [x] 3.3 InventoryIntegrationTests.cs uses Mother Objects
- [x] 3.4 ReturnsControllerTests.cs uses Mother Objects (exemplar)

## 4. Validation
- [x] 4.1 Unit tests pass (PaymentMethodServiceTests: 21/21)
- [x] 4.2 Build succeeds with no errors
- [x] 4.3 No manual TRUNCATE statements in codebase
- [x] 4.4 All Mother Objects use TestDataGenerator internally

## Summary
Key integration tests refactored to use Mother Objects pattern.
PaymentMethodServiceTests refactored to use TestDataGenerator.
All Mother Objects created and registered in TestDataMother.
