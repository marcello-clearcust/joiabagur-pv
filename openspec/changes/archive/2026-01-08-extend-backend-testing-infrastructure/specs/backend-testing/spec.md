## ADDED Requirements

### Requirement: Test Data Generation

The system SHALL provide Bogus-based test data generators that create realistic fake entities for unit and integration tests.

#### Scenario: Entity Faker Generation
- **WHEN** a unit test needs a fake entity (User, Product, Inventory, etc.)
- **THEN** a corresponding Faker method is available in `TestDataGenerator`
- **AND** the generated entity has realistic values
- **AND** key properties can be customized via optional parameters

#### Scenario: Related Entity Generation
- **WHEN** a test needs entities with relationships (e.g., Inventory with Product and PointOfSale)
- **THEN** the generator allows specifying foreign key values
- **AND** navigation properties can be populated for in-memory tests

### Requirement: Database Cleanup with Respawn

The system SHALL use Respawn for database cleanup between integration tests to ensure test isolation.

#### Scenario: Database Reset Before Test
- **WHEN** an integration test requires a clean database state
- **THEN** `ResetDatabaseAsync()` is called
- **AND** Respawn removes all data from tables
- **AND** seed data is re-applied if needed
- **AND** the operation completes in under 1 second

#### Scenario: Schema Change Resilience
- **WHEN** a new table is added to the database schema
- **THEN** Respawn automatically includes the new table in cleanup
- **AND** no manual SQL TRUNCATE statements need updating

### Requirement: Integration Test Container Sharing

The system SHALL share PostgreSQL Testcontainers across integration test classes to optimize execution time.

#### Scenario: Collection Fixture Usage
- **WHEN** multiple integration test classes run
- **THEN** they share a single PostgreSQL container via xUnit collection fixture
- **AND** each test class resets the database before its tests
- **AND** tests remain isolated despite sharing the container

#### Scenario: Container Lifecycle
- **WHEN** the first test in the collection runs
- **THEN** the PostgreSQL container is started once
- **AND** the container is stopped after all tests in the collection complete

### Requirement: Mocking Conventions

The system SHALL follow consistent mocking conventions for unit tests using Moq.

#### Scenario: Repository Mocking
- **WHEN** a service test needs to mock repository behavior
- **THEN** `Mock<IRepository<T>>` is used
- **AND** setup methods define return values for expected calls
- **AND** verify methods confirm expected interactions

#### Scenario: Service Mocking
- **WHEN** a controller or higher-level test needs to mock a service
- **THEN** the service interface is mocked (not the concrete implementation)
- **AND** mocks are configured in Arrange section
- **AND** behavior verification happens in Assert section

