## ADDED Requirements

### Requirement: Dashboard Aggregation Service

The backend SHALL provide a DashboardService that computes pre-aggregated KPIs by querying Sales, Returns, and Inventory data, with optional POS scoping and IMemoryCache for expensive aggregations.

#### Scenario: Compute global KPIs for administrator
- **WHEN** DashboardService is called without posId
- **THEN** service queries Sales table for today's count and total across all POS
- **AND** queries Sales for current month's total revenue
- **AND** queries Sales for same month of previous year's revenue (nullable if no data)
- **AND** queries Returns for current month's count and total amount
- **AND** returns all KPIs in a single response DTO

#### Scenario: Compute POS-scoped KPIs for operator
- **WHEN** DashboardService is called with a posId
- **THEN** service queries Sales table for today's count and total filtered by posId
- **AND** queries Sales for current natural week (Mon-Sun) revenue filtered by posId
- **AND** queries Returns for today's count filtered by posId
- **AND** does not compute donut chart aggregations

#### Scenario: Cache donut chart aggregations
- **WHEN** DashboardService computes payment method distribution or return category distribution
- **THEN** service checks IMemoryCache for cached result
- **AND** if cache hit, returns cached data without querying database
- **AND** if cache miss, queries database, stores result in IMemoryCache with AbsoluteExpirationRelativeToNow = 24 hours, and returns fresh data

#### Scenario: Validate POS access for operator
- **WHEN** DashboardService is called with posId by an operator
- **THEN** service validates the operator is assigned to the POS via UserPointOfSale
- **AND** returns 403 Forbidden if not assigned

### Requirement: Dashboard API Controller

The backend SHALL expose a DashboardController with a GET /api/dashboard/stats endpoint that delegates to DashboardService, enforcing authentication and role-based response shaping.

#### Scenario: GET /api/dashboard/stats returns 200 for authenticated users
- **WHEN** authenticated user calls GET /api/dashboard/stats
- **THEN** controller delegates to DashboardService
- **AND** returns 200 OK with DashboardStatsDto

#### Scenario: Reject unauthenticated requests
- **WHEN** unauthenticated user calls GET /api/dashboard/stats
- **THEN** controller returns 401 Unauthorized

#### Scenario: Admin receives full response including distributions
- **WHEN** authenticated administrator calls GET /api/dashboard/stats
- **THEN** response includes paymentMethodDistribution and returnCategoryDistribution arrays

#### Scenario: Operator receives scoped response without distributions
- **WHEN** authenticated operator calls GET /api/dashboard/stats?posId={id}
- **THEN** response includes POS-scoped KPIs
- **AND** paymentMethodDistribution and returnCategoryDistribution are null
