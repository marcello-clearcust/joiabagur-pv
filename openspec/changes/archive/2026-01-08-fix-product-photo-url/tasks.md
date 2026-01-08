# Implementation Tasks

## 1. Backend Fix
- [x] 1.1 Add `IFileStorageService` parameter to `ProductService` constructor
- [x] 1.2 Store `IFileStorageService` as private readonly field
- [x] 1.3 Create `MapPhotoToDtoAsync()` helper method in `ProductService`
- [x] 1.4 Update photo mapping in `MapToDtoAsync()` to populate `Url` field using `_fileStorageService.GetUrlAsync()`
- [x] 1.5 Ensure `ProductId` field is populated in photo DTOs

## 2. Verification
- [x] 2.1 Test GET /api/products/{id} returns photos with populated URLs
- [x] 2.2 Test GET /api/products returns catalog with primaryPhotoUrl populated
- [x] 2.3 Verify photos display in product edit page (restart API to test)
- [x] 2.4 Verify primary photo displays in product catalog cards (restart API to test)
- [x] 2.5 Test with multiple photos to ensure all URLs are generated

## 3. Testing
- [x] 3.1 Code builds successfully with no linter errors
- [x] 3.2 Manual test: Restart API and upload photo to verify it displays immediately
- [x] 3.3 Manual test: Navigate away and return to verify photo persists
- [x] 3.4 Manual test: Verify photo displays in catalog view

## 4. Additional Fixes
- [x] 4.1 Added FileStorage configuration to appsettings.json
- [x] 4.2 Fixed frontend to convert relative photo URLs to absolute URLs
- [x] 4.3 Updated MapToListDtoAsync to populate primary photo URLs with full path
- [x] 4.4 Fixed catalog page to use absolute photo URLs

## 5. Refactoring (No Hardcoded Values)
- [x] 5.1 Created centralized `lib/image-url.ts` utility
- [x] 5.2 Removed hardcoded `http://localhost:5056` from components
- [x] 5.3 Utility reads from `VITE_API_BASE_URL` environment variable
- [x] 5.4 Created `lib/image-url.test.ts` with 10 unit tests (all passing)
- [x] 5.5 Updated components to use centralized utility
- [x] 5.6 Frontend build successful with new implementation
