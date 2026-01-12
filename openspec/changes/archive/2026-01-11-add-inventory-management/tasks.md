## 1. Backend Domain Layer

- [x] 1.1 Create Inventory entity (verify matches data model: ProductId, PointOfSaleId, Quantity, IsActive, LastUpdatedAt, CreatedAt)
- [x] 1.2 Create InventoryMovement entity (verify matches data model: InventoryId, SaleId, ReturnId, UserId, MovementType enum, QuantityChange, QuantityBefore, QuantityAfter, Reason, MovementDate, CreatedAt)
- [x] 1.3 Create MovementType enum (Sale, Return, Adjustment, Import)
- [x] 1.4 Add unique constraint on Inventory(ProductId, PointOfSaleId) in DbContext
- [x] 1.5 Configure Inventory entity relationships (Product, PointOfSale)
- [x] 1.6 Configure InventoryMovement entity relationships (Inventory, Sale, Return, User)
- [x] 1.7 Add indexes as defined in data model (PointOfSaleId+Quantity, ProductId, PointOfSaleId+ProductId+IsActive)

## 1A. Backend Application Layer - Shared Excel Template Service

- [x] 1A.1 Create IExcelTemplateService interface
- [x] 1A.2 Implement ExcelTemplateService using ClosedXML
- [x] 1A.3 Implement template generation with common formatting (bold headers, protected header row)
- [x] 1A.4 Implement data validation rules support (number ranges, text formats)
- [x] 1A.5 Implement example row generation
- [x] 1A.6 Implement instructions/comments support

## 2. Backend Infrastructure Layer

- [x] 2.1 Create Inventory repository interface (IInventoryRepository)
- [x] 2.2 Implement Inventory repository (InventoryRepository) with EF Core
- [x] 2.3 Create InventoryMovement repository interface (IInventoryMovementRepository)
- [x] 2.4 Implement InventoryMovement repository (InventoryMovementRepository) with EF Core
- [x] 2.5 Add repository methods: FindByProductAndPointOfSale, FindByPointOfSale, FindByProduct, FindActiveByPointOfSales
- [x] 2.6 Add repository methods for InventoryMovement: FindByInventory, FindByFilters (product, POS, date range)
- [x] 2.7 Create database migration for Inventory and InventoryMovement tables
- [x] 2.8 Verify migration includes all indexes from data model

## 3. Backend Application Layer - Assignment Service

- [x] 3.1 Create IInventoryService interface
- [x] 3.2 Implement InventoryService with assignment methods
- [x] 3.3 Implement AssignProduct method (single product, validate product exists and active, check for duplicate)
- [x] 3.4 Implement AssignProducts method (bulk assignment, batch processing)
- [x] 3.5 Implement UnassignProduct method (validate Quantity = 0, set IsActive = false)
- [x] 3.6 Implement GetAssignedProducts method (filter by PointOfSaleId, IsActive = true)
- [x] 3.7 Implement ReactivateAssignment method (handle IsActive = false → true, preserve quantity)
- [x] 3.8 Add validation: prevent assigning inactive products
- [x] 3.9 Add validation: prevent duplicate assignments
- [x] 3.10 Add validation: prevent unassignment when Quantity > 0

## 4. Backend Application Layer - Stock Import Service

- [x] 4.1 Create IStockImportService interface
- [x] 4.2 Implement StockImportService using ClosedXML
- [x] 4.3 Implement Excel validation (required columns: SKU, Quantity with exact name matching)
- [x] 4.4 Implement SKU validation (must exist in Product catalog)
- [x] 4.5 Implement Quantity validation (>= 0)
- [x] 4.6 Implement ImportStock method (validate all rows first, show all errors, process valid rows on confirmation)
- [x] 4.7 Handle implicit assignment (create Inventory if not exists, set IsActive = true)
- [x] 4.8 Handle quantity addition (add to existing quantity)
- [x] 4.9 Return validation errors with detailed format (row number, field name, error message, grouped by type)
- [x] 4.10 Return import summary (rows processed, assignments created, quantities updated)
- [x] 4.11 Implement GenerateTemplate method using shared IExcelTemplateService (create Excel template with exact column headers "SKU" and "Quantity", example row, instructions/comments, data validation rules, protected header row, formatting)

## 5. Backend Application Layer - Adjustment Service

- [x] 5.1 Implement AdjustStock method in InventoryService
- [x] 5.2 Validate product is assigned (IsActive = true)
- [x] 5.3 Validate adjustment reason is provided
- [x] 5.4 Calculate resulting quantity and validate >= 0
- [x] 5.5 Update Inventory.Quantity and LastUpdatedAt
- [x] 5.6 Create InventoryMovement record (type Adjustment, record QuantityBefore/After/Change, reason)
- [x] 5.7 Return error if product not assigned
- [x] 5.8 Return error if adjustment would result in negative stock

## 6. Backend Application Layer - Stock Query Service

- [x] 6.1 Implement GetStockByPointOfSale method (filter by PointOfSaleId, IsActive = true)
- [x] 6.2 Implement GetCentralizedStock method (aggregate by ProductId, sum quantities)
- [x] 6.3 Implement GetStockBreakdown method (group by ProductId and PointOfSaleId)
- [x] 6.4 Add pagination support (max 50 items/page)
- [x] 6.5 Add operator filtering (restrict to assigned points of sale via access control integration)

## 6A. Backend Application Layer - Stock Validation Service (for Sales Integration)

- [x] 6A.1 Create IStockValidationService interface
- [x] 6A.2 Implement StockValidationService
- [x] 6A.3 Implement ValidateStockAvailability method (check assignment + sufficient quantity)
- [x] 6A.4 Add validation: product must be assigned (IsActive = true)
- [x] 6A.5 Add validation: Quantity >= requested quantity
- [x] 6A.6 Implement low stock warning (check MinimumThreshold if configured)
- [x] 6A.7 Return detailed error messages with available quantity
- [x] 6A.8 Support multi-unit validation (quantity > 1)
- [x] 6A.9 Register service in DI container for use by sales-management

## 6B. Backend Application Layer - Automatic Movement Creation (for Sales Integration)

- [x] 6B.1 Extend InventoryService with CreateSaleMovement method
- [x] 6B.2 Implement atomic stock update + movement creation in single transaction
- [x] 6B.3 Create InventoryMovement record with type "Sale"
- [x] 6B.4 Record QuantityBefore, QuantityChange (negative), QuantityAfter
- [x] 6B.5 Link movement to Sale via SaleId
- [x] 6B.6 Update Inventory.Quantity and LastUpdatedAt atomically
- [x] 6B.7 Implement transaction rollback on failure
- [x] 6B.8 Return movement record and updated inventory to calling service

## 7. Backend Application Layer - Movement History Service

- [x] 7.1 Create IInventoryMovementService interface
- [x] 7.2 Implement InventoryMovementService
- [x] 7.3 Implement GetMovementHistory method with filters (product, point of sale, date range)
- [x] 7.4 Add pagination support (max 50 items/page)
- [x] 7.5 Order results by MovementDate descending
- [x] 7.6 Return movement details (type, quantity change, before/after, user, date, reason)

## 8. Backend API Layer - Inventory Controller

- [x] 8.1 Create InventoryController
- [x] 8.2 Add POST /api/inventory/assign endpoint (single and bulk assignment)
- [x] 8.3 Add POST /api/inventory/unassign endpoint (validate Quantity = 0)
- [x] 8.4 Add GET /api/inventory/assigned?pointOfSaleId={id} endpoint
- [x] 8.5 Add GET /api/inventory/import-template endpoint (download Excel template)
- [x] 8.6 Add POST /api/inventory/import endpoint (Excel file upload, point of sale selection)
- [x] 8.7 Add GET /api/inventory?pointOfSaleId={id} endpoint (stock by POS)
- [x] 8.8 Add GET /api/inventory/centralized endpoint (admin only)
- [x] 8.9 Add POST /api/inventory/adjustment endpoint (manual adjustment)
- [x] 8.10 Add GET /api/inventory-movements endpoint (history with filters, default to last 30 days)
- [x] 8.11 Add JWT authentication to all endpoints
- [x] 8.12 Add role-based authorization (admin for assignment/import/adjustment, operator for view)
- [x] 8.13 Add operator filtering (restrict to assigned points of sale)
- [x] 8.14 Create DTOs: AssignProductDto, UnassignProductDto, StockImportDto, StockAdjustmentDto, InventoryDto, InventoryMovementDto
- [x] 8.15 Add input validation using FluentValidation
- [x] 8.16 Add error handling and appropriate HTTP status codes

## 9. Backend Testing

- [x] 9.1 Write unit tests for InventoryService.AssignProduct (success, duplicate, inactive product)
- [x] 9.2 Write unit tests for InventoryService.UnassignProduct (success, quantity > 0 error)
- [x] 9.3 Write unit tests for StockImportService (valid import, implicit assignment, invalid SKU, template generation)
- [x] 9.4 Write unit tests for InventoryService.AdjustStock (success, negative stock error, unassigned product error)
- [x] 9.5 Write unit tests for stock validation (non-negative enforcement)
- [x] 9.6 Write unit tests for StockValidationService (sufficient stock, unassigned product, insufficient stock, low stock warning)
- [x] 9.7 Write unit tests for CreateSaleMovement (success, transaction rollback on failure)
- [x] 9.8 Write integration tests for InventoryController endpoints (with Testcontainers)
- [x] 9.9 Write integration tests for operator filtering (access control integration)
- [x] 9.10 Write integration tests for Excel import workflow
- [x] 9.11 Write integration tests for movement history creation and querying
- [x] 9.12 Write integration tests for stock validation service (integration with sales-management)
- [x] 9.13 Write integration tests for automatic movement creation on sale
- [x] 9.14 Achieve minimum 70% code coverage for inventory services

## 10. Frontend - Inventory Assignment Module

- [x] 10.1 Create inventory assignment page route (/inventory/assign)
- [x] 10.2 Create PointOfSale selector component (admin: all POS, operator: assigned only)
- [x] 10.3 Create product selection component (multi-select from catalog)
- [x] 10.4 Create assignment form (POS selector + product multi-select)
- [x] 10.5 Add bulk assignment button and handler
- [x] 10.6 Create assigned products list view (filtered by selected POS)
- [x] 10.7 Add unassign button for each product (with confirmation dialog)
- [x] 10.8 Add validation: show error if product already assigned
- [x] 10.9 Add success/error toast notifications
- [x] 10.10 Add loading states during assignment operations

## 11. Frontend - Stock Import Module

- [x] 11.1 Create stock import page route (/inventory/import)
- [x] 11.2 Create file upload component (Excel file picker)
- [x] 11.3 Create PointOfSale selector for import (user selects POS in UI before upload)
- [x] 11.4 Add Excel template download button (calls GET /api/inventory/import-template, visible before file selection)
- [x] 11.5 Add Excel format instructions (SKU, Quantity columns with exact names)
- [x] 11.6 Implement file validation (file type, size)
- [x] 11.7 Add preview functionality (show preview table with first few rows when file selected, before upload)
- [x] 11.8 Add validation step (show all errors with detailed format: row number, field name, error message, grouped by type)
- [x] 11.9 Add import confirmation dialog with summary (total rows, breakdown: "X products will have stock added, Y products will be assigned")
- [x] 11.10 Display import summary after completion (rows processed, assignments created)
- [x] 11.11 Show implicit assignment notifications
- [x] 11.12 Add error handling for invalid Excel format
- [x] 11.13 Add template download link to help/documentation pages

## 12. Frontend - Stock View Module

- [x] 12.1 Create stock by POS page route (/inventory/stock)
- [x] 12.2 Create PointOfSale selector (operator: assigned only)
- [x] 12.3 Create stock table component (SKU, product name, quantity)
- [x] 12.4 Add visual indicator for zero quantity products
- [x] 12.5 Add pagination controls (max 50 items/page)
- [x] 12.6 Create centralized stock page route (/inventory/centralized) - admin only
- [x] 12.7 Create aggregated stock view (total per product)
- [x] 12.8 Add expandable breakdown by point of sale
- [x] 12.9 Add search/filter functionality (by product name or SKU)
- [x] 12.10 Add loading states and error handling

## 13. Frontend - Stock Adjustment Module

- [x] 13.1 Create stock adjustment page route (/inventory/adjust)
- [x] 13.2 Create adjustment form (product selector, POS selector, quantity change, reason)
- [x] 13.3 Add quantity input with +/- buttons or direct input
- [x] 13.4 Add reason text area (required, max 500 characters)
- [x] 13.5 Add validation: product must be assigned to selected POS
- [x] 13.6 Add validation: prevent negative stock
- [x] 13.7 Show current stock quantity before adjustment
- [x] 13.8 Show calculated new quantity after adjustment
- [x] 13.9 Add confirmation dialog before applying adjustment
- [x] 13.10 Add success/error toast notifications

## 14. Frontend - Movement History Module

- [x] 14.1 Create movement history page route (/inventory/movements)
- [x] 14.2 Create filter component (product, point of sale, date range)
- [x] 14.3 Set default date range to last 30 days if not specified
- [x] 14.4 Create movements table (type, quantity change, before/after, user, date, reason)
- [x] 14.5 Add pagination controls (max 50 items/page)
- [x] 14.6 Add date range picker component (defaults to last 30 days)
- [x] 14.7 Add product search/selector for filtering
- [x] 14.8 Add point of sale selector for filtering
- [x] 14.9 Display movement type badges (Sale, Return, Adjustment, Import)
- [x] 14.10 Add export functionality (optional, for Fase 2)
- [x] 14.11 Add loading states and error handling

## 15. Frontend Testing

- [x] 15.1 Write component tests for assignment form (React Testing Library)
- [x] 15.2 Write component tests for stock import form
- [x] 15.3 Write component tests for stock adjustment form
- [x] 15.4 Write component tests for stock view tables
- [x] 15.5 Write component tests for movement history filters
- [x] 15.6 Write integration tests with MSW for API mocking
- [x] 15.7 Write E2E tests with Playwright (assignment workflow)
- [x] 15.8 Write E2E tests with Playwright (import workflow)
- [x] 15.9 Write E2E tests with Playwright (adjustment workflow)
- [x] 15.10 Achieve minimum 70% code coverage for inventory components

## 16. Integration and Validation

- [x] 16.1 Verify integration with access-control (operator product filtering)
- [x] 16.2 Verify integration with product-management (product existence validation)
- [x] 16.3 Verify integration with point-of-sale-management (POS existence validation)
- [x] 16.4 Test end-to-end assignment → import → adjustment → view workflow
- [x] 16.5 Test operator restrictions (cannot access unassigned POS)
- [x] 16.6 Test admin full access (all POS, all products)
- [x] 16.7 Verify non-negative stock validation in all code paths
- [x] 16.8 Verify audit trail completeness (all movements recorded)
- [x] 16.9 Performance test Excel import with 500 products
- [x] 16.10 Load test stock queries with pagination

## 17. Documentation

- [x] 17.1 Update API documentation (Scalar/Swagger) with inventory endpoints
- [x] 17.2 Document Excel import format and requirements
- [x] 17.3 Document assignment/unassignment workflow
- [x] 17.4 Document adjustment reason requirements
- [x] 17.5 Add code comments for complex business logic
- [x] 17.6 Update README with inventory management capabilities

