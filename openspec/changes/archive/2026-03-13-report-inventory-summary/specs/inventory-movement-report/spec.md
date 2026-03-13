## ADDED Requirements

### Requirement: Inventory Movement Summary API with Date and POS Filters

The system SHALL provide a REST API endpoint at `GET /api/reports/inventory-movements` that returns a paginated summary of inventory movements aggregated by product. Each row SHALL contain the total additions (sum of positive `QuantityChange`), total subtractions (sum of absolute value of negative `QuantityChange`), and the difference (additions minus subtractions) for a product within the filtered date range. Only administrators SHALL access this endpoint.

#### Scenario: Administrator retrieves inventory movement summary with required date filters

- **WHEN** an authenticated administrator requests `GET /api/reports/inventory-movements?startDate=2025-01-01&endDate=2025-03-31`
- **THEN** the system returns a paginated response with `items`, `totalCount`, `page`, `pageSize`, `totalPages`
- **AND** each item contains `productId`, `productName`, `productSku`, `additions`, `subtractions`, `difference`
- **AND** `additions` equals the sum of all positive `QuantityChange` values for that product within the date range
- **AND** `subtractions` equals the sum of the absolute values of all negative `QuantityChange` values for that product within the date range
- **AND** `difference` equals `additions` minus `subtractions`

#### Scenario: Filter by point of sale

- **WHEN** an authenticated administrator requests `GET /api/reports/inventory-movements?startDate=2025-01-01&endDate=2025-03-31&pointOfSaleId=5`
- **THEN** the system returns only movements associated with inventories belonging to the specified point of sale
- **AND** aggregation is still grouped by product across that POS only

#### Scenario: No POS filter returns all POS combined

- **WHEN** an authenticated administrator requests `GET /api/reports/inventory-movements` without `pointOfSaleId`
- **THEN** the system aggregates movements across all points of sale per product

#### Scenario: Missing date filters returns 400

- **WHEN** an authenticated administrator requests `GET /api/reports/inventory-movements` without `startDate` or `endDate`
- **THEN** the system returns HTTP 400 Bad Request

#### Scenario: Empty result set

- **WHEN** the applied filters match zero inventory movements
- **THEN** the system returns an empty `items` list with `totalCount = 0`

#### Scenario: Non-administrator access denied

- **WHEN** an authenticated operator requests `GET /api/reports/inventory-movements`
- **THEN** the system returns HTTP 403 Forbidden

#### Scenario: Unauthenticated access denied

- **WHEN** an unauthenticated user requests `GET /api/reports/inventory-movements`
- **THEN** the system returns HTTP 401 Unauthorized

### Requirement: Inventory Movement Summary Sorting

The system SHALL support server-side sorting on the summary columns so the administrator can rank products by movement volume.

#### Scenario: Sort by additions descending

- **WHEN** an authenticated administrator requests `GET /api/reports/inventory-movements?startDate=2025-01-01&endDate=2025-03-31&sortBy=additions&sortDirection=desc`
- **THEN** the returned items are ordered by `additions` in descending order

#### Scenario: Sort by subtractions ascending

- **WHEN** an authenticated administrator requests with `sortBy=subtractions&sortDirection=asc`
- **THEN** the returned items are ordered by `subtractions` in ascending order

#### Scenario: Sort by difference

- **WHEN** an authenticated administrator requests with `sortBy=difference&sortDirection=desc`
- **THEN** the returned items are ordered by `difference` in descending order

#### Scenario: Default sort when no sortBy specified

- **WHEN** no `sortBy` parameter is provided
- **THEN** the system SHALL sort by `productName` ascending by default

### Requirement: Inventory Movement Summary Excel Export with Row Limit

The system SHALL provide a REST API endpoint at `GET /api/reports/inventory-movements/export` that generates an Excel file (.xlsx) with the full aggregated result set using the same filters as the report API. The export is limited to 50,000 rows.

#### Scenario: Successful export within row limit

- **WHEN** an authenticated administrator requests `GET /api/reports/inventory-movements/export` with filters matching <= 50,000 aggregated product rows
- **THEN** the system returns an Excel file with content type `application/vnd.openxmlformats-officedocument.spreadsheetml.sheet`
- **AND** the filename follows the pattern `reporte-movimientos-inventario-{yyyy-MM-dd-HH-mm}.xlsx`

#### Scenario: Excel sheet format

- **WHEN** the Excel file is generated
- **THEN** it SHALL contain a single sheet named "Resumen movimientos"
- **AND** columns in order: Producto, SKU, Adiciones, Sustracciones, Diferencia
- **AND** headers are formatted in bold
- **AND** numeric columns use appropriate number formatting
- **AND** columns are auto-fitted to content width

#### Scenario: Export respects current sort order

- **WHEN** the export request includes `sortBy` and `sortDirection` parameters
- **THEN** the Excel rows SHALL follow the specified sort order

#### Scenario: Export exceeds 50,000-row limit

- **WHEN** an authenticated administrator requests `GET /api/reports/inventory-movements/export` with filters matching more than 50,000 aggregated product rows
- **THEN** the system returns HTTP 409 Conflict
- **AND** the response body includes `{ "message": "Más de 50.000 productos en el resultado. Ajuste los filtros para exportar.", "totalCount": <real count> }`

#### Scenario: Non-administrator export access denied

- **WHEN** an authenticated operator requests `GET /api/reports/inventory-movements/export`
- **THEN** the system returns HTTP 403 Forbidden

#### Scenario: Unauthenticated export access denied

- **WHEN** an unauthenticated user requests `GET /api/reports/inventory-movements/export`
- **THEN** the system returns HTTP 401 Unauthorized

### Requirement: Inventory Movement Summary Frontend Page

The system SHALL provide a frontend page at `/reports/inventory-movement-summary` accessible from the Reports hub. The page SHALL display a filter panel with mandatory date range, optional POS selector, a paginated sortable results table, and an Excel export button.

#### Scenario: Reports hub includes inventory movement summary card

- **WHEN** an administrator navigates to the Reports hub (`/reports`)
- **THEN** a card for "Resumen de movimientos de inventario" is displayed with a brief description and link to `/reports/inventory-movement-summary`

#### Scenario: Date filters are required before search

- **WHEN** an administrator navigates to `/reports/inventory-movement-summary`
- **THEN** the page displays date range inputs (start date and end date) that MUST be filled before querying
- **AND** the search button is disabled until both dates are provided

#### Scenario: Optional POS filter

- **WHEN** the filter panel is displayed
- **THEN** a POS selector is available with "Todos los puntos de venta" as default
- **AND** the list of POS is populated from the existing POS API

#### Scenario: Results table with sortable columns

- **WHEN** report results are displayed
- **THEN** a table shows columns: Producto, SKU, Adiciones, Sustracciones, Diferencia
- **AND** the Adiciones, Sustracciones, and Diferencia columns are sortable (clicking toggles asc/desc)
- **AND** pagination controls allow navigating between pages

#### Scenario: Help legend explains terminology

- **WHEN** the inventory movement summary page is displayed
- **THEN** a visible text or tooltip explains: "Adiciones = entradas al inventario (devoluciones, ajustes positivos, importaciones). Sustracciones = salidas del inventario (ventas, ajustes negativos). Diferencia = Adiciones − Sustracciones."

#### Scenario: Export button triggers download

- **WHEN** the administrator clicks "Exportar a Excel"
- **AND** the current filters produce <= 50,000 rows
- **THEN** the browser downloads the generated Excel file with name `reporte-movimientos-inventario-{yyyy-MM-dd-HH-mm}.xlsx`

#### Scenario: Export button shows warning on 409

- **WHEN** the administrator clicks "Exportar a Excel"
- **AND** the API returns 409 with `totalCount`
- **THEN** a warning toast is displayed with the message from the API response

#### Scenario: Export limit notice displayed

- **WHEN** the inventory movement summary page is displayed
- **THEN** a notice near the export button reads: "Máximo 50.000 filas. Si hay más resultados, ajuste los filtros."

#### Scenario: Operator cannot access the report

- **WHEN** an operator is logged in
- **THEN** the "Resumen de movimientos de inventario" card is NOT visible in the Reports hub
- **AND** navigating directly to `/reports/inventory-movement-summary` is denied
