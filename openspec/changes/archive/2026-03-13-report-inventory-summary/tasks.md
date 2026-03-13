## 1. Backend DTOs

- [x] 1.1 Create `InventoryMovementSummaryRow` DTO in `backend/src/JoiabagurPV.Application/DTOs/Inventory/` with fields: `ProductId`, `ProductName`, `ProductSku`, `Additions` (int), `Subtractions` (int), `Difference` (int)
- [x] 1.2 Create `InventoryMovementReportFilterRequest` DTO in `backend/src/JoiabagurPV.Application/DTOs/Inventory/` with fields: `StartDate` (DateTime, required), `EndDate` (DateTime, required), `PointOfSaleId` (Guid?, optional), `Page` (int, default 1), `PageSize` (int, default 20), `SortBy` (string?, optional), `SortDirection` (string?, optional)
- [x] 1.3 Create `InventoryMovementReportResponse` DTO in `backend/src/JoiabagurPV.Application/DTOs/Inventory/` with fields: `Items` (List of `InventoryMovementSummaryRow`), `TotalCount`, `Page`, `PageSize`, `TotalPages`

## 2. Backend Repository

- [x] 2.1 Add `GetMovementSummaryByProductAsync` method signature to `IInventoryMovementRepository` (`backend/src/JoiabagurPV.Domain/Interfaces/Repositories/IInventoryMovementRepository.cs`) that accepts `startDate`, `endDate`, `pointOfSaleId?` and returns `IQueryable<InventoryMovementSummaryRow>`
- [x] 2.2 Implement `GetMovementSummaryByProductAsync` in `InventoryMovementRepository` (`backend/src/JoiabagurPV.Infrastructure/Data/Repositories/InventoryMovementRepository.cs`) using LINQ GroupBy on `Inventory.ProductId`, filtering by `MovementDate` range and optionally by `Inventory.PointOfSaleId`, computing `Additions = SUM(QuantityChange WHERE > 0)`, `Subtractions = SUM(ABS(QuantityChange) WHERE < 0)`, `Difference = Additions - Subtractions`

## 3. Backend Service

- [x] 3.1 Create `IInventoryMovementReportService` interface in `backend/src/JoiabagurPV.Application/Interfaces/` with methods `GetReportAsync(InventoryMovementReportFilterRequest)` and `ExportReportAsync(InventoryMovementReportFilterRequest)`
- [x] 3.2 Create `InventoryMovementReportService` implementation in `backend/src/JoiabagurPV.Application/Services/` — `GetReportAsync`: call repository, apply sorting (SortBy/SortDirection with default `productName` asc), apply pagination (Skip/Take), return `InventoryMovementReportResponse`
- [x] 3.3 Implement `ExportReportAsync` in `InventoryMovementReportService` — get full sorted result up to 50,000 rows; if exceeded throw exception with total count; otherwise generate Excel with ClosedXML (single sheet "Resumen movimientos", columns: Producto, SKU, Adiciones, Sustracciones, Diferencia, bold headers, auto-fit, number formatting), return `(MemoryStream, int TotalCount)`
- [x] 3.4 Register `IInventoryMovementReportService` / `InventoryMovementReportService` in DI at `backend/src/JoiabagurPV.Application/Extensions/ServiceCollectionExtensions.cs`

## 4. Backend API Controller

- [x] 4.1 Create `InventoryMovementReportController` at `backend/src/JoiabagurPV.API/Controllers/InventoryMovementReportController.cs` with route base `api/reports/inventory-movements` and `[Authorize(Roles = "Administrator")]`
- [x] 4.2 Add `GET api/reports/inventory-movements` endpoint — bind query params to `InventoryMovementReportFilterRequest`, validate required dates (return 400 if missing), normalize dates to UTC (same pattern as `SalesReportController`), call `GetReportAsync`, return JSON response
- [x] 4.3 Add `GET api/reports/inventory-movements/export` endpoint — same filters (no pagination), call `ExportReportAsync`, return file result with content type `application/vnd.openxmlformats-officedocument.spreadsheetml.sheet` and filename `reporte-movimientos-inventario-{yyyy-MM-dd-HH-mm}.xlsx`; catch export-limit exception and return 409 with `{ message, totalCount }`

## 5. Frontend Types and Service

- [x] 5.1 Create `frontend/src/types/inventory-movement-report.types.ts` with interfaces: `InventoryMovementReportFilter` (startDate, endDate, pointOfSaleId?, page, pageSize, sortBy?, sortDirection?), `InventoryMovementSummaryRow` (productId, productName, productSku, additions, subtractions, difference), `InventoryMovementReportResponse` (items, totalCount, page, pageSize, totalPages)
- [x] 5.2 Create `frontend/src/services/inventory-movement-report.service.ts` with `getReport(params)` (GET to `api/reports/inventory-movements` with query params) and `exportReport(params)` (GET to `api/reports/inventory-movements/export` with `responseType: 'blob'`; handle 409 by parsing JSON body and showing toast — follow pattern from `sales-report.service.ts`)

## 6. Frontend Report Page

- [x] 6.1 Create `frontend/src/pages/reports/inventory-movement-summary.tsx` with filter panel (required date range inputs, optional POS select from existing POS API), search button disabled until both dates provided
- [x] 6.2 Add results table using DataGrid with columns: Producto, SKU, Adiciones, Sustracciones, Diferencia — Adiciones/Sustracciones/Diferencia columns sortable (send sortBy/sortDirection to API on header click)
- [x] 6.3 Add pagination controls (DataGridPagination) matching the sales report pattern
- [x] 6.4 Add help legend text above the table: "Adiciones = entradas al inventario (devoluciones, ajustes positivos, importaciones). Sustracciones = salidas del inventario (ventas, ajustes negativos). Diferencia = Adiciones − Sustracciones."
- [x] 6.5 Add "Exportar a Excel" button with export-limit notice ("Máximo 50.000 filas. Si hay más resultados, ajuste los filtros.") — call `exportReport` with current filters and sorting, trigger blob download with filename `reporte-movimientos-inventario-{yyyy-MM-dd-HH-mm}.xlsx`, handle 409 with warning toast

## 7. Frontend Routing and Reports Hub

- [x] 7.1 Add route constant `INVENTORY_MOVEMENT_SUMMARY: '/reports/inventory-movement-summary'` in `frontend/src/routing/routes.tsx`
- [x] 7.2 Register the route with lazy import of the new page in `frontend/src/routing/app-routing-setup.tsx` (admin-only)
- [x] 7.3 Add report card "Resumen de movimientos de inventario" with description and link in `frontend/src/pages/reports/index.tsx`
