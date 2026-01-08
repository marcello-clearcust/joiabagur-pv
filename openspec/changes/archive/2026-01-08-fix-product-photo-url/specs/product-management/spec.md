# Product Management Delta

## MODIFIED Requirements

### Requirement: Product Catalog Listing API
The system SHALL provide a paginated product catalog endpoint with role-based filtering: administrators see all products, operators see only products with Inventory records at their assigned points of sale. **Product photos SHALL include accessible URL fields for display in UI.**

#### Scenario: Administrator retrieves full product catalog
- **WHEN** an authenticated administrator requests GET /api/products with page=1, pageSize=50
- **THEN** the system returns a paginated result with up to 50 products from the global Product table
- **AND** includes pagination metadata (totalCount, totalPages, currentPage)
- **AND** each product includes Id, SKU, Name, Price, PrimaryPhotoUrl, and AvailableQuantity
- **AND** PrimaryPhotoUrl is a complete, accessible URL (not just filename) if a primary photo exists

#### Scenario: Operator retrieves filtered product catalog
- **WHEN** an authenticated operator requests GET /api/products with page=1, pageSize=50
- **THEN** the system returns ONLY products that have Inventory records at operator's assigned points of sale
- **AND** includes products with Quantity = 0 if Inventory record exists
- **AND** excludes products with no Inventory records at assigned POS
- **AND** aggregates quantities from all assigned POS for multi-POS operators

#### Scenario: Sort products by name
- **WHEN** an authenticated user requests GET /api/products?sortBy=Name
- **THEN** the products are returned sorted alphabetically by name (filtered by role)

#### Scenario: Sort products by creation date
- **WHEN** an authenticated user requests GET /api/products?sortBy=CreatedAt
- **THEN** the products are returned sorted by creation date (filtered by role)

#### Scenario: Navigate to next page
- **WHEN** an authenticated user requests GET /api/products?page=2&pageSize=50
- **THEN** the system returns products 51-100 with correct pagination metadata (filtered by role)

#### Scenario: Operator with no assigned products
- **WHEN** an operator has no products in Inventory at assigned points of sale
- **THEN** GET /api/products returns an empty array with totalCount=0

#### Scenario: Require authentication for catalog access
- **WHEN** an unauthenticated user requests GET /api/products
- **THEN** the request is rejected with 401 Unauthorized

### Requirement: Product Search API
The system SHALL provide a search endpoint that performs SKU exact match and name partial match with role-based filtering: administrators search all products, operators search only products with Inventory records at their assigned points of sale. **Product photos in search results SHALL include accessible URL fields.**

#### Scenario: Administrator searches by exact SKU match
- **WHEN** an authenticated administrator requests GET /api/products/search?query=JOY-001
- **AND** a product with SKU "JOY-001" exists in the global catalog
- **THEN** the system returns the matching product with its primary photo
- **AND** the primary photo URL is a complete, accessible URL (not just filename)

#### Scenario: Operator searches by exact SKU match in assigned inventory
- **WHEN** an authenticated operator requests GET /api/products/search?query=JOY-001
- **AND** a product with SKU "JOY-001" has Inventory records at operator's assigned POS
- **THEN** the system returns the matching product with its primary photo and quantity
- **AND** the primary photo URL is a complete, accessible URL if photo exists

#### Scenario: Operator searches for product not in assigned inventory
- **WHEN** an authenticated operator requests GET /api/products/search?query=JOY-999
- **AND** a product with SKU "JOY-999" exists but has NO Inventory records at operator's assigned POS
- **THEN** the system returns an empty array (product is hidden from operator)

#### Scenario: Administrator searches by partial name match
- **WHEN** an authenticated administrator requests GET /api/products/search?query=Anillo
- **AND** products with names containing "Anillo" exist (case-insensitive)
- **THEN** the system returns all matching products from global catalog

#### Scenario: Operator searches by partial name match
- **WHEN** an authenticated operator requests GET /api/products/search?query=Anillo
- **AND** products with names containing "Anillo" exist
- **THEN** the system returns ONLY matching products that have Inventory records at assigned POS

#### Scenario: Search returns no results
- **WHEN** an authenticated user searches for a term that matches no accessible products
- **THEN** the system returns an empty array (not 404)

#### Scenario: Search with minimum query length
- **WHEN** an authenticated user provides a query with less than 2 characters
- **THEN** the request is rejected with 400 Bad Request and validation error

#### Scenario: Search result limit
- **WHEN** a search query matches more than 50 accessible products
- **THEN** only the first 50 results are returned (filtered by role)

#### Scenario: Require authentication for search
- **WHEN** an unauthenticated user requests the search endpoint
- **THEN** the request is rejected with 401 Unauthorized

## ADDED Requirements

### Requirement: Product Detail Endpoint Returns Photo URLs
The system SHALL return complete photo information including accessible URLs when retrieving individual product details via GET /api/products/{id}.

#### Scenario: Get product with photos
- **WHEN** an authenticated user requests GET /api/products/{id}
- **AND** the product has uploaded photos
- **THEN** the response includes a Photos array
- **AND** each photo object contains Id, ProductId, FileName, Url, DisplayOrder, IsPrimary, CreatedAt, UpdatedAt
- **AND** the Url field contains a complete, accessible URL constructed via IFileStorageService
- **AND** photos are ordered by DisplayOrder

#### Scenario: Get product without photos
- **WHEN** an authenticated user requests GET /api/products/{id}
- **AND** the product has no uploaded photos
- **THEN** the response includes an empty Photos array
- **AND** no errors are returned

#### Scenario: Photo URLs are accessible
- **WHEN** a product detail response includes photo URLs
- **THEN** each URL can be used directly in an HTML <img> src attribute
- **AND** URLs point to the correct storage location (local filesystem or cloud storage)
- **AND** URLs follow the format returned by IFileStorageService.GetUrlAsync()
