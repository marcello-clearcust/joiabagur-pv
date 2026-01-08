## 1. Domain Layer

- [x] 1.1 Create Product entity with SKU, Name, Description, Price, CollectionId, IsActive, and BaseEntity inheritance (T-EP1-001-001)
- [x] 1.2 Create ProductPhoto entity with ProductId, FileName, DisplayOrder, IsPrimary (T-EP1-001-001)
- [x] 1.3 Create Collection entity with Name, Description (T-EP1-001-001)
- [x] 1.4 Add domain validations (price > 0, SKU format) (T-EP1-001-001)
- [x] 1.5 Create IProductRepository interface with CRUD + FindBySku, GetWithPhotos, GetByCollection (T-EP1-001-002)
- [x] 1.6 Create IProductPhotoRepository interface with GetByProductId, UpdateDisplayOrder (T-EP1-001-002)
- [x] 1.7 Create ICollectionRepository interface with CRUD + FindByName (T-EP1-001-002)

## 2. Infrastructure Layer - Database

- [x] 2.1 Create ProductConfiguration for EF Core entity mapping (T-EP1-001-003)
- [x] 2.2 Create ProductPhotoConfiguration with FK relationships (T-EP1-001-003)
- [x] 2.3 Create CollectionConfiguration (T-EP1-001-003)
- [x] 2.4 Add DbSets to AppDbContext (T-EP1-001-003)
- [x] 2.5 Create and apply database migration for Product entities (T-EP1-001-003)

## 3. Infrastructure Layer - Repositories

- [x] 3.1 Implement ProductRepository with EF Core queries (T-EP1-001-004)
- [x] 3.2 Implement ProductPhotoRepository with photo-specific operations (T-EP1-001-004)
- [x] 3.3 Implement CollectionRepository (T-EP1-001-004)
- [x] 3.4 Register repositories in DI container (T-EP1-001-004)

## 4. Infrastructure Layer - File Storage

- [x] 4.1 Create IFileStorageService interface with UploadAsync, DownloadAsync, DeleteAsync, GetUrlAsync (T-EP1-001-005)
- [x] 4.2 Implement LocalFileStorageService for development (T-EP1-001-005)
- [x] 4.3 Implement CloudFileStorageService stub for future AWS S3/Azure Blob (T-EP1-001-005)
- [x] 4.4 Add configuration-based storage provider selection (T-EP1-001-005)
- [x] 4.5 Add file validation (format, size limits) (T-EP1-001-005)

## 4A. Application Layer - Shared Excel Template Service

- [x] 4A.1 Create IExcelTemplateService interface
- [x] 4A.2 Implement ExcelTemplateService using ClosedXML
- [x] 4A.3 Implement template generation with common formatting (bold headers, protected header row)
- [x] 4A.4 Implement data validation rules support (number ranges, text formats)
- [x] 4A.5 Implement example row generation
- [x] 4A.6 Implement instructions/comments support

## 5. Application Layer - Services

- [x] 5.1 Create IExcelImportService interface (T-EP1-001-006)
- [x] 5.2 Implement ExcelImportService with ClosedXML parsing (T-EP1-001-006)
- [x] 5.3 Add Excel validation (required columns with exact name matching, data formats, duplicate SKUs) (T-EP1-001-006)
- [x] 5.4 Implement UPSERT logic (create new, update existing by SKU) (T-EP1-001-006)
- [x] 5.5 Add automatic collection creation during import (T-EP1-001-006)
- [x] 5.6 Implement transaction handling for atomicity (T-EP1-001-006)
- [x] 5.7 Create ImportResult DTO with success/error counts and details (T-EP1-001-006)
- [x] 5.8 Implement GenerateTemplate method using shared IExcelTemplateService (create Excel template with exact column headers, example row, instructions/comments, data validation rules, protected header row, formatting)
- [x] 5.9 Update error reporting to include detailed format (row number, field name, error message, grouped by type)
- [x] 5.10 Create IProductService interface with CRUD operations (T-EP1-001-007)
- [x] 5.11 Implement ProductService with business validations (T-EP1-001-007)
- [x] 5.12 Add SKU uniqueness validation (T-EP1-001-007)
- [x] 5.13 Add soft delete support with IsActive flag (T-EP1-001-007)

## 6. API Layer

- [x] 6.1 Create ProductsController with route /api/products (T-EP1-001-008)
- [x] 6.2 Implement GET /api/products/import-template endpoint (download Excel template with proper content-type)
- [x] 6.3 Implement POST /api/products/import endpoint (T-EP1-001-008)
- [x] 6.4 Add file upload handling with multipart/form-data (T-EP1-001-008)
- [x] 6.5 Add file validation middleware (size: 10MB, format: .xlsx/.xls) (T-EP1-001-008)
- [x] 6.6 Add [Authorize(Roles = "Administrator")] authorization (T-EP1-001-008)
- [x] 6.7 Add Swagger/Scalar documentation for endpoints (T-EP1-001-008)

## 7. Backend Testing

- [x] 7.1 Unit tests for domain entity validations (T-EP1-001-009)
- [x] 7.2 Unit tests for ExcelImportService parsing and validation (T-EP1-001-009)
- [x] 7.3 Unit tests for UPSERT logic (create vs update) (T-EP1-001-009)
- [x] 7.4 Unit tests for duplicate SKU detection (T-EP1-001-009)
- [x] 7.5 Unit tests for automatic collection creation (T-EP1-001-009)
- [x] 7.6 Unit tests for transaction rollback scenarios (T-EP1-001-009)
- [ ] 7.7 Integration tests for POST /api/products/import endpoint (T-EP1-001-010) - Deferred to integration test phase
- [ ] 7.8 Integration tests for authentication/authorization (T-EP1-001-010) - Deferred to integration test phase
- [ ] 7.9 Integration tests with real Excel files and Testcontainers (T-EP1-001-010) - Deferred to integration test phase

## 8. Frontend - Import Page

- [x] 8.1 Create ProductImportPage component with Metronic layout (T-EP1-001-011)
- [x] 8.2 Add drag-and-drop file upload component (T-EP1-001-011)
- [x] 8.3 Add Excel template download button (calls GET /api/products/import-template, visible before file selection) (T-EP1-001-011)
- [x] 8.4 Add preview functionality (show preview table with first few rows when file selected, before upload)
- [x] 8.5 Add validation error display with detailed format (row number, field name, error message, grouped by type) (T-EP1-001-011)
- [x] 8.6 Update import confirmation dialog to show summary (total rows, breakdown: "X products will be created, Y products will be updated")
- [x] 8.7 Add progress indicator during import (T-EP1-001-011)
- [x] 8.8 Add results summary display (created, updated, errors) (T-EP1-001-011)
- [x] 8.9 Add route and navigation menu entry for import page (T-EP1-001-011)
- [ ] 8.10 Add template download link to help/documentation pages

## 9. Frontend Testing

- [x] 9.1 Component tests for ProductImportPage (T-EP1-001-012)
- [x] 9.2 Tests for file upload interactions (T-EP1-001-012)
- [x] 9.3 Tests for validation error display (T-EP1-001-012)
- [x] 9.4 MSW mocks for import API endpoint (T-EP1-001-012)
- [x] 9.5 Tests for progress and results display (T-EP1-001-012)

## 10. Product Update - Backend

- [x] 10.1 Add UpdateProduct method to IProductService interface (T-EP1-003-001)
- [x] 10.2 Implement UpdateProduct in ProductService with SKU immutability validation (T-EP1-003-001)
- [x] 10.3 Add PUT /api/products/{id} endpoint in ProductsController (T-EP1-003-001)
- [x] 10.4 Add UpdateProductDto with validation attributes (T-EP1-003-001)
- [x] 10.5 Enforce SKU immutability (reject if SKU is included in update payload) (T-EP1-003-001)
- [x] 10.6 Add authorization [Authorize(Roles = "Administrator")] (T-EP1-003-001)
- [x] 10.7 Return updated product with 200 OK or appropriate error codes (T-EP1-003-001)

## 11. Product Update - Frontend

- [x] 11.1 Create ProductEditPage component with Metronic layout (T-EP1-003-002)
- [x] 11.2 Load existing product data on page mount (T-EP1-003-002)
- [x] 11.3 Pre-populate form fields with product data (T-EP1-003-002)
- [x] 11.4 Disable SKU field with visual indication (readonly, grayed out) (T-EP1-003-002)
- [x] 11.5 Implement form validation with React Hook Form + Zod (T-EP1-003-002)
- [x] 11.6 Add update button with loading state (T-EP1-003-002)
- [x] 11.7 Handle success with toast notification and navigation (T-EP1-003-002)
- [x] 11.8 Handle 404 Not Found with redirect to product list (T-EP1-003-002)
- [x] 11.9 Add route for /products/:id/edit (T-EP1-003-002)

## 12. Product Update - Testing

- [ ] 12.1 Integration tests for PUT /api/products/{id} endpoint (T-EP1-003-003)
- [ ] 12.2 Test successful product update (T-EP1-003-003)
- [ ] 12.3 Test SKU immutability enforcement (T-EP1-003-003)
- [ ] 12.4 Test validation errors (invalid price, missing fields) (T-EP1-003-003)
- [ ] 12.5 Test 404 for non-existent product (T-EP1-003-003)
- [ ] 12.6 Test authentication and authorization (T-EP1-003-003)
- [ ] 12.7 Component tests for ProductEditPage (T-EP1-003-004)
- [ ] 12.8 Test form pre-population (T-EP1-003-004)
- [ ] 12.9 Test SKU field disabled state (T-EP1-003-004)
- [ ] 12.10 Test form validation and submission (T-EP1-003-004)
- [ ] 12.11 MSW mocks for update endpoint (T-EP1-003-004)

## 13. Photo Upload - Backend Service

- [x] 13.1 Create IProductPhotoService interface (T-EP1-004-002)
- [x] 13.2 Implement ProductPhotoService with photo upload logic (T-EP1-004-002)
- [x] 13.3 Add automatic primary designation for first photo (T-EP1-004-002)
- [x] 13.4 Add primary photo management (unmark existing, mark new) (T-EP1-004-002)
- [x] 13.5 Add display order management (auto-increment) (T-EP1-004-002)
- [x] 13.6 Add photo deletion with file cleanup (T-EP1-004-002)
- [x] 13.7 Register ProductPhotoService in DI container (T-EP1-004-002)

## 14. Photo Upload - Backend API

- [x] 14.1 Add POST /api/products/{id}/photos endpoint (T-EP1-004-001)
- [x] 14.2 Add multipart/form-data file upload handling (T-EP1-004-001)
- [x] 14.3 Add file format validation (JPG, PNG only) (T-EP1-004-001)
- [x] 14.4 Add file size validation (max 5MB) (T-EP1-004-001)
- [x] 14.5 Generate unique filename to prevent conflicts (T-EP1-004-001)
- [x] 14.6 Store file using IFileStorageService (T-EP1-004-001)
- [x] 14.7 Create ProductPhoto record via ProductPhotoService (T-EP1-004-001)
- [x] 14.8 Add authorization [Authorize(Roles = "Administrator")] (T-EP1-004-001)
- [x] 14.9 Return photo information with 201 Created (T-EP1-004-001)

## 15. Photo Upload - Frontend Component

- [x] 15.1 Create ProductPhotoUpload component (T-EP1-004-003)
- [x] 15.2 Add drag-and-drop zone using Metronic file-upload component (T-EP1-004-003)
- [x] 15.3 Add file format validation (client-side) (T-EP1-004-003)
- [x] 15.4 Add file size validation (client-side) (T-EP1-004-003)
- [x] 15.5 Add photo preview thumbnails (T-EP1-004-003)
- [x] 15.6 Add upload progress indicator (T-EP1-004-003)
- [x] 15.7 Add primary photo badge/indicator (T-EP1-004-003)
- [x] 15.8 Integrate component into ProductEditPage (T-EP1-004-003)
- [x] 15.9 Add photo gallery display with DisplayOrder sorting (T-EP1-004-003)

## 16. Photo Upload - Testing

- [ ] 16.1 Integration tests for POST /api/products/{id}/photos (deferred)
- [ ] 16.2 Test successful photo upload (deferred)
- [ ] 16.3 Test format validation (reject non-JPG/PNG) (deferred)
- [ ] 16.4 Test size validation (reject > 5MB) (deferred)
- [ ] 16.5 Test automatic primary designation (deferred)
- [ ] 16.6 Test 404 for non-existent product (deferred)
- [ ] 16.7 Component tests for ProductPhotoUpload (deferred)
- [ ] 16.8 Test drag-and-drop interactions (deferred)
- [ ] 16.9 Test file validation feedback (deferred)
- [ ] 16.10 MSW mocks for photo upload endpoint (deferred)

