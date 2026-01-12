## ADDED Requirements

### Requirement: Product Entity Management
The system SHALL maintain Product entities with unique SKU identifiers, name, description, price, optional collection assignment, and active status.

#### Scenario: Create product with valid data
- **WHEN** a product is created with SKU "JOY-001", Name "Gold Ring", Price 299.99
- **THEN** the product is stored with a unique ID, IsActive = true, and audit timestamps

#### Scenario: SKU uniqueness validation
- **WHEN** attempting to create a product with an existing SKU "JOY-001"
- **THEN** the system rejects the creation with a validation error

#### Scenario: Price validation
- **WHEN** attempting to create a product with Price <= 0
- **THEN** the system rejects the creation with a validation error

### Requirement: Product Photo Management
The system SHALL support multiple photos per product with display ordering and primary photo designation.

#### Scenario: Associate photo with product
- **WHEN** a photo is uploaded for product "JOY-001"
- **THEN** the photo is stored with DisplayOrder and linked to the product

#### Scenario: Primary photo designation
- **WHEN** a photo is marked as primary for a product
- **THEN** any existing primary photo for that product is unmarked
- **AND** the new photo becomes the primary photo

#### Scenario: Multiple photos per product
- **WHEN** multiple photos are uploaded for a product
- **THEN** each photo has a unique DisplayOrder for sorting

### Requirement: Collection Management
The system SHALL support product categorization through Collections with name and description.

#### Scenario: Create collection
- **WHEN** a collection is created with Name "Summer 2024"
- **THEN** the collection is stored and available for product assignment

#### Scenario: Assign product to collection
- **WHEN** a product is assigned to collection "Summer 2024"
- **THEN** the product's CollectionId references the collection

### Requirement: Excel Product Import
The system SHALL support bulk product import from Excel files (.xlsx, .xls) with validation, UPSERT logic, and atomic transactions.

#### Scenario: Import new products successfully
- **WHEN** an administrator uploads an Excel file with products having new SKUs
- **AND** all data is valid (SKU, Name required; Price > 0)
- **THEN** all products are created with IsActive = true
- **AND** the import result shows the count of created products

#### Scenario: Update existing products
- **WHEN** an administrator imports products with SKUs that already exist
- **THEN** the existing products are updated with new Name, Description, Price, Collection values
- **AND** the product ID and associated photos are preserved
- **AND** UpdatedAt timestamp is refreshed

#### Scenario: Mixed create and update
- **WHEN** an Excel file contains both new and existing SKUs
- **THEN** new products are created and existing products are updated
- **AND** the import result shows separate counts for created and updated

#### Scenario: Validation errors before import
- **WHEN** an Excel file contains validation errors (empty SKU, invalid price, missing required fields)
- **THEN** the system returns a list of errors with row numbers
- **AND** no products are imported until errors are corrected

#### Scenario: Duplicate SKU detection in file
- **WHEN** an Excel file contains the same SKU in multiple rows
- **THEN** the system detects the duplicate
- **AND** returns an error "SKU duplicado en el archivo: [SKU]"
- **AND** prevents import until duplicates are removed

#### Scenario: Automatic collection creation
- **WHEN** an Excel file references a collection name that does not exist
- **THEN** the system automatically creates the collection
- **AND** assigns the product to the newly created collection

#### Scenario: Transaction atomicity
- **WHEN** an error occurs during import processing
- **THEN** all changes are rolled back
- **AND** no partial data is persisted

### Requirement: Product Import API
The system SHALL provide REST API endpoints for Excel template download and file upload with proper authentication and file validation.

#### Scenario: Download Excel template as administrator
- **WHEN** an authenticated administrator GETs /api/products/import-template
- **THEN** the system returns an Excel file with exact column headers and example data
- **AND** file has appropriate content-type header for Excel download

#### Scenario: Upload Excel file as administrator
- **WHEN** an authenticated administrator POSTs to /api/products/import with a valid Excel file
- **THEN** the system validates all rows before processing any
- **AND** shows all validation errors before import confirmation
- **AND** processes valid rows only after user confirmation
- **AND** returns ImportResult with success/error details

#### Scenario: Unauthorized import attempt
- **WHEN** an operator or unauthenticated user attempts to POST to /api/products/import
- **THEN** the request is rejected with 401 Unauthorized or 403 Forbidden

#### Scenario: Invalid file format
- **WHEN** a file with format other than .xlsx or .xls is uploaded
- **THEN** the request is rejected with 400 Bad Request and error message

#### Scenario: File size limit exceeded
- **WHEN** a file larger than 10MB is uploaded
- **THEN** the request is rejected with 413 Payload Too Large

### Requirement: File Storage Service
The system SHALL provide an abstraction for file storage that supports local development and cloud production environments.

#### Scenario: Store file locally in development
- **WHEN** running in development environment
- **AND** a file is uploaded
- **THEN** the file is stored in the local filesystem under ./uploads/

#### Scenario: Store file in cloud in production
- **WHEN** running in production environment
- **AND** a file is uploaded
- **THEN** the file is stored in the configured cloud storage (S3/Azure Blob)

#### Scenario: Generate unique filenames
- **WHEN** a file is uploaded
- **THEN** a unique filename is generated to prevent conflicts

#### Scenario: File format validation
- **WHEN** a file upload is attempted
- **THEN** only allowed formats (JPG, PNG for photos; XLSX, XLS for Excel) are accepted

### Requirement: Product Import Frontend
The system SHALL provide a user-friendly interface for bulk product import with preview, validation feedback, and results display.

#### Scenario: Drag and drop file upload
- **WHEN** an administrator navigates to the product import page
- **THEN** a drag-and-drop zone is displayed for Excel file selection

#### Scenario: Download Excel template
- **WHEN** an administrator requests product import template
- **THEN** system provides downloadable Excel template file via GET /api/products/import-template endpoint
- **AND** template contains exact column headers: "SKU", "Name", "Description", "Price", "Collection" (header row protected/locked)
- **AND** template includes example row with sample data
- **AND** template includes data validation rules (Price > 0, SKU text format, Name required)
- **AND** template includes instructions or comments explaining format requirements
- **AND** template has proper formatting (bold headers, number format for Price column, text format for SKU)

#### Scenario: Template accessible from help documentation
- **WHEN** administrator accesses help or documentation pages
- **THEN** template download link is available
- **AND** link provides same template as import page

#### Scenario: Preview before import
- **WHEN** a valid Excel file is selected (before upload)
- **THEN** the system shows a preview with row count and basic validation status
- **AND** displays preview table showing first few rows that will be imported

#### Scenario: Display validation errors
- **WHEN** the uploaded file has validation errors
- **THEN** errors are displayed with detailed format: row number, field name, and error message
- **AND** errors are grouped by type (missing columns, invalid data, business rule violations)
- **AND** example error format: "Row 5: SKU 'ABC-123' is duplicate" or "Row 8: Price must be greater than 0"
- **AND** the import button is disabled until errors are resolved

#### Scenario: Confirm import action with summary
- **WHEN** the file is valid and the administrator clicks import
- **THEN** a confirmation dialog is shown before processing
- **AND** dialog displays summary: total rows to process, breakdown (e.g., "X products will be created, Y products will be updated")
- **AND** requires explicit confirmation before processing

#### Scenario: Display import progress
- **WHEN** an import is in progress
- **THEN** a progress indicator is shown to the user
- **AND** import button is disabled during processing

#### Scenario: Display import results
- **WHEN** an import completes
- **THEN** a summary is displayed showing created count, updated count, and any warnings

### Requirement: Product Update API
The system SHALL provide a REST API endpoint for updating existing products with SKU immutability enforcement.

#### Scenario: Update product successfully
- **WHEN** an authenticated administrator PUTs to /api/products/{id} with valid data (Name, Description, Price, CollectionId)
- **THEN** the product is updated with the new values
- **AND** UpdatedAt timestamp is refreshed
- **AND** the updated product is returned

#### Scenario: SKU immutability enforcement
- **WHEN** an administrator attempts to update a product's SKU
- **THEN** the system rejects the request with 400 Bad Request
- **AND** returns error message "SKU cannot be modified"

#### Scenario: Update with invalid price
- **WHEN** an administrator attempts to update a product with Price <= 0
- **THEN** the system rejects the request with 400 Bad Request
- **AND** returns validation error

#### Scenario: Update non-existent product
- **WHEN** an administrator attempts to PUT to /api/products/{id} with non-existent ID
- **THEN** the system returns 404 Not Found

#### Scenario: Unauthorized update attempt
- **WHEN** an operator or unauthenticated user attempts to PUT to /api/products/{id}
- **THEN** the request is rejected with 401 Unauthorized or 403 Forbidden

### Requirement: Product Update Frontend
The system SHALL provide a user interface for editing existing products with pre-populated form fields and SKU immutability.

#### Scenario: Load product for editing
- **WHEN** an administrator navigates to the product edit page for an existing product
- **THEN** the form is pre-populated with current product data (SKU, Name, Description, Price, Collection)

#### Scenario: SKU field disabled
- **WHEN** the product edit form is displayed
- **THEN** the SKU field is disabled and visually indicated as non-editable
- **AND** the SKU value is displayed but cannot be modified

#### Scenario: Update product successfully
- **WHEN** an administrator modifies editable fields and submits the form
- **THEN** the product is updated via PUT /api/products/{id}
- **AND** a success message is displayed
- **AND** the user is redirected to the product list or detail page

#### Scenario: Validation errors on update
- **WHEN** an administrator submits invalid data (empty name, negative price)
- **THEN** validation errors are displayed inline
- **AND** the form is not submitted until errors are corrected

#### Scenario: Product not found
- **WHEN** an administrator attempts to edit a non-existent product
- **THEN** a "Product not found" message is displayed
- **AND** the user is redirected to the product list

### Requirement: Product Photo Upload API
The system SHALL provide a REST API endpoint for uploading product photos with format validation and storage.

#### Scenario: Upload photo successfully
- **WHEN** an authenticated administrator POSTs to /api/products/{id}/photos with a valid JPG or PNG file
- **THEN** the photo is stored using IFileStorageService
- **AND** a ProductPhoto record is created with DisplayOrder
- **AND** if no primary photo exists, IsPrimary is set to true
- **AND** the photo information is returned

#### Scenario: Upload multiple photos
- **WHEN** an administrator uploads multiple photos for a product
- **THEN** each photo is stored with an incremented DisplayOrder
- **AND** the photos are returned in display order

#### Scenario: Invalid photo format
- **WHEN** an administrator attempts to upload a file that is not JPG or PNG
- **THEN** the request is rejected with 400 Bad Request
- **AND** returns error message "Only JPG and PNG formats are allowed"

#### Scenario: Photo size limit exceeded
- **WHEN** an administrator attempts to upload a photo larger than 5MB
- **THEN** the request is rejected with 413 Payload Too Large

#### Scenario: Upload photo to non-existent product
- **WHEN** an administrator attempts to POST to /api/products/{id}/photos with non-existent product ID
- **THEN** the system returns 404 Not Found

#### Scenario: Unauthorized photo upload
- **WHEN** an operator or unauthenticated user attempts to upload a photo
- **THEN** the request is rejected with 401 Unauthorized or 403 Forbidden

### Requirement: Product Photo Management Service
The system SHALL provide business logic for photo management including primary designation and display ordering.

#### Scenario: Automatic primary designation
- **WHEN** the first photo is uploaded for a product
- **THEN** IsPrimary is automatically set to true

#### Scenario: Primary photo designation
- **WHEN** a photo is marked as primary
- **THEN** any existing primary photo for that product has IsPrimary set to false
- **AND** the selected photo has IsPrimary set to true

#### Scenario: Display order management
- **WHEN** photos are uploaded
- **THEN** each photo receives a unique DisplayOrder value
- **AND** DisplayOrder is used for sorting in photo galleries

#### Scenario: Photo deletion
- **WHEN** a photo is deleted
- **THEN** the file is removed from storage via IFileStorageService
- **AND** the ProductPhoto record is removed from the database
- **AND** if it was the primary photo, another photo is automatically designated as primary

### Requirement: Product Photo Upload Frontend
The system SHALL provide a user interface for uploading product photos with drag-and-drop, preview, and validation.

#### Scenario: Photo upload component
- **WHEN** an administrator is on the product edit page
- **THEN** a photo upload component is displayed with drag-and-drop zone

#### Scenario: Drag and drop photo upload
- **WHEN** an administrator drags and drops a JPG or PNG file
- **THEN** the file is validated and uploaded via POST /api/products/{id}/photos
- **AND** a preview thumbnail is displayed

#### Scenario: Multiple photo upload
- **WHEN** an administrator uploads multiple photos
- **THEN** all photos are uploaded and displayed in the gallery
- **AND** photos are shown in DisplayOrder

#### Scenario: Photo format validation
- **WHEN** an administrator attempts to upload an invalid format
- **THEN** an error message is displayed
- **AND** the upload is prevented

#### Scenario: Primary photo indication
- **WHEN** photos are displayed in the gallery
- **THEN** the primary photo is visually indicated (e.g., with a badge or border)

#### Scenario: Photo upload progress
- **WHEN** a photo is being uploaded
- **THEN** a progress indicator is shown
- **AND** the upload can be cancelled if needed

