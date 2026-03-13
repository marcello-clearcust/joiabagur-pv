## Why

The business needs visibility into how inventory moves across products and points of sale. Today there is no way to see an aggregated view of stock additions (returns, adjustments, imports) vs subtractions (sales, negative adjustments) per product over a time period. This report enables the administrator to quickly identify net inventory changes, detect anomalies, and make restocking decisions — complementing the existing sales reports with an inventory-focused perspective.

## What Changes

- Add a new **"Inventory Movement Summary"** report accessible from the Reports hub (admin only).
- New backend aggregation endpoint that groups `InventoryMovement` records by product, summing additions (positive `QuantityChange`) and subtractions (negative `QuantityChange`) within a date range, with optional POS filter.
- New paginated API endpoint (`GET api/reports/inventory-movements`) with server-side sorting by Additions, Subtractions, or Difference.
- New Excel export endpoint (`GET api/reports/inventory-movements/export`) returning the full result set (up to 50,000 rows; 409 if exceeded).
- New frontend report page with mandatory date filters, optional POS filter, sortable paginated table, help legend, and Excel export button.
- New route and report card entry in the Reports hub.

## Capabilities

### New Capabilities
- `inventory-movement-report`: Aggregated inventory movement summary report by product — backend aggregation, paginated API, Excel export, and frontend report page with filters, sorting, and help legend.

### Modified Capabilities
_(none — this is a standalone report that reads existing `InventoryMovement` data without modifying any existing capability requirements)_

## Impact

- **Backend (Domain)**: New method signature on `IInventoryMovementRepository`.
- **Backend (Infrastructure)**: New LINQ aggregation query in `InventoryMovementRepository`.
- **Backend (Application)**: New DTOs, service interface and implementation (`IInventoryMovementReportService`), DI registration.
- **Backend (API)**: New `InventoryMovementReportController` with two endpoints (report + export), admin-only authorization.
- **Frontend (types)**: New TypeScript types for filters, response, and row data.
- **Frontend (services)**: New API service for report and export calls.
- **Frontend (pages)**: New report page component with filters, table, legend, and export.
- **Frontend (routing)**: New route registration and Reports hub card.
- **Dependencies**: Uses existing ClosedXML (backend) for Excel generation — no new dependencies.
