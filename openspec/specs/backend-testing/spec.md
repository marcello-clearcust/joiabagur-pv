# backend-testing Specification

## Purpose
TBD - created by archiving change setup-backend-testing. Update Purpose after archive.
## Requirements
### Requirement: Backend Test CI Workflow

The system SHALL provide a GitHub Actions workflow that automatically executes all backend tests (unit and integration) on code changes to main/develop branches and pull requests.

#### Scenario: Test Execution on Pull Request
- **WHEN** a pull request is opened or updated targeting main or develop
- **THEN** the backend test workflow is triggered automatically
- **AND** all unit tests are executed
- **AND** all integration tests are executed with PostgreSQL container
- **AND** test results are reported in the pull request

#### Scenario: Test Execution on Push
- **WHEN** code is pushed to main or develop branch
- **THEN** the backend test workflow is triggered automatically
- **AND** all tests are executed
- **AND** workflow status is visible in repository

### Requirement: Backend Code Coverage Reporting

The system SHALL collect and report code coverage metrics during CI test execution, enforcing a minimum coverage threshold.

#### Scenario: Coverage Collection
- **WHEN** backend tests are executed in CI
- **THEN** code coverage is collected using Coverlet
- **AND** coverage data is generated in Cobertura format

#### Scenario: Coverage Threshold Enforcement
- **WHEN** code coverage falls below 70%
- **THEN** the CI workflow fails
- **AND** coverage percentage is reported in workflow logs

#### Scenario: Coverage Report Artifact
- **WHEN** backend tests complete in CI
- **THEN** HTML coverage report is generated
- **AND** report is uploaded as workflow artifact
- **AND** report is accessible for 30 days

### Requirement: Backend Test Status Visibility

The system SHALL provide visible indicators of test and coverage status in the repository documentation.

#### Scenario: Status Badges Display
- **WHEN** viewing backend README.md
- **THEN** test workflow status badge is displayed
- **AND** current test status (passing/failing) is visible

### Requirement: Integration Test Database

The system SHALL provide isolated PostgreSQL database instances for integration tests in CI environment.

#### Scenario: PostgreSQL Service Container
- **WHEN** integration tests run in GitHub Actions
- **THEN** PostgreSQL 15 container is started
- **AND** tests connect to the container database
- **AND** container is cleaned up after tests complete

#### Scenario: Test Isolation
- **WHEN** multiple test runs execute
- **THEN** each run uses an isolated database instance
- **AND** no data persists between workflow runs

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

