## Context

The application already has a Sales Report feature (backend + frontend) that serves as the primary pattern reference. The existing `SalesReportController`, `SalesReportService`, and `frontend/src/pages/reports/sales.tsx` provide a proven structure for paginated reports with Excel export and 409 limit handling.

`InventoryMovement` records already capture every stock change (Sale, Return, Adjustment, Import) with a signed `QuantityChange`. No aggregation endpoint exists today — this change introduces one, read-only, against existing data.

## Goals / Non-Goals

**Goals:**
- Provide administrators with an aggregated view of inventory movements per product within a date range.
- Support optional POS filtering to narrow the scope.
- Paginated API with server-side sorting on the numeric summary columns.
- Excel export of the full result set (up to 50,000 rows) with the same filters and sort order.
- Informative legend explaining what "Adiciones" and "Sustracciones" represent.

**Non-Goals:**
- Detailed movement-level listing (each individual movement row) — out of scope.
- Per-POS breakdown in rows (rows are per-product only; POS is a filter, not a grouping dimension).
- Global totals row in the table or Excel (optional future enhancement).
- Operator access — admin only.
- Real-time or scheduled report generation.

## Decisions

### 1. Aggregate in the database via LINQ GroupBy

**Decision**: Perform the grouping and summation in a single LINQ query that translates to SQL `GROUP BY`.

**Rationale**: The dataset size (~500 products, moderate movement volume) allows EF Core to handle the aggregation efficiently. A raw SQL or stored procedure is unnecessary at this scale and would deviate from the project's existing patterns.

**Alternative considered**: In-memory aggregation after fetching all movements. Rejected because it transfers unnecessary data and breaks down with volume growth.

### 2. Repository returns IQueryable of aggregated rows

**Decision**: The repository method returns `IQueryable<InventoryMovementSummaryRow>` so the service layer can apply sorting and paging without re-querying.

**Rationale**: Matches the existing pattern where repositories expose queryables and services compose on top. Keeps sorting/paging concerns in the application layer.

### 3. Separate controller at `api/reports/inventory-movements`

**Decision**: Create a new `InventoryMovementReportController` rather than extending `SalesReportController`.

**Rationale**: Each report has distinct filters, DTOs, and business rules. A dedicated controller keeps each report's API surface clean and independently maintainable.

### 4. Export limit of 50,000 rows with 409 response

**Decision**: Cap the Excel export at 50,000 rows. If the aggregated result exceeds this, return HTTP 409 with `totalCount` so the user knows to narrow filters.

**Rationale**: Mirrors the sales report pattern (which uses 10,000) but raises the limit since aggregated rows are smaller and fewer. 50k rows is within ClosedXML's comfortable memory range for the server's constraints.

### 5. Frontend follows the sales report page pattern

**Decision**: Build the page using the same component structure as `reports/sales.tsx` — filter panel, paginated DataGrid, export button with 409 handling.

**Rationale**: Consistency for the user and reduced development effort. The existing pattern handles loading states, pagination, date filter normalization, and blob download correctly.

### 6. Date filter is mandatory; POS filter is optional

**Decision**: `startDate` and `endDate` are required query parameters. `pointOfSaleId` is optional (defaults to all POS).

**Rationale**: Unbounded date queries could be expensive. Requiring a date range keeps queries performant and results meaningful. POS is optional because the most common use case is reviewing all locations at once.

## Risks / Trade-offs

- **[Risk] Large date ranges on high-volume data** → Mitigated by mandatory date filters and the 50k export cap. Pagination limits in-flight data for the API response.
- **[Risk] EF Core LINQ GroupBy translation edge cases** → The aggregation uses simple `SUM` with `CASE` (via conditional `Sum`), which EF Core/Npgsql handles well. Will verify the generated SQL during implementation.
- **[Trade-off] No per-POS grouping in output** → Keeps the initial scope simpler. Can be added later as a separate report or toggle without breaking the current API contract (add optional `groupByPos` parameter).
- **[Trade-off] Admin-only access** → Operators don't need this report per business requirements. If needed later, role-based filtering (similar to sales report) can be layered on.
