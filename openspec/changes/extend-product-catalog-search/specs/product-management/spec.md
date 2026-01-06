## ADDED Requirements

### Requirement: Product Catalog Listing API
The system SHALL provide a paginated product catalog endpoint with role-based filtering: administrators see all products, operators see only products with Inventory records at their assigned points of sale.

#### Scenario: Administrator retrieves full product catalog
- **WHEN** an authenticated administrator requests GET /api/products with page=1, pageSize=50
- **THEN** the system returns a paginated result with up to 50 products from the global Product table
- **AND** includes pagination metadata (totalCount, totalPages, currentPage)
- **AND** each product includes Id, SKU, Name, Price, PrimaryPhotoUrl, and AvailableQuantity

#### Scenario: Operator retrieves filtered product catalog
- **WHEN** an authenticated operator requests GET /api/products with page=1, pageSize=50
- **THEN** the system returns ONLY products that have Inventory records at operator's assigned points of sale
- **AND** includes products with Quantity = 0 if Inventory record exists
- **AND** excludes products with no Inventory records at assigned POS
- **AND** aggregates quantities from all assigned POS for multi-POS operators

#### Scenario: Operator with multiple POS assignments
- **WHEN** an operator assigned to POS-A and POS-B requests GET /api/products
- **AND** Product-1 has Inventory at POS-A only
- **AND** Product-2 has Inventory at POS-B only
- **AND** Product-3 has Inventory at both POS-A and POS-B
- **THEN** all three products are returned without duplicates
- **AND** quantities are aggregated across assigned POS

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
The system SHALL provide a search endpoint that performs SKU exact match and name partial match with role-based filtering: administrators search all products, operators search only products with Inventory records at their assigned points of sale.

#### Scenario: Administrator searches by exact SKU match
- **WHEN** an authenticated administrator requests GET /api/products/search?query=JOY-001
- **AND** a product with SKU "JOY-001" exists in the global catalog
- **THEN** the system returns the matching product with its primary photo

#### Scenario: Operator searches by exact SKU match in assigned inventory
- **WHEN** an authenticated operator requests GET /api/products/search?query=JOY-001
- **AND** a product with SKU "JOY-001" has Inventory records at operator's assigned POS
- **THEN** the system returns the matching product with its primary photo and quantity

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

### Requirement: Product Photo Deletion API
The system SHALL provide an endpoint for deleting product photos with automatic file cleanup and primary photo reassignment. This operation is restricted to administrators only.

#### Scenario: Delete non-primary photo successfully
- **WHEN** an authenticated administrator DELETEs /api/products/{id}/photos/{photoId} for a non-primary photo
- **THEN** the photo file is removed from storage via IFileStorageService
- **AND** the ProductPhoto record is deleted from the database
- **AND** the operation returns 204 No Content

#### Scenario: Delete primary photo
- **WHEN** an authenticated administrator deletes the primary photo
- **AND** other photos exist for the product
- **THEN** the photo is deleted and another photo is automatically designated as primary
- **AND** the operation returns 204 No Content

#### Scenario: Delete only photo for product
- **WHEN** an authenticated administrator deletes the only photo for a product
- **THEN** the photo is deleted successfully
- **AND** no other photo is designated as primary

#### Scenario: Delete photo from non-existent product
- **WHEN** an authenticated administrator attempts to DELETE /api/products/{id}/photos/{photoId} with non-existent product ID
- **THEN** the system returns 404 Not Found

#### Scenario: Delete non-existent photo
- **WHEN** an authenticated administrator attempts to delete a photo that does not exist
- **THEN** the system returns 404 Not Found

#### Scenario: Unauthorized photo deletion
- **WHEN** an operator or unauthenticated user attempts to delete a photo
- **THEN** the request is rejected with 401 Unauthorized or 403 Forbidden

### Requirement: Product Photo Reordering API
The system SHALL provide an endpoint for updating photo display order. This operation is restricted to administrators only.

#### Scenario: Reorder photo successfully
- **WHEN** an authenticated administrator PUTs to /api/products/{id}/photos/{photoId}/order with newOrder=2
- **THEN** the photo's DisplayOrder is updated to 2
- **AND** other photos' DisplayOrder values are adjusted accordingly
- **AND** the operation returns 200 OK with updated photo

#### Scenario: Reorder with invalid order
- **WHEN** an authenticated administrator provides a DisplayOrder less than 1 or greater than photo count
- **THEN** the request is rejected with 400 Bad Request

#### Scenario: Reorder photo in non-existent product
- **WHEN** an authenticated administrator attempts to reorder a photo for a non-existent product
- **THEN** the system returns 404 Not Found

#### Scenario: Unauthorized photo reordering
- **WHEN** an operator or unauthenticated user attempts to reorder photos
- **THEN** the request is rejected with 401 Unauthorized or 403 Forbidden

### Requirement: Product Photo Primary Designation API
The system SHALL provide an endpoint for changing the primary photo of a product. This operation is restricted to administrators only.

#### Scenario: Set primary photo successfully
- **WHEN** an authenticated administrator PUTs to /api/products/{id}/photos/{photoId}/primary
- **THEN** the specified photo has IsPrimary set to true
- **AND** all other photos for that product have IsPrimary set to false
- **AND** the operation returns 200 OK

#### Scenario: Set primary on already primary photo
- **WHEN** an authenticated administrator sets a photo as primary that is already primary
- **THEN** the operation succeeds with no changes
- **AND** returns 200 OK

#### Scenario: Set primary on non-existent product
- **WHEN** an authenticated administrator attempts to set primary photo for non-existent product
- **THEN** the system returns 404 Not Found

#### Scenario: Set primary on non-existent photo
- **WHEN** an authenticated administrator attempts to set a non-existent photo as primary
- **THEN** the system returns 404 Not Found

#### Scenario: Unauthorized primary designation
- **WHEN** an operator or unauthenticated user attempts to set primary photo
- **THEN** the request is rejected with 401 Unauthorized or 403 Forbidden

### Requirement: Product Catalog Frontend
The system SHALL provide a user interface for browsing the product catalog with pagination and visual product cards. Content is filtered by the backend based on user role (admin sees all, operator sees only assigned products).

#### Scenario: Display product catalog page
- **WHEN** an authenticated user navigates to the product catalog page
- **THEN** products are displayed in a grid or card layout
- **AND** each card shows SKU, name, price, quantity (if operator), and primary photo thumbnail
- **AND** pagination controls are displayed at the bottom
- **AND** products with quantity = 0 display a warning badge or indicator

#### Scenario: Navigate through pages
- **WHEN** an authenticated user clicks the next page button
- **THEN** the next page of products is loaded and displayed
- **AND** the current page indicator is updated

#### Scenario: Navigate to product edit
- **WHEN** an authenticated administrator clicks on a product card
- **THEN** the user is navigated to the product edit page

#### Scenario: Empty catalog display
- **WHEN** no products exist in the catalog
- **THEN** an empty state message is displayed
- **AND** prompts the user to import or create products

#### Scenario: Catalog loading state
- **WHEN** the catalog is being loaded
- **THEN** skeleton loaders are displayed in the card layout

#### Scenario: Mobile responsive catalog
- **WHEN** an authenticated user views the catalog on mobile
- **THEN** the layout adjusts to a single column with responsive cards

### Requirement: Product Search Frontend
The system SHALL provide a search interface integrated into the product catalog for real-time product lookup. Search results are filtered by the backend based on user role.

#### Scenario: Search with real-time filtering
- **WHEN** an authenticated user types in the search input
- **THEN** the search is triggered after 300ms debounce delay
- **AND** matching accessible products are displayed replacing or filtering the catalog
- **AND** products with quantity = 0 display a warning indicator

#### Scenario: Display SKU exact match prominently
- **WHEN** a search matches a product by exact SKU
- **THEN** the matching product is displayed first or highlighted

#### Scenario: Display partial name matches
- **WHEN** a search matches products by partial name
- **THEN** all matching products are displayed in the catalog

#### Scenario: Clear search results
- **WHEN** an authenticated user clicks the clear search button
- **THEN** the search query is cleared
- **AND** the full catalog is displayed again

#### Scenario: No search results
- **WHEN** a search returns no matching products
- **THEN** a "No results found" message is displayed
- **AND** suggests trying different search terms

#### Scenario: Search loading indicator
- **WHEN** a search is in progress
- **THEN** a loading indicator is displayed in the search input

### Requirement: Product Photo Management Frontend
The system SHALL provide UI controls for deleting, reordering, and designating primary photos in the product edit page. These controls are visible and functional only for administrators.

#### Scenario: Delete photo with confirmation
- **WHEN** an administrator clicks the delete button on a photo
- **THEN** a confirmation dialog is displayed
- **AND** upon confirmation, the photo is deleted via DELETE endpoint
- **AND** the photo gallery is refreshed

#### Scenario: Photo management controls hidden for operators
- **WHEN** an operator views the product edit page (if accessible)
- **THEN** photo management controls (delete, reorder, set primary) are hidden
- **AND** photos are displayed in read-only mode

#### Scenario: Drag and drop photo reordering
- **WHEN** an administrator drags a photo to a new position in the gallery
- **THEN** the photo's DisplayOrder is updated via PUT endpoint
- **AND** the gallery reflects the new order

#### Scenario: Set primary photo
- **WHEN** an administrator clicks the "set as primary" button on a photo
- **THEN** the photo is marked as primary via PUT endpoint
- **AND** the primary indicator (badge/star) moves to the selected photo

#### Scenario: Photo management loading states
- **WHEN** a photo operation (delete, reorder, set primary) is in progress
- **THEN** the UI shows a loading indicator on the affected photo
- **AND** prevents additional operations until completion

#### Scenario: Photo management error handling
- **WHEN** a photo operation fails
- **THEN** an error toast notification is displayed
- **AND** the gallery state is reverted or refreshed

