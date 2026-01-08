# Bug Investigation: Product Photos Not Displaying

## Issue Summary
Uploaded product photos do not display in the frontend. Users see empty placeholder icons instead of the actual uploaded images.

## Root Cause

### The Problem is in the VIEW, not the UPLOAD

**Upload Flow (✅ Working):**
```
ProductPhotoService.UploadPhotoAsync()
  ↓
MapToDtoAsync()
  ↓
_fileStorageService.GetUrlAsync(photo.FileName, PhotoFolder)
  ↓
ProductPhotoDto with populated Url field
```

**View Flow (❌ Broken):**
```
ProductService.GetProductAsync()
  ↓
Maps product.Photos directly to DTOs
  ↓
ProductPhotoDto WITHOUT Url field (just FileName)
  ↓
Frontend receives photo objects with url: undefined
  ↓
Displays placeholder icon instead of image
```

## Code Evidence

### ProductPhotoService.cs (Lines 214-229) - CORRECT ✅
```csharp
private async Task<ProductPhotoDto> MapToDtoAsync(ProductPhoto photo)
{
    var url = await _fileStorageService.GetUrlAsync(photo.FileName, PhotoFolder);
    
    return new ProductPhotoDto
    {
        Id = photo.Id,
        ProductId = photo.ProductId,
        FileName = photo.FileName,
        Url = url,  // ✅ URL is populated
        DisplayOrder = photo.DisplayOrder,
        IsPrimary = photo.IsPrimary,
        CreatedAt = photo.CreatedAt,
        UpdatedAt = photo.UpdatedAt
    };
}
```

### ProductService.cs (Lines 427-433) - BROKEN ❌
```csharp
Photos = product.Photos?.Select(p => new ProductPhotoDto
{
    Id = p.Id,
    FileName = p.FileName,
    DisplayOrder = p.DisplayOrder,
    IsPrimary = p.IsPrimary
    // ❌ Missing: Url field (not populated)
    // ❌ Missing: ProductId field
    // ❌ Missing: CreatedAt/UpdatedAt fields
}).ToList() ?? new List<ProductPhotoDto>()
```

### Frontend (product-photo-upload.tsx Lines 222-232) - Checking for URL
```tsx
{photo.url ? (
  <img
    src={photo.url}  // ❌ undefined, so this branch never executes
    alt={`Foto ${photo.displayOrder + 1}`}
    className="w-full h-full object-cover"
  />
) : (
  <div className="w-full h-full flex items-center justify-center">
    <ImageIcon className="size-8 text-muted-foreground" />  // ✅ This shows instead
  </div>
)}
```

## The Fix

**Backend:**
1. `ProductService` needs to inject `IFileStorageService` and call `GetUrlAsync()` when mapping photos
2. `ProductRepository.GetAllAsync()` needs `.Include(p => p.Photos)` to load photos from database
3. `appsettings.json` needs FileStorage configuration section

**Frontend:**
4. Components need to convert relative URLs (`/api/files/...`) to absolute URLs (`http://localhost:5056/api/files/...`) because HTML `<img>` tags don't use Axios baseURL

### Affected Endpoints
- `GET /api/products/{id}` - Returns product with photos array
- `GET /api/products` - Returns catalog with primaryPhotoUrl
- `GET /api/products/search` - Returns search results with primaryPhotoUrl

## Impact Assessment

### User Impact
- **Before Fix**: Photos upload successfully but don't display (confusing UX)
- **After Fix**: Photos display immediately after upload (expected behavior)

### Technical Impact
- **Breaking Changes**: None (fixes missing data in existing response)
- **API Contract**: No changes (ProductPhotoDto already has Url field, it's just null)
- **Dependencies**: ProductService gains dependency on IFileStorageService

## Verification Steps
1. Upload a photo to a product
2. Navigate to product edit page
3. **Before fix**: See placeholder icon
4. **After fix**: See actual uploaded image
5. Check product catalog - primary photo should display in cards
