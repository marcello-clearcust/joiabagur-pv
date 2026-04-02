## MODIFIED Requirements

### Requirement: Sales History and Queries

The system SHALL provide sales history with filtering capabilities, applying role-based access control (administrators see all sales, operators see only sales from assigned points of sale). Each sale in the response MUST include a `hasReturn` boolean indicating whether the sale has at least one associated return record.

#### Scenario: Administrator views full sales history

- **WHEN** authenticated administrator requests GET /api/sales
- **AND** applies optional filters (date range, product, POS, user, payment method)
- **THEN** system returns paginated sales (max 50 per page)
- **AND** includes sale details (date, product, quantity, price, total, payment method, operator, photo indicator)
- **AND** includes `hasReturn: true` for sales that have at least one associated ReturnSale record
- **AND** includes `hasReturn: false` for sales with no associated returns
- **AND** includes pagination metadata (totalCount, totalPages, currentPage)
- **AND** sales from all points of sale are visible

#### Scenario: Operator views sales history for assigned POS

- **WHEN** authenticated operator requests GET /api/sales
- **AND** applies optional filters
- **THEN** system returns sales ONLY from points of sale assigned to operator
- **AND** each sale includes `hasReturn` boolean
- **AND** filters by UserPointOfSale assignments (via access-control integration)
- **AND** applies same pagination and filtering as admin
- **AND** sales from unassigned POS are invisible

#### Scenario: Filter sales by date range

- **WHEN** user requests sales history with date range filter (startDate, endDate)
- **THEN** system returns sales where SaleDate >= startDate AND SaleDate <= endDate
- **AND** defaults to last 30 days if no date range specified

#### Scenario: Filter sales by product

- **WHEN** user requests sales history with product filter (productId or SKU)
- **THEN** system returns sales matching specified product
- **AND** includes product name and SKU in response

#### Scenario: Filter sales by payment method

- **WHEN** user requests sales history with payment method filter
- **THEN** system returns sales matching specified payment method
- **AND** includes payment method name in response

#### Scenario: View sale details with photo

- **WHEN** user requests GET /api/sales/{id}
- **THEN** system returns full sale details
- **AND** includes SalePhoto with pre-signed URL if photo exists
- **AND** includes product details, payment method, operator name, inventory movement reference
- **AND** admin can view any sale, operator can view only sales from assigned POS

#### Scenario: Require authentication for sales access

- **WHEN** unauthenticated user requests sales endpoints
- **THEN** system returns 401 Unauthorized
