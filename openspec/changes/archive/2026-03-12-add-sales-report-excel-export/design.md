## Context

The system already has a sales history endpoint (`GET /api/sales`) with basic filters and pagination (max 50), used by operators and admins for transactional lookup. It also has an established reports section under `api/reports` with ClosedXML-based Excel export (see `ComponentReportsController`). The frontend Reports hub at `/reports` shows report cards (margin report, products without components).

Administrators need analytical capabilities: cross-POS totals, text search across SKU/name, amount range filtering, and bulk Excel export. These features should live in the Reports section, reusing existing patterns, without touching the transactional sales history.

## Goals / Non-Goals

**Goals:**
- Provide a paginated sales report API with extended filters and server-side global aggregates (count, quantity, amount) computed via a single SQL aggregation query.
- Generate an Excel export with a detail sheet and a per-POS summary sheet, capped at 10,000 rows.
- Reuse the existing ClosedXML export pattern from `ComponentReportsController` and the existing role-based POS filtering logic from `SalesService`.
- Deliver a frontend page under `/reports/sales` matching the existing Reports hub UX.

**Non-Goals:**
- Modify the existing sales history page or `GET /api/sales` endpoint.
- Add a "Reports" menu entry for operators.
- Support exports exceeding 10,000 rows (streaming, background jobs, etc.).
- Add scheduled/automated report generation.

## Decisions

### 1. Extend `ISalesService` rather than creating a separate report service

**Decision**: Add `GetSalesReportAsync` and `ExportSalesReportAsync` to `ISalesService` / `SalesService`.

**Rationale**: The report needs the same role-based POS filtering logic already in `SalesService.GetSalesHistoryAsync`. Duplicating this into a separate service would mean two places to maintain access-control logic. The report is conceptually an analytical view over the same sales data.

**Alternative considered**: Separate `ISalesReportService` — rejected because it would duplicate POS-filtering and auth logic with no real isolation benefit given the small team and monolithic architecture.

### 2. Controller under `api/reports/sales`

**Decision**: New `SalesReportController` at `api/reports/sales` with two GET endpoints (list + export).

**Rationale**: Keeps reports grouped under `api/reports/*` alongside `ComponentReportsController` (`api/reports/product-margins`). Avoids adding analytical endpoints to the transactional `SalesController`.

### 3. Single aggregate query for global totals

**Decision**: Use EF Core to generate a single SQL `SELECT COUNT(*), SUM(Quantity), SUM(Price * Quantity)` over the filtered `IQueryable`, separate from the paginated data query.

**Rationale**: Avoids loading all rows into memory. With ~500 products and modest sales volume, this remains performant. The frontend receives totals independent of the current page.

### 4. 409 Conflict for over-limit exports with real count

**Decision**: When export results exceed 10,000 rows, return HTTP 409 with `{ "message": "...", "totalCount": <real count> }` computed from a `COUNT(*)` query.

**Rationale**: The real count lets the frontend display a precise message ("Hay 25.000 ventas. Ajuste los filtros.") so administrators know how much to narrow down. Using 409 distinguishes this from validation errors (400) and server errors (500).

**Alternative considered**: Return 200 with a truncated file + warning header — rejected because silent data loss is worse than an explicit prompt to refine filters.

### 5. Excel structure: detail sheet + POS summary sheet

**Decision**: Sheet 1 "Ventas" has one row per sale (no totals row). Sheet 2 "Resumen por punto de venta" has one row per POS plus a "TOTAL GENERAL" footer row. Generated with ClosedXML following the existing `ExportMarginReport` pattern (bold headers, currency format, auto-fit columns).

**Rationale**: Separating detail from summary keeps the detail sheet clean for raw-data analysis while providing the aggregated view admins need. Matches the existing Excel export style.

### 6. Frontend page with TanStack Table and existing filter pattern

**Decision**: Build `SalesReportPage` using TanStack Table for the preview grid, Radix UI / Metronic components for filters, and Sonner toasts for the 409 warning.

**Rationale**: Consistent with existing report pages. Filters trigger a `getSalesReport` call; totals bar displays aggregate values from the response; export button calls `exportSalesReport` and handles 409 vs 200.

## Risks / Trade-offs

- **Aggregate query performance on large datasets** → At current scale (~500 products, few concurrent users), a COUNT/SUM query is negligible. If sales volume grows significantly, adding a database index on `(SaleDate, PointOfSaleId)` would help. No action needed now.
- **10,000-row limit may frustrate admins with broad filters** → The 409 response includes the real count and a clear message. Admins can narrow by date range or POS. This is an intentional trade-off to avoid memory/timeout issues on the free-tier infrastructure.
- **ClosedXML memory usage for large exports** → At 10,000 rows the memory footprint is manageable (~20-30 MB). No streaming needed at this scale.
- **Collection data may be null** → Products may not have an assigned collection. The report handles this gracefully with null/empty values in both the API response and Excel output.
