## Why

Administrators need a dedicated sales report with advanced filtering and Excel export to analyze sales performance across points of sale. The existing sales history (`GET /api/sales`) is designed for transactional lookup by operators, not for analytical reporting — it lacks aggregate totals, text search, amount-range filters, and export capabilities. This change adds a report-oriented view under the existing Reports section with global totals (unaffected by pagination) and a 10,000-row Excel export with a per-POS summary sheet.

## What Changes

- New `GET /api/reports/sales` endpoint returning paginated sales data with extended filters (text search, amount range, has-photo, price-was-overridden) and global aggregates (totalSalesCount, totalQuantity, totalAmount) computed server-side.
- New `GET /api/reports/sales/export` endpoint generating an Excel file (ClosedXML) with a "Ventas" detail sheet and a "Resumen por punto de venta" summary sheet including a total-general row. Returns 409 with the real total count when results exceed 10,000 rows.
- New frontend page at `/reports/sales` with filters, global totals bar, paginated preview table, and Excel download button (with warning toast on 409).
- New card on the existing Reports hub linking to the sales report.
- No changes to the existing sales history page or `GET /api/sales` endpoint.

## Capabilities

### New Capabilities
- `sales-reports`: Advanced sales report with extended filters, server-side global aggregates, paginated preview, and Excel export with per-POS summary and 10,000-row limit.

### Modified Capabilities

## Impact

- **Backend**: New DTOs (`SalesReportFilterRequest`, `SalesReportItemDto`), repository method with aggregate query, service method, and `SalesReportController` under `api/reports/sales`. Requires `Product.Collection` include in queries.
- **Frontend**: New route `/reports/sales`, new page component, types, and API service. Reports hub updated with a new card. Operator menu unchanged.
- **Dependencies**: ClosedXML (already in use for component reports).
- **Existing code**: `SalesController`, `SaleDto`, and sales history page are not modified.
