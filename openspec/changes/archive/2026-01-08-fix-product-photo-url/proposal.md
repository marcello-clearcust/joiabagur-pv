# Change: Fix Product Photo URL Not Populated in Product Retrieval

## Why
Uploaded product photos are not displayed in the product edit page and catalog because the `Url` field in `ProductPhotoDto` is not being populated when products are retrieved through the `ProductService`. The frontend shows empty placeholder icons instead of the actual uploaded images.

### Root Cause Analysis

**Upload Flow (Working):**
- `ProductPhotoService.UploadPhotoAsync()` → calls `MapToDtoAsync()` → calls `_fileStorageService.GetUrlAsync()` ✅
- Photos uploaded successfully with URLs populated

**View Flow (Broken):**
- `ProductService.GetProductAsync()` → maps `product.Photos` directly without calling `GetUrlAsync()` ❌
- `ProductPhotoDto.Url` remains null/undefined
- Frontend displays placeholder instead of image

### Evidence
- `ProductService.cs:427-433` - Photos mapped without URL field
- `ProductPhotoService.cs:214-229` - Correct implementation with URL mapping
- `product-photo-upload.tsx:222` - Frontend checks `photo.url` before rendering image

## What Changes
- **ProductService** must populate the `Url` field when mapping `ProductPhoto` entities to DTOs
- Add `IFileStorageService` dependency to `ProductService` to construct photo URLs
- **ProductRepository.GetAllAsync()** must include Photos navigation property (`.Include(p => p.Photos)`)
- Add `FileStorage` configuration to `appsettings.json`
- Frontend must convert relative photo URLs to absolute URLs for cross-origin image loading
- Ensure all product retrieval endpoints return photos with accessible URLs

## Impact
- Affected specs: `product-management`
- Affected code:
  - `backend/src/JoiabagurPV.Application/Services/ProductService.cs` (constructor and photo mapping)
  - `backend/src/JoiabagurPV.Infrastructure/Data/Repositories/ProductRepository.cs` (Include Photos)
  - `backend/src/JoiabagurPV.API/appsettings.json` (FileStorage configuration)
  - `frontend/src/pages/products/components/product-photo-upload.tsx` (absolute URL conversion)
  - `frontend/src/pages/products/catalog.tsx` (absolute URL conversion)
  - No API contract changes (only fixes missing data in existing response)
- **User Impact**: This restores intended behavior - uploaded photos will now display correctly
- **Breaking**: None - this is a bug fix restoring spec compliance
