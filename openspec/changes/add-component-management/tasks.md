## 1. Component Master Table (HU-EP10-001)
- [x] 1.1 Create `ProductComponent` entity with EF configuration (Description max 35, CostPrice/SalePrice decimal(18,4) nullable, IsActive, unique constraint on Description)
- [x] 1.2 Create EF migration for ProductComponent table
- [x] 1.3 Implement `IProductComponentRepository` and `ProductComponentRepository`
- [x] 1.4 Implement `IProductComponentService` and `ProductComponentService` (CRUD + validation: unique description, price >= 0, description length)
- [x] 1.5 Add DTOs: `CreateComponentRequest`, `UpdateComponentRequest`, `ComponentResponseDto`
- [x] 1.6 Create `ProductComponentsController` with endpoints: GET (list with filters, pagination), GET/{id}, POST, PUT/{id} — all admin-only
- [x] 1.7 Frontend: Component list page at `/products/components` with active/inactive filter and description search
- [x] 1.8 Frontend: Create/edit component dialog with validation (description required max 35, prices optional >= 0)
- [x] 1.9 Frontend: Activate/deactivate toggle with confirmation
- [x] 1.10 Frontend: Admin-only navigation entry in Products section
- [x] 1.11 Backend unit tests for ProductComponentService
- [x] 1.12 Frontend tests for component list and CRUD dialogs

## 2. Component Assignment in Product Edit (HU-EP10-002)
- [x] 2.1 Create `ProductComponentAssignment` entity with EF configuration (ProductId FK, ComponentId FK, Quantity decimal(18,4), CostPrice/SalePrice decimal(18,4), DisplayOrder int, unique constraint on ProductId+ComponentId)
- [x] 2.2 Create EF migration for ProductComponentAssignment table
- [x] 2.3 Implement `IComponentAssignmentService` and `ComponentAssignmentService` (get, save, autocomplete, validation)
- [x] 2.4 Create endpoint GET /api/products/{id}/components (list assignments ordered by DisplayOrder)
- [x] 2.5 Create endpoint PUT /api/products/{id}/components (replace all assignments with validation)
- [x] 2.6 Create endpoint GET /api/product-components/search?query= (autocomplete, active only, min 2 chars, max 20 results)
- [x] 2.7 Frontend: Component assignment section in product edit page (conditionally rendered for admin role)
- [x] 2.8 Frontend: Autocomplete input with 300ms debounce for component search
- [x] 2.9 Frontend: Assignment row with Quantity, CostPrice, SalePrice fields (4-decimal precision)
- [x] 2.10 Frontend: Pre-fill prices from master when adding component
- [x] 2.11 Frontend: Real-time TotalCostPrice and TotalSalePrice calculation
- [x] 2.12 Frontend: Drag-and-drop reordering with DisplayOrder persistence on save
- [x] 2.13 Frontend: Block save if any assignment has missing/invalid prices
- [x] 2.14 Frontend: Prevent adding already-assigned components (duplicate check)
- [x] 2.15 Backend unit tests for ComponentAssignmentService
- [x] 2.16 Frontend tests for assignment section (add, remove, reorder, totals, validation)

## 3. Component Assignment in Product Creation (HU-EP10-003)
- [x] 3.1 Frontend: Integrate component assignment section into product creation page (reuse same component from 2.7)
- [x] 3.2 Backend: Implement sequential creation flow (POST product, then PUT components) or accept components in product creation request
- [x] 3.3 Validation: same blocking rules as in edit (prices required, quantity > 0, no duplicates)
- [x] 3.4 Frontend tests for product creation with and without components

## 4. Price Sync from Master (HU-EP10-004)
- [x] 4.1 Backend: Implement sync logic in ComponentAssignmentService (load master prices, compute diff, apply to assignments where master has values)
- [x] 4.2 Backend: Endpoint POST /api/products/{id}/components/sync-from-master (returns preview of changes)
- [x] 4.3 Frontend: "Apply master prices" button visible when product has components (admin-only)
- [x] 4.4 Frontend: Confirmation dialog with before/after price table showing each component
- [x] 4.5 Frontend: Apply confirmed sync, refresh assignments list and totals
- [x] 4.6 Backend unit tests for sync logic (full sync, partial sync, skip components without master prices)
- [x] 4.7 Frontend tests for sync flow (preview, confirm, cancel)

## 5. Price Deviation Warning (HU-EP10-005)
- [x] 5.1 Frontend: Calculate deviation = |Product.Price - TotalSalePrice| / TotalSalePrice in real-time
- [x] 5.2 Frontend: Show warning alert when deviation > 10% and TotalSalePrice > 0 and product has components
- [x] 5.3 Frontend: "Adjust to suggested price" button that updates Product.Price form field to TotalSalePrice
- [x] 5.4 Frontend: Hide warning when no components, TotalSalePrice = 0, or deviation <= 10%
- [x] 5.5 Frontend tests for deviation calculation, warning display, and quick adjust action

## 6. Component Templates (HU-EP10-006)
- [x] 6.1 Create `ComponentTemplate` and `ComponentTemplateItem` entities with EF configuration (unique constraint on TemplateId+ComponentId in items)
- [x] 6.2 Create EF migration for ComponentTemplate and ComponentTemplateItem tables
- [x] 6.3 Implement `IComponentTemplateService` and `ComponentTemplateService` (CRUD, apply-to-product with merge logic)
- [x] 6.4 Create `ComponentTemplateController` with endpoints: GET (list), GET/{id}, POST, PUT/{id}, DELETE/{id} — admin-only
- [x] 6.5 Create endpoint POST /api/products/{id}/components/apply-template (merge: skip existing, add new with master prices)
- [x] 6.6 Frontend: Template list page at `/products/component-templates`
- [x] 6.7 Frontend: Template create/edit page with name, description, and component list (autocomplete + quantity)
- [x] 6.8 Frontend: Template selector dropdown + "Apply" button in product edit/create component section
- [x] 6.9 Frontend: Show merge result (which components added, which skipped)
- [x] 6.10 Backend unit tests for template service (CRUD, merge logic)
- [x] 6.11 Frontend tests for template CRUD and apply flow

## 7. Margin Report (HU-EP10-007)
- [x] 7.1 Backend: Endpoint GET /api/reports/product-margins with filters (collectionId, maxMarginPercent, search) and pagination
- [x] 7.2 Backend: Aggregation logic (TotalCostPrice, TotalSalePrice, Margin, Margin % per product; summed totals)
- [x] 7.3 Backend: Endpoint GET /api/reports/product-margins/export for Excel download with current filters
- [x] 7.4 Frontend: Margin report page at `/reports/product-margins` with TanStack Table
- [x] 7.5 Frontend: Filter controls (collection dropdown, margin threshold, search input)
- [x] 7.6 Frontend: Aggregated totals row at bottom of table
- [x] 7.7 Frontend: Export to Excel button
- [x] 7.8 Backend unit tests for report endpoint and aggregation
- [x] 7.9 Frontend tests for report page (filters, totals, export)

## 8. Products Without Components Report (HU-EP10-008)
- [x] 8.1 Backend: Endpoint GET /api/reports/products-without-components with filters (collectionId, search) and pagination
- [x] 8.2 Frontend: Report page at `/reports/products-without-components` with TanStack Table
- [x] 8.3 Frontend: "Edit" button per row linking to `/products/{id}/edit`
- [x] 8.4 Frontend: Empty state message "No hay productos sin componentes"
- [x] 8.5 Backend unit tests for report endpoint
- [x] 8.6 Frontend tests for report page (list, navigation, empty state)
