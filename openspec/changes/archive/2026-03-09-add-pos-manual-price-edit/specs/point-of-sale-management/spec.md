## MODIFIED Requirements
### Requirement: Point of Sale Retrieval

The system SHALL provide appropriate point of sale information based on user roles, including manual sale price edit policy metadata required by sales forms.

#### Scenario: Administrator views all points of sale

- **WHEN** admin requests list of points of sale
- **THEN** system returns all points of sale (active and inactive)
- **AND** includes complete information (id, name, code, address, phone, email, isActive, allowManualPriceEdit)
- **AND** results are paginated

#### Scenario: Administrator views single point of sale

- **WHEN** admin requests specific point of sale by ID
- **THEN** system returns complete point of sale information
- **AND** includes assigned operators and payment methods
- **AND** includes allowManualPriceEdit

#### Scenario: Operator views assigned points of sale

- **WHEN** operator requests points of sale
- **THEN** system returns only points of sale where operator has active assignment
- **AND** includes basic information (id, name, code, address, allowManualPriceEdit)

#### Scenario: Operator views unassigned point of sale

- **WHEN** operator requests specific point of sale they are not assigned to
- **THEN** system returns 403 Forbidden
- **AND** provides message "No tiene acceso a este punto de venta"

## ADDED Requirements
### Requirement: Manual Sale Price Edit Configuration

The system SHALL allow administrators to configure whether operators can manually edit sale prices per point of sale using the `AllowManualPriceEdit` boolean setting.

#### Scenario: Administrator enables manual sale price editing

- **WHEN** admin creates or updates a point of sale with AllowManualPriceEdit = true
- **THEN** system persists the configuration for that point of sale
- **AND** sales forms can use this flag to enable editable price input

#### Scenario: Administrator disables manual sale price editing

- **WHEN** admin creates or updates a point of sale with AllowManualPriceEdit = false
- **THEN** system persists the configuration for that point of sale
- **AND** sales forms use official product price only

#### Scenario: Operator attempts to modify manual price edit configuration

- **WHEN** operator attempts to create or update AllowManualPriceEdit for any point of sale
- **THEN** system returns 403 Forbidden
- **AND** preserves existing configuration value
