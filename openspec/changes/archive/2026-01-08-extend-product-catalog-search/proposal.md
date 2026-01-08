# Change: Extend Product Management with Catalog Browsing, Search, and Photo Management

## Why

The current product management implementation covers product creation, import, update, and basic photo upload. However, the system lacks essential catalog browsing and search capabilities that are critical for daily operations:

1. **No catalog browsing**: Administrators and operators cannot view product catalogs in a paginated list
2. **No search functionality**: Users cannot quickly find products by SKU or name, which is essential for manual sales registration
3. **Incomplete photo management**: While photo upload exists, there are no endpoints for deleting photos or changing the primary photo designation
4. **No role-based product filtering**: The system must enforce inventory-based product visibility for operators (operators can only see products assigned to their POS inventories)

These gaps prevent users from effectively managing and finding products in the catalog according to their role and assigned points of sale.

## What Changes

- **API Layer**: Add product catalog and search endpoints with role-based filtering:
  - GET /api/products (paginated catalog listing with sorting and inventory-based filtering for operators)
  - GET /api/products/search?query={term} (SKU exact match + name partial search with filtering)
  - DELETE /api/products/{id}/photos/{photoId} (delete photo with file cleanup, admin-only)
  - PUT /api/products/{id}/photos/{photoId}/primary (change primary photo, admin-only)

- **Application Layer**: Extend ProductService and ProductPhotoService:
  - Add GetProductsAsync with pagination parameters (page, pageSize, sortBy) **and user context for filtering**
  - Add SearchProductsAsync with query parameter **and user context for filtering**
  - Implement inventory-based filtering: operators see only products with Inventory records at assigned POS
  - Add DeletePhotoAsync with file cleanup logic (admin-only authorization)
  - Add SetPrimaryPhotoAsync with business rules (only one primary per product, admin-only authorization)

- **Infrastructure Layer**: Add database query optimization:
  - Add efficient JOIN between Product, Inventory, and UserPointOfSale for operator filtering
  - Add index on (PointOfSaleId, ProductId) in Inventory table for performance

- **Frontend**: Add catalog browsing and search pages:
  - Product catalog page with data grid/cards layout (role-aware content)
  - Pagination controls (50 items per page default)
  - Search input with real-time filtering (results filtered by backend based on role)
  - Photo management UI with delete and primary designation (admin-only)
  - Display quantity indicators and warnings for products with qty=0

## Impact

- **Affected specs**: 
  - Modify existing `product-management` capability with new requirements
  - Modify existing `access-control` capability with product filtering requirements
- **Affected code**:
  - `backend/src/JoiabagurPV.Application/Interfaces/` - extend IProductService and IProductPhotoService interfaces with user context
  - `backend/src/JoiabagurPV.Application/Services/` - extend ProductService and ProductPhotoService with filtering logic
  - `backend/src/JoiabagurPV.Infrastructure/Repositories/` - add inventory-based queries for product filtering
  - `backend/src/JoiabagurPV.API/Controllers/ProductsController.cs` - add GET catalog, GET search, and photo management endpoints with role validation
  - `backend/src/JoiabagurPV.API/Middleware/` - may need to pass user context to services
  - `frontend/src/pages/products/` - add ProductCatalogPage, integrate search
  - `frontend/src/services/` - add catalog and search API calls
  - `frontend/src/components/products/` - add photo management components (admin-only)
- **Dependencies**: 
  - Extends `add-product-management` change (must be completed first)
  - Depends on `add-authentication-user-management` for user context and role validation

