## Context

The system has two related data-integrity gaps that share the same root cause: **missing EF Core eager-loading of navigation properties in list/history queries**. When a history query does not load a child collection, EF Core leaves those navigation properties as empty collections or null references. Code that relies on those properties silently falls back to default values (`DateTime.MinValue`, empty count), producing incorrect UI output.

Additionally, the dashboard treats gross sales revenue as net revenue, never subtracting returns. The frontend aggregation for charts and recent-sales tables draws from the raw sales list without filtering out returned sales, meaning a single return does not reduce any displayed metric.

Current state before this fix:
- `ReturnRepository` history methods: include `ReturnSales` but not `.ThenInclude(rs => rs.Sale)` → `rs.Sale` is null in list responses → `SaleDate` falls back to `DateTime.MinValue` → UI renders "01/01/1, 00:00".
- `SaleDto` has no `HasReturn` field → frontend cannot distinguish returned from active sales.
- `SaleRepository` history methods: do not include `ReturnSales` → `sale.ReturnSales.Count` is always 0 → `HasReturn` would always be false.
- `DashboardService.GetGlobalStatsAsync`: `MonthlyRevenue = SUM(sales)` ignoring returns.
- `DashboardService.GetPaymentMethodDistributionAsync`: groups all month's sales regardless of return status.
- `AdminDashboard` / `OperatorDashboard`: iterate `salesHistory.sales` without filtering `hasReturn`.

## Goals / Non-Goals

**Goals:**
- Return history always shows correct associated sale dates.
- `GET /api/sales` response includes `hasReturn` boolean per sale.
- Dashboard `MonthlyRevenue` is net of returns for the same month.
- Payment method donut excludes returned sales from aggregation.
- Recent-sales tables and client-side chart aggregations skip returned sales.
- No DB migration (query/DTO changes only).
- No breaking API change (`hasReturn` is purely additive).

**Non-Goals:**
- Operator dashboard KPIs (`salesTodayTotal`, `weeklyRevenue`) are NOT adjusted for returns; they represent gross activity, which is intentional.
- Return history `GET /api/returns/{id}` single-item endpoint already eager-loads `Sale` correctly — it is not touched.
- Sales report (`GET /api/sales/report`) is not modified; report consumers already have dedicated return columns.
- No UI changes beyond the two dashboard components.

## Decisions

### Decision 1 — Eager-load `Sale` in all `ReturnRepository` history methods

**Chosen approach:** Add `.ThenInclude(rs => rs.Sale)` to `GetByPointOfSaleAsync`, `GetAllReturnsAsync`, and `GetByPointOfSalesAsync` in `ReturnRepository`.

**Alternative considered:** Lazy-loading via EF Core proxies.
**Rejected because:** Lazy-loading introduces N+1 query risk and requires enabling proxy packages. Explicit `Include` chains are already used throughout the codebase (consistent with project conventions) and are safer for paginated list endpoints.

**Alternative considered:** Load `Sale` only when rendering detail modal (frontend re-fetch on open).
**Rejected because:** The API already returns `associatedSales` in list responses; changing that contract would require a frontend redesign and adds an extra network round-trip for each modal open.

### Decision 2 — Add `HasReturn` to `SaleDto` (additive field)

**Chosen approach:** Add `bool HasReturn` to `SaleDto`, populate in `SalesService.MapToDto` from `sale.ReturnSales.Count > 0`, and include `ReturnSales` in the three `SaleRepository` history methods.

**Alternative considered:** Add a separate `GET /api/sales/{id}/has-return` endpoint.
**Rejected because:** Adding a per-sale field to the existing DTO is zero-cost for callers that ignore it, avoids extra requests, and requires only one query per page of results rather than N additional queries.

**Alternative considered:** Filter at the query level (return only sales without returns).
**Rejected because:** Callers (sales history page, reports) still need to see returned sales; they just need to know about the status. Hiding them would be a separate, unrelated decision.

### Decision 3 — Net revenue = gross sales − monthly returns total (backend)

**Chosen approach:** Compute `monthlyReturnsTotal` (already fetched for `MonthlyReturnsTotal` field), then `monthlyRevenue = monthlySalesRevenue − monthlyReturnsTotal`.

**Alternative considered:** Join sales and returns tables in a single SQL query.
**Rejected because:** The existing code already runs two separate async queries; reusing the already-fetched `monthlyReturns.Total` is simpler, avoids a complex join, and does not change query count.

**Trade-off:** The subtraction uses the return date window (same month), not the sale date window. A return registered in April for a March sale reduces April revenue, not March. This matches the user's stated expectation ("devoluciones del mes corriente") and is consistent with how `MonthlyReturnsTotal` is already reported.

### Decision 4 — Exclude returned sales from payment distribution server-side

**Chosen approach:** Add `.Where(s => !s.ReturnSales.Any())` to `GetPaymentMethodDistributionAsync` before the `GroupBy`.

**Alternative considered:** Filter client-side from the `paymentMethodDistribution` array.
**Rejected because:** The distribution is already aggregated on the server (amounts, not individual sales); the client receives totals, not raw rows. Filtering must happen before the `GroupBy`.

**Note:** The 24-hour `IMemoryCache` for this distribution means the fix only takes effect after the next cache expiry. This is acceptable given the low-traffic context (2–3 concurrent users).

### Decision 5 — Filter returned sales client-side in dashboards

**Chosen approach:** Both `AdminDashboard` and `OperatorDashboard` derive a `salesWithoutReturns` array (`sales.filter(s => !s.hasReturn)`) immediately after receiving the API response and use it for all downstream aggregations.

**Alternative considered:** Filter at the `GET /api/sales` query level (add `excludeReturned` query param).
**Rejected because:** The sales history page itself should still show returned sales (they are valid transactions). Filtering at the API level for dashboard use would require a new parameter and complicates the general-purpose endpoint. The current sales-history fetch for the dashboard already retrieves ≤1000 records; client-side filtering is negligible.

## Risks / Trade-offs

- **Cache staleness for payment distribution**: After deploy, the corrected payment distribution only appears after the 24-hour cache expires (or server restart). For a low-traffic system this is acceptable, but operators may see the old data for up to 24 hours.  
  → *Mitigation*: If immediate correction is needed, restart the API process to clear `IMemoryCache`.

- **`ReturnSales` include on sales history adds join cost**: Every paginated sales-history query now joins `ReturnSales`. With max 50 rows per page and few returns per sale, this is negligible. At larger scale (thousands of sales with many returns) it should be revisited.  
  → *Mitigation*: Monitor query time; if needed, replace with a `COUNT(*)` subquery or a computed column.

- **Operator weekly trend uses gross sales for week chart, net only for recent-sales table**: `salesTodayTotal` and `weeklyRevenue` KPI cards (from `GET /api/dashboard/stats`) are not adjusted. Only the client-aggregated chart and table are filtered. This slight inconsistency was accepted as intentional: the KPI cards represent transaction volume, not net revenue.

## Migration Plan

1. Deploy updated backend (no DB migration required).
2. Deploy updated frontend.
3. If payment distribution correction is urgent, restart API to clear `IMemoryCache`.
4. No rollback complexity: reverting either side independently is safe — `hasReturn` is ignored by older frontend, and older frontend doesn't filter even if backend provides the field.

## Open Questions

*(none — all decisions made during implementation)*
