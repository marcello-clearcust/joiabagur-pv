## 1. Photo Management - Backend API

- [ ] 1.1 Add DeletePhotoAsync to IProductPhotoService interface (T-EP1-005-001)
- [ ] 1.2 Implement DeletePhotoAsync in ProductPhotoService with file cleanup via IFileStorageService (T-EP1-005-001)
- [ ] 1.3 Add ReorderPhotoAsync to IProductPhotoService interface (T-EP1-005-001)
- [ ] 1.4 Implement ReorderPhotoAsync with DisplayOrder updates (T-EP1-005-001)
- [ ] 1.5 Add SetPrimaryPhotoAsync to IProductPhotoService interface (T-EP1-005-001)
- [ ] 1.6 Implement SetPrimaryPhotoAsync with logic to unmark existing primary (T-EP1-005-001)
- [ ] 1.7 Add DELETE /api/products/{id}/photos/{photoId} endpoint (T-EP1-005-001)
- [ ] 1.8 Add PUT /api/products/{id}/photos/{photoId}/order endpoint with order parameter (T-EP1-005-001)
- [ ] 1.9 Add PUT /api/products/{id}/photos/{photoId}/primary endpoint (T-EP1-005-001)
- [ ] 1.10 Add authorization [Authorize(Roles = "Administrator")] to photo management endpoints (T-EP1-005-001)
- [ ] 1.11 Add proper error handling (404 for missing product/photo) (T-EP1-005-001)

## 2. Product Catalog - Backend API

- [ ] 2.1 Add GetProductsAsync to IProductService interface with pagination parameters and user context (T-EP1-006-001)
- [ ] 2.2 Implement GetProductsAsync in ProductService with pagination, sorting, and role-based filtering (T-EP1-006-001)
- [ ] 2.3 Implement inventory-based filtering: JOIN Product with Inventory and UserPointOfSale for operators (T-EP1-006-001)
- [ ] 2.4 Add index on (PointOfSaleId, ProductId) in Inventory table for query performance (T-EP1-006-001)
- [ ] 2.5 Create ProductListDto with essential fields (Id, SKU, Name, Price, PrimaryPhotoUrl, AvailableQuantity) (T-EP1-006-001)
- [ ] 2.6 Create PaginatedResultDto<T> for paginated responses (T-EP1-006-001)
- [ ] 2.7 Add GET /api/products endpoint with query parameters (page, pageSize, sortBy) (T-EP1-006-001)
- [ ] 2.8 Extract user ID and role from JWT claims in controller (T-EP1-006-001)
- [ ] 2.9 Pass user context to GetProductsAsync for filtering (T-EP1-006-001)
- [ ] 2.10 Include primary photo URL and aggregated quantity in product list responses (T-EP1-006-001)
- [ ] 2.11 Add pagination metadata (totalCount, totalPages, currentPage) (T-EP1-006-001)
- [ ] 2.12 Add sorting support (by Name, CreatedAt) (T-EP1-006-001)
- [ ] 2.13 Add authorization [Authorize] for authenticated users (admin and operator) (T-EP1-006-001)
- [ ] 2.14 Optimize query with Include for primary photos and Inventory filtering (T-EP1-006-001)

## 3. Product Catalog - Frontend

- [ ] 3.1 Create ProductCatalogPage component with Metronic layout (T-EP1-006-002)
- [ ] 3.2 Add useProductCatalog hook for data fetching and pagination (T-EP1-006-002)
- [ ] 3.3 Create ProductCard component displaying SKU, name, price, quantity, primary photo (T-EP1-006-002)
- [ ] 3.4 Add quantity indicator badge on product cards (show "Sin stock" for qty=0) (T-EP1-006-002)
- [ ] 3.5 Implement data grid or card layout (responsive) (T-EP1-006-002)
- [ ] 3.6 Add pagination controls at bottom (T-EP1-006-002)
- [ ] 3.7 Add links to product edit page from cards (admin) or view-only (operator) (T-EP1-006-002)
- [ ] 3.8 Add loading state with skeleton loaders (T-EP1-006-002)
- [ ] 3.9 Add empty state when no products exist or operator has no assigned products (T-EP1-006-002)
- [ ] 3.10 Add route /products for catalog page (T-EP1-006-002)
- [ ] 3.11 Add navigation menu entry for product catalog (visible to all authenticated users) (T-EP1-006-002)

## 4. Product Search - Backend API

- [ ] 4.1 Add SearchProductsAsync to IProductService interface with query parameter and user context (T-EP1-007-001)
- [ ] 4.2 Implement SearchProductsAsync with SKU exact match logic and role-based filtering (T-EP1-007-001)
- [ ] 4.3 Implement name partial match logic (case-insensitive) using LIKE or Contains with filtering (T-EP1-007-001)
- [ ] 4.4 Apply inventory-based filtering for operators (same JOIN as catalog) (T-EP1-007-001)
- [ ] 4.5 Add result limit (e.g., max 50 results) for performance (T-EP1-007-001)
- [ ] 4.6 Include primary photo and available quantity in search results (T-EP1-007-001)
- [ ] 4.7 Add GET /api/products/search?query={term} endpoint (T-EP1-007-001)
- [ ] 4.8 Extract user ID and role from JWT claims in controller (T-EP1-007-001)
- [ ] 4.9 Pass user context to SearchProductsAsync for filtering (T-EP1-007-001)
- [ ] 4.10 Add query parameter validation (min 2 characters) (T-EP1-007-001)
- [ ] 4.11 Add authorization [Authorize] for authenticated users (T-EP1-007-001)
- [ ] 4.12 Return empty array when no accessible results found (not 404) (T-EP1-007-001)

## 5. Product Search - Frontend

- [ ] 5.1 Create SearchInput component in catalog header (T-EP1-007-002)
- [ ] 5.2 Add debounced search input (300ms delay) for real-time search (T-EP1-007-002)
- [ ] 5.3 Integrate search with catalog display (filter results) (T-EP1-007-002)
- [ ] 5.4 Add search state management (query, results, loading) (T-EP1-007-002)
- [ ] 5.5 Display search results with quantity indicators (T-EP1-007-002)
- [ ] 5.6 Show "Sin stock" warning badge on products with qty=0 in search results (T-EP1-007-002)
- [ ] 5.7 Display search results replacing catalog or in overlay (T-EP1-007-002)
- [ ] 5.8 Add clear search button (X icon) to reset (T-EP1-007-002)
- [ ] 5.9 Add "No results found" message for empty results (T-EP1-007-002)
- [ ] 5.10 Add search loading indicator (T-EP1-007-002)
- [ ] 5.11 Prioritize SKU exact match in result display (highlight or show first) (T-EP1-007-002)

## 6. Photo Management - Frontend (Admin-Only)

- [ ] 6.1 Add role-based rendering: show photo management controls only for administrators (HU-EP1-005)
- [ ] 6.2 Add DeletePhotoButton component with confirmation dialog (HU-EP1-005)
- [ ] 6.3 Add drag-and-drop reordering to photo gallery (use react-beautiful-dnd or similar) (HU-EP1-005)
- [ ] 6.4 Add SetPrimaryPhotoButton component (star icon or similar) (HU-EP1-005)
- [ ] 6.5 Update photo gallery to show primary photo indicator (HU-EP1-005)
- [ ] 6.6 Add loading states for photo operations (HU-EP1-005)
- [ ] 6.7 Add error handling with toast notifications (HU-EP1-005)
- [ ] 6.8 Refresh photo gallery after operations (HU-EP1-005)
- [ ] 6.9 Display photos in read-only mode for operators (if they can access product edit) (HU-EP1-005)

## 7. Backend Testing

- [ ] 7.1 Unit tests for ProductService.GetProductsAsync (pagination logic) (Backend testing)
- [ ] 7.2 Unit tests for admin role: GetProductsAsync returns all products (Backend testing)
- [ ] 7.3 Unit tests for operator role: GetProductsAsync filters by inventory assignments (Backend testing)
- [ ] 7.4 Unit tests for operator with multiple POS: aggregate products without duplicates (Backend testing)
- [ ] 7.5 Unit tests for ProductService.SearchProductsAsync (SKU and name matching) (Backend testing)
- [ ] 7.6 Unit tests for admin search: searches all products (Backend testing)
- [ ] 7.7 Unit tests for operator search: filters by inventory assignments (Backend testing)
- [ ] 7.8 Unit tests for ProductPhotoService.DeletePhotoAsync (file cleanup) (Backend testing)
- [ ] 7.9 Unit tests for ProductPhotoService.ReorderPhotoAsync (Backend testing)
- [ ] 7.10 Unit tests for ProductPhotoService.SetPrimaryPhotoAsync (single primary rule) (Backend testing)
- [ ] 7.11 Integration tests for GET /api/products (pagination, sorting, filtering) (Deferred)
- [ ] 7.12 Integration tests for GET /api/products/search with role-based filtering (Deferred)
- [ ] 7.13 Integration tests for DELETE /api/products/{id}/photos/{photoId} (admin-only) (Deferred)
- [ ] 7.14 Integration tests for PUT photo management endpoints (admin-only) (Deferred)

## 8. Frontend Testing

- [ ] 8.1 Component tests for ProductCatalogPage (Frontend testing)
- [ ] 8.2 Component tests for ProductCard (Frontend testing)
- [ ] 8.3 Component tests for SearchInput (Frontend testing)
- [ ] 8.4 Tests for pagination interactions (Frontend testing)
- [ ] 8.5 Tests for search debouncing and filtering (Frontend testing)
- [ ] 8.6 Tests for photo management interactions (delete, reorder, set primary) (Frontend testing)
- [ ] 8.7 MSW mocks for catalog and search endpoints (Frontend testing)

