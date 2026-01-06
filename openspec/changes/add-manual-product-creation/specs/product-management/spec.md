## ADDED Requirements

### Requirement: Manual Product Creation UI
The system SHALL provide a user-friendly form interface for administrators to create individual products with real-time validation and immediate feedback.

#### Scenario: Create product successfully via form
- **WHEN** an administrator navigates to the product creation page
- **AND** fills in SKU "JOY-001", Name "Gold Ring", Price 299.99, and optionally selects a collection
- **AND** submits the form
- **THEN** the product is created successfully
- **AND** a success confirmation is displayed with options to create another product or navigate to the product list

#### Scenario: SKU uniqueness validation in form
- **WHEN** an administrator enters an SKU that already exists
- **THEN** the form displays a validation error "El SKU ya está en uso"
- **AND** the submit button is disabled until a unique SKU is entered

#### Scenario: Required field validation
- **WHEN** an administrator attempts to submit the form without filling required fields (SKU, Name, Price)
- **THEN** the form displays validation messages for each missing field
- **AND** the form is not submitted

#### Scenario: Price validation
- **WHEN** an administrator enters a price <= 0
- **THEN** the form displays a validation error for the price field
- **AND** the submit button is disabled until a valid price is entered

#### Scenario: Collection dropdown population
- **WHEN** an administrator opens the product creation page
- **THEN** the collection dropdown is populated with available collections from the API
- **AND** selecting a collection is optional

#### Scenario: Form loading states
- **WHEN** the form is being submitted
- **THEN** a loading indicator is displayed
- **AND** form inputs are disabled to prevent duplicate submissions

#### Scenario: API error handling
- **WHEN** the product creation API returns an error
- **THEN** the error message is displayed to the user
- **AND** the form remains editable for correction

### Requirement: Product Creation API Testing
The system SHALL have comprehensive integration tests for the POST /api/products endpoint verifying all business rules and security requirements.

#### Scenario: Integration test coverage
- **WHEN** integration tests are executed for product creation
- **THEN** they verify successful creation, validation errors, SKU uniqueness, authentication, and authorization
- **AND** use Testcontainers for real database operations

