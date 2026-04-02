## Why

Two data integrity bugs were discovered in production: the returns history modal displayed an invalid sale date ("01/01/1, 00:00") due to missing EF Core eager-loading, and dashboard metrics (monthly revenue, recent sales tables, trend/POS/payment charts) incorrectly included sales that had associated returns. These bugs distort financial reporting and erode operator trust in the dashboard.

## What Changes

- **Return history sale dates fixed**: EF Core `ThenInclude(rs => rs.Sale)` added to all three history-scope `ReturnRepository` query methods so `ReturnSaleDto.SaleDate` is populated from the real `Sale` entity instead of falling back to `DateTime.MinValue`.
- **`SaleDto` gains `HasReturn` field**: A new boolean field signals whether a sale has at least one associated return, propagated to the frontend via the existing `GET /api/sales` endpoint.
- **`SaleRepository` history queries include `ReturnSales`**: All three paginated history methods include `ReturnSales` so `MapToDto` can compute `HasReturn` correctly.
- **`MonthlyRevenue` is now net of returns**: `DashboardService.GetGlobalStatsAsync` subtracts `monthlyReturnsTotal` from gross sales revenue before exposing `MonthlyRevenue`.
- **Payment method distribution excludes returned sales**: The backend aggregation filters out sales that have any associated return.
- **Frontend dashboards filter returned sales**: Both `AdminDashboard` and `OperatorDashboard` exclude sales where `hasReturn === true` from recent-sales tables and all client-side chart aggregations (30-day trend, POS revenue, weekly operator trend).

## Capabilities

### New Capabilities

*(none)*

### Modified Capabilities

- `returns-management`: The history query for associated sales (`ReturnSale`) must now always eager-load the related `Sale` entity, regardless of whether the request is for a list or a single item.
- `sales-management`: `SaleDto` / `Sale` type gains a `hasReturn` boolean field. Sales history queries must include `ReturnSales` to populate it.
- `dashboard-analytics`: Monthly revenue is net of returns. Recent-sales tables and all frontend chart aggregations exclude sales with associated returns. Payment method distribution excludes returned sales server-side.

## Impact

- **Backend**: `ReturnRepository` (3 query methods), `SaleRepository` (3 query methods), `SalesService.MapToDto`, `SaleDto`, `DashboardService.GetGlobalStatsAsync`, `DashboardService.GetPaymentMethodDistributionAsync`.
- **Frontend**: `sales.types.ts` (`Sale` interface), `AdminDashboard.tsx`, `OperatorDashboard.tsx`.
- **No DB migration required**: all changes are query/DTO-level only.
- **No breaking API changes**: `hasReturn` is an additive field on `SaleDto`; existing consumers that don't read it are unaffected.
- **Cache note**: `PaymentDistributionCacheKey` is still 24h; after deploy, the first cache expiry will return the corrected (returns-excluded) distribution.
