# Change: Add Product Management with Excel Import, Update, and Photo Upload

## Why

The system needs comprehensive product catalog management as the foundation for inventory, sales, and AI image recognition. Administrators require:
1. Bulk product import via Excel files to efficiently populate and update the catalog
2. Individual product update capability with SKU immutability enforcement
3. Photo upload functionality for product reference images used in AI recognition

## What Changes

- **Domain Layer**: Add Product, ProductPhoto, and Collection entities with proper relationships and validations
- **Infrastructure Layer**: Add database migrations, EF Core configurations, and repository implementations
- **Infrastructure Layer**: Add IFileStorageService abstraction with local/cloud implementations for product photos
- **Application Layer**: Add ExcelImportService for bulk product processing with validation and transactions
- **Application Layer**: Add ProductService with full CRUD operations and business rules
- **Application Layer**: Add ProductPhotoService for photo management with primary designation logic
- **API Layer**: Add ProductsController with endpoints:
  - GET /api/products/import-template (download Excel template)
  - POST /api/products/import (Excel bulk import with validation)
  - PUT /api/products/{id} (update existing product)
  - POST /api/products/{id}/photos (upload product photos)
- **Frontend**: Add product management pages:
  - Product import page with drag-and-drop file upload
  - Product edit form with pre-populated data and disabled SKU field
  - Photo upload component with drag-and-drop and preview

## Impact

- **Affected specs**: New `product-management` capability
- **Affected code**:
  - `backend/src/JoiabagurPV.Domain/Entities/` - new entities
  - `backend/src/JoiabagurPV.Domain/Interfaces/Repositories/` - new repository interfaces
  - `backend/src/JoiabagurPV.Infrastructure/Data/` - migrations and configurations
  - `backend/src/JoiabagurPV.Infrastructure/Repositories/` - repository implementations
  - `backend/src/JoiabagurPV.Infrastructure/Services/` - file storage service
  - `backend/src/JoiabagurPV.Application/Services/` - Excel import, product, and photo services
  - `backend/src/JoiabagurPV.API/Controllers/` - ProductsController with update and photo endpoints
  - `frontend/src/pages/products/` - import, edit pages, and photo components
  - `frontend/src/services/` - product and photo services
- **Dependencies**: Requires completed auth/user management (EP7) for authorization

