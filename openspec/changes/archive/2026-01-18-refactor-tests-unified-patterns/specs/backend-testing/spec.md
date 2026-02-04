## MODIFIED Requirements

### Requirement: Test Data Generation with Bogus

The system SHALL provide Bogus-based test data generators in `TestDataGenerator` that create realistic fake entities. This class is the single source of truth for entity generation logic, used by both unit tests (directly) and integration tests (via Mother Objects). All unit tests MUST use `TestDataGenerator` methods instead of inline entity construction.

#### Scenario: Entity Faker Generation
- **WHEN** a unit test needs a fake entity (User, Product, Inventory, etc.)
- **THEN** a corresponding Faker method is available in `TestDataGenerator`
- **AND** the generated entity has realistic values (e.g., `f.Commerce.ProductName()`)
- **AND** key properties can be customized via optional parameters
- **AND** the entity is returned in-memory (not persisted)

#### Scenario: Related Entity Generation
- **WHEN** a test needs entities with relationships (e.g., Inventory with Product and PointOfSale)
- **THEN** the generator allows specifying foreign key values via parameters
- **AND** navigation properties can be populated for in-memory tests

#### Scenario: TestDataGenerator as Single Source
- **WHEN** adding a new entity type to the system
- **THEN** a new Faker and Create method MUST be added to `TestDataGenerator`
- **AND** the corresponding Mother Object MUST use `TestDataGenerator.Create*()` internally
- **AND** default values MUST NOT be duplicated between the two

#### Scenario: Prohibited Inline Entity Construction in Unit Tests
- **WHEN** writing unit tests
- **THEN** developers MUST NOT use `new Entity { Property = value }` for domain entities
- **AND** developers MUST use `TestDataGenerator.Create*()` methods instead
- **AND** code reviews SHALL reject inline entity construction patterns
- **AND** specific property overrides MAY be applied after `TestDataGenerator` call

### Requirement: Mother Object Pattern for Integration Tests

The system SHALL provide Mother Object classes that create and persist test entities with realistic defaults, simplifying integration test setup. Mother Objects MUST use Bogus (via TestDataGenerator) internally for default values to ensure consistency with unit test data. All core domain entities MUST have corresponding Mother Objects.

#### Scenario: Using TestDataMother Factory
- **WHEN** an integration test needs to create test data
- **THEN** `TestDataMother` is instantiated with `IServiceProvider`
- **AND** fluent builder methods are used to configure entity properties
- **AND** `CreateAsync()` persists the entity to the database
- **AND** the created entity is returned for use in assertions

#### Scenario: Creating Related Entities
- **WHEN** a test needs entities with foreign key relationships
- **THEN** parent entities are created first using their Mother Objects
- **AND** child entities reference parent IDs via fluent `With*()` methods
- **AND** all entities are persisted in the correct order

#### Scenario: Mother Object Default Values via Bogus
- **WHEN** a Mother Object creates an entity without explicit configuration
- **THEN** the Mother Object calls `TestDataGenerator.Create*()` to get a Bogus-generated entity
- **AND** the entity has realistic, randomized default values
- **AND** unique constraints are satisfied via Bogus random generation
- **AND** the entity passes domain validation

#### Scenario: Unified Test Data Architecture
- **WHEN** implementing test data generation
- **THEN** `TestDataGenerator` (Bogus Fakers) is the single source of entity generation logic
- **AND** Mother Objects wrap `TestDataGenerator` for database persistence
- **AND** unit tests use `TestDataGenerator` directly with mocked repositories
- **AND** integration tests use Mother Objects via `TestDataMother`
- **AND** no duplication of default value logic exists between the two patterns

#### Scenario: Mother Object Location and Naming
- **WHEN** creating a new Mother Object
- **THEN** the class is placed in `TestHelpers/Mothers/` directory
- **AND** the class is named `{Entity}Mother` (e.g., `ProductMother`, `SaleMother`)
- **AND** the class is registered in `TestDataMother` factory

#### Scenario: Required Mother Objects for Core Entities
- **WHEN** the test suite is fully configured
- **THEN** `UserMother` MUST exist for creating test users
- **AND** `ProductMother` MUST exist for creating test products
- **AND** `PointOfSaleMother` MUST exist for creating test points of sale
- **AND** `PaymentMethodMother` MUST exist for creating test payment methods
- **AND** `InventoryMother` MUST exist for creating test inventory records
- **AND** `SaleMother` MUST exist for creating test sales
- **AND** `ReturnMother` MUST exist for creating test returns
- **AND** all Mother Objects are accessible via `TestDataMother` factory

#### Scenario: Integration Test Prerequisite Setup
- **WHEN** an integration test requires prerequisite entities (not the system under test)
- **THEN** Mother Objects SHALL be used instead of API calls
- **AND** the test SHALL call the API only for the functionality being tested
- **AND** this reduces test brittleness when unrelated validations change

### Requirement: Integration Test Structure

The system SHALL follow a consistent structure for integration test classes.

#### Scenario: Test Class Setup
- **WHEN** creating a new integration test class
- **THEN** the class implements `IAsyncLifetime`
- **AND** the class is decorated with `[Collection(IntegrationTestCollection.Name)]`
- **AND** `InitializeAsync()` calls `_factory.ResetDatabaseAsync()` first
- **AND** test data is created using Mother Objects after reset

#### Scenario: Authenticated Client Creation
- **WHEN** a test needs an authenticated HTTP client
- **THEN** `CreateAuthenticatedClientAsync()` helper method is used
- **AND** the method posts to `/api/auth/login` and extracts cookies
- **AND** cookies are added to the client's request headers

#### Scenario: Unified Test Collection
- **WHEN** integration tests share database infrastructure
- **THEN** all tests MUST use `[Collection(IntegrationTestCollection.Name)]`
- **AND** separate collection fixtures (e.g., `RepositoryTestCollection`) MUST NOT be created
- **AND** test isolation is achieved via `ResetDatabaseAsync()` not separate containers
