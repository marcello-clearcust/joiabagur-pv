# point-of-sale-management Specification

## Purpose

Enable administrators to create, manage, and configure points of sale for multi-location retail operations. Points of sale represent physical business locations and serve as the foundation for inventory tracking, sales processing, and operator assignments. This capability provides role-based access control where administrators manage all locations while operators access only their assigned points of sale.

**Integration Points:**
- Integrates with `user-management` for operator assignments via UserPointOfSale
- Integrates with `payment-method-management` for payment method assignments
- Integrates with `access-control` for role-based permissions and point-of-sale-specific authorization
- Will integrate with future inventory, sales, and returns capabilities

**Deferred Documentation:** See `openspec/DEFERRED_TASKS.md` for system documentation tasks.
## Requirements
### Requirement: Point of Sale Creation

The system SHALL allow administrators to create new points of sale with complete information.

#### Scenario: Create point of sale successfully

- **WHEN** admin creates point of sale with valid data (name, code, address, phone, email)
- **THEN** system validates code is unique
- **AND** validates required fields are provided (name, code)
- **AND** creates point of sale with IsActive=true
- **AND** sets CreatedAt and UpdatedAt timestamps
- **AND** returns created point of sale data

#### Scenario: Create point of sale with duplicate code

- **WHEN** admin creates point of sale with existing code
- **THEN** system returns 409 Conflict
- **AND** provides message "El código de punto de venta ya está en uso"

#### Scenario: Create point of sale with missing required fields

- **WHEN** admin creates point of sale without name or code
- **THEN** system returns 400 Bad Request
- **AND** provides validation errors for each missing field

#### Scenario: Operator attempts to create point of sale

- **WHEN** operator attempts to create point of sale
- **THEN** system returns 403 Forbidden
- **AND** provides message "Acceso denegado"

### Requirement: Point of Sale Modification

The system SHALL allow administrators to modify existing point of sale information.

#### Scenario: Update point of sale information

- **WHEN** admin updates point of sale name, address, phone, or email
- **THEN** system validates changes are valid
- **AND** updates point of sale record
- **AND** sets UpdatedAt timestamp
- **AND** returns updated point of sale data

#### Scenario: Update point of sale code

- **WHEN** admin updates point of sale code
- **THEN** system validates new code is unique (excluding current point of sale)
- **AND** updates code if valid
- **AND** prevents update if code already exists

#### Scenario: Update non-existent point of sale

- **WHEN** admin attempts to update point of sale that doesn't exist
- **THEN** system returns 404 Not Found

### Requirement: Point of Sale Activation/Deactivation

The system SHALL allow administrators to control point of sale operational status.

#### Scenario: Activate point of sale

- **WHEN** admin activates inactive point of sale
- **THEN** system sets IsActive to true
- **AND** sets UpdatedAt timestamp
- **AND** allows assignments and operations on the point of sale

#### Scenario: Deactivate point of sale

- **WHEN** admin deactivates active point of sale
- **THEN** system validates no active assignments exist (optional business rule)
- **AND** sets IsActive to false
- **AND** sets UpdatedAt timestamp
- **AND** prevents new assignments and operations

#### Scenario: Deactivate point of sale with active assignments

- **WHEN** admin attempts to deactivate point of sale with active operator assignments
- **THEN** system returns 400 Bad Request
- **AND** provides message "No se puede desactivar punto de venta con operadores asignados activos"

### Requirement: Point of Sale Retrieval

The system SHALL provide appropriate point of sale information based on user roles.

#### Scenario: Administrator views all points of sale

- **WHEN** admin requests list of points of sale
- **THEN** system returns all points of sale (active and inactive)
- **AND** includes complete information (id, name, code, address, phone, email, isActive)
- **AND** results are paginated

#### Scenario: Administrator views single point of sale

- **WHEN** admin requests specific point of sale by ID
- **THEN** system returns complete point of sale information
- **AND** includes assigned operators and payment methods

#### Scenario: Operator views assigned points of sale

- **WHEN** operator requests points of sale
- **THEN** system returns only points of sale where operator has active assignment
- **AND** includes basic information (id, name, code, address)

#### Scenario: Operator views unassigned point of sale

- **WHEN** operator requests specific point of sale they are not assigned to
- **THEN** system returns 403 Forbidden
- **AND** provides message "No tiene acceso a este punto de venta"

### Requirement: Point of Sale Validation Rules

The system SHALL enforce validation rules for point of sale data.

#### Scenario: Code validation

- **WHEN** creating or updating point of sale
- **THEN** code must be at least 2 characters
- **AND** code must be alphanumeric with underscores and hyphens allowed
- **AND** code must be unique (case insensitive)

#### Scenario: Name validation

- **WHEN** creating or updating point of sale
- **THEN** name must be at least 2 characters
- **AND** name must not be empty

#### Scenario: Contact information validation

- **WHEN** email is provided
- **THEN** email must be valid format
- **WHEN** phone is provided
- **THEN** phone must be valid format (optional validation)

### Requirement: Point of Sale Assignment Management

The system SHALL manage assignments between points of sale and operators/payment methods.

#### Scenario: Assign operator to point of sale

- **WHEN** admin assigns operator to active point of sale
- **THEN** system creates UserPointOfSale record with IsActive=true
- **AND** sets AssignedAt timestamp
- **AND** operator gains access to point of sale operations

#### Scenario: Unassign operator from point of sale

- **WHEN** admin unassigns operator from point of sale
- **THEN** system sets UserPointOfSale.IsActive to false
- **AND** sets UnassignedAt timestamp
- **AND** preserves historical record
- **AND** operator loses access to point of sale

#### Scenario: Assign payment method to point of sale

- **WHEN** admin assigns active payment method to point of sale
- **THEN** system creates PointOfSalePaymentMethod record
- **AND** payment method becomes available for sales at that location

#### Scenario: Unassign payment method from point of sale

- **WHEN** admin unassigns payment method from point of sale
- **THEN** system marks PointOfSalePaymentMethod as inactive
- **AND** payment method is no longer available for new sales

### Requirement: Data Integrity and Constraints

The system SHALL maintain data integrity for point of sale operations.

#### Scenario: Prevent duplicate assignments

- **WHEN** admin attempts duplicate operator assignment
- **THEN** system returns 409 Conflict if active assignment exists
- **OR** reactivates existing assignment if inactive

#### Scenario: Cascade deactivation

- **WHEN** point of sale is deactivated
- **THEN** system may optionally deactivate related assignments
- **AND** prevents new assignments to deactivated point of sale

#### Scenario: Audit trail maintenance

- **WHEN** point of sale is modified
- **THEN** system maintains audit trail of changes
- **AND** preserves historical data for compliance

