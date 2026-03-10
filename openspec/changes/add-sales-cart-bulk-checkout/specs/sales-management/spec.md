## ADDED Requirements
### Requirement: Atomic Bulk Sales Registration
The system SHALL provide `POST /api/sales/bulk` to register multiple sale lines in a single atomic operation, where all lines succeed or none are persisted.

#### Scenario: Bulk sale checkout succeeds
- **WHEN** an authenticated user submits a bulk request with valid lines
- **AND** every line passes authorization, payment method, quantity, and stock validation
- **THEN** the system creates one Sale record per line
- **AND** creates corresponding inventory movements for all lines
- **AND** commits the transaction only after all operations succeed
- **AND** returns a successful bulk result with created sale identifiers

#### Scenario: Bulk sale checkout fails on one invalid line
- **WHEN** a bulk request contains at least one line that fails validation
- **THEN** the system aborts the bulk operation
- **AND** rolls back all pending Sale and InventoryMovement writes
- **AND** returns a single error response describing the failing condition

### Requirement: Bulk Checkout Invariants
The system SHALL enforce cross-line invariants for bulk sales: all lines MUST use the same point of sale and the same payment method.

#### Scenario: Reject mixed point-of-sale lines
- **WHEN** a bulk request includes lines with different `PointOfSaleId` values
- **THEN** the system returns `400 Bad Request`
- **AND** does not persist any sale line

#### Scenario: Reject mixed payment method lines
- **WHEN** a bulk request includes lines with different `PaymentMethodId` values
- **THEN** the system returns `400 Bad Request`
- **AND** does not persist any sale line

### Requirement: Global Bulk Confirmation Note
The system SHALL support one optional checkout note in bulk sales and propagate it consistently to each created sale record.

#### Scenario: Apply global note to all created sales
- **WHEN** a bulk checkout is confirmed with a global note
- **THEN** every sale created by that bulk operation stores the same note content
- **AND** note validation rules remain consistent with existing sale note constraints

### Requirement: Idempotent Bulk Submission
The system SHALL support idempotent retries for bulk sale checkout requests using an `Idempotency-Key` to prevent duplicate sale creation.

#### Scenario: Retry with same idempotency key
- **WHEN** a client sends a valid bulk request with an `Idempotency-Key`
- **AND** the same client retries with the same key and equivalent payload
- **THEN** the system returns the original successful result
- **AND** does not create additional duplicate sale records

#### Scenario: Reuse key with different payload
- **WHEN** a client reuses an existing `Idempotency-Key` with a different payload
- **THEN** the system rejects the request with a validation/conflict response
- **AND** preserves previously created records unchanged

### Requirement: Bulk Operation Traceability
The system SHALL assign a `BulkOperationId` to each successful bulk checkout and associate all resulting sales with that operation identifier.

#### Scenario: Group sales created by one bulk checkout
- **WHEN** a bulk checkout succeeds
- **THEN** all created sales include the same `BulkOperationId`
- **AND** the bulk response returns that identifier for auditing and support workflows
