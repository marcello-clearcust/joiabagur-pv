## MODIFIED Requirements

### Requirement: Return History and Queries

The system SHALL provide return history with filtering capabilities, applying role-based access control for visibility. All list responses MUST eager-load the associated `Sale` entity for each `ReturnSale` so that sale date and payment method are always available without requiring a separate detail request.

#### Scenario: View return history with filters

- **WHEN** authenticated user requests GET /api/returns
- **AND** applies optional filters (date range, product, point of sale, category)
- **THEN** system returns paginated list (max 50 per page)
- **AND** includes: returnId, returnDate, product (SKU, name), quantity, category, reason, user, pointOfSale, totalValue
- **AND** each entry's `associatedSales` array includes `saleDate` populated from the actual Sale entity (never a default/fallback date)
- **AND** applies role-based POS filtering

#### Scenario: Associated sale date is always present in list response

- **WHEN** authenticated user requests GET /api/returns (list endpoint)
- **THEN** every `associatedSales[].saleDate` is a valid ISO 8601 timestamp matching the original sale's `SaleDate`
- **AND** the date is NEVER `0001-01-01T00:00:00` (DateTime.MinValue) regardless of query scope

#### Scenario: Default date range

- **WHEN** user requests return history without date range
- **THEN** system defaults to last 30 days
- **AND** displays returns within that period

#### Scenario: View return details

- **WHEN** user requests GET /api/returns/{id}
- **THEN** system returns full return details
- **AND** includes associated sales with quantities and prices
- **AND** includes photo URL if exists
- **AND** includes inventory movement reference
- **AND** admin can view any return, operator can view only assigned POS returns

#### Scenario: Calculate total return value

- **WHEN** displaying return details or history
- **THEN** totalValue = SUM(ReturnSale.Quantity * ReturnSale.UnitPrice)
- **AND** value is displayed in Euro format (es-ES locale)
