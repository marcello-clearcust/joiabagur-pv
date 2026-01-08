# Solution Summary: Product Photo URL Fix

## Root Cause Analysis

The issue had **multiple layers**:

### 1. Backend: Missing URL Population (ProductService)
**Problem:** `ProductService.MapToDto()` was mapping photos WITHOUT calling `GetUrlAsync()` to generate URLs.

```csharp
// BEFORE (Broken):
Photos = product.Photos?.Select(p => new ProductPhotoDto
{
    Id = p.Id,
    FileName = p.FileName,  // Only filename, no URL!
    DisplayOrder = p.DisplayOrder,
    IsPrimary = p.IsPrimary
}).ToList()

// AFTER (Fixed):
private async Task<ProductPhotoDto> MapPhotoToDtoAsync(ProductPhoto photo)
{
    var url = await _fileStorageService.GetUrlAsync(photo.FileName, "products");
    return new ProductPhotoDto
    {
        Id = photo.Id,
        ProductId = photo.ProductId,
        FileName = photo.FileName,
        Url = url,  // ✅ URL now populated
        DisplayOrder = photo.DisplayOrder,
        IsPrimary = photo.IsPrimary,
        CreatedAt = photo.CreatedAt,
        UpdatedAt = photo.UpdatedAt
    };
}
```

### 2. Backend: Missing FileStorage Configuration
**Problem:** `appsettings.json` didn't have FileStorage configuration.

```json
// ADDED:
"FileStorage": {
  "LocalPath": "uploads",
  "BaseUrl": "/api/files"
}
```

### 3. Backend: Catalog Missing Photo URLs
**Problem:** `MapToListDto()` only returned `FileName` for primary photos, not the full URL.

```csharp
// BEFORE:
PrimaryPhotoUrl = primaryPhoto?.FileName,  // Just filename

// AFTER:
private async Task<ProductListDto> MapToListDtoAsync(...)
{
    string? primaryPhotoUrl = null;
    if (primaryPhoto != null)
    {
        primaryPhotoUrl = await _fileStorageService.GetUrlAsync(primaryPhoto.FileName, "products");
    }
    return new ProductListDto { PrimaryPhotoUrl = primaryPhotoUrl, ... };
}
```

### 4. Frontend: Relative URLs Don't Work for Images
**Problem:** Backend returns relative URLs like `/api/files/products/photo.jpg`, but HTML `<img>` tags don't use Axios's baseURL - they resolve relative to the current page origin.

- Frontend runs on: `http://localhost:3002`
- API runs on: `http://localhost:5056`
- Image URL `/api/files/products/photo.jpg` resolves to: `http://localhost:3002/api/files/...` ❌

**Solution:** Created centralized `getImageUrl()` utility that reads from `VITE_API_BASE_URL` environment variable.

```typescript
// CREATED: lib/image-url.ts
export function getImageUrl(relativeUrl: string | undefined | null): string | undefined {
  if (!relativeUrl) return undefined;
  
  // If already absolute (cloud storage), use as-is
  if (relativeUrl.startsWith('http://') || relativeUrl.startsWith('https://')) {
    return relativeUrl;
  }
  
  // If relative, convert using configured API base URL from .env
  if (relativeUrl.startsWith('/api/')) {
    const apiBaseUrl = import.meta.env.VITE_API_BASE_URL;
    const baseUrlWithoutApi = apiBaseUrl.replace(/\/api\/?$/, '');
    return `${baseUrlWithoutApi}${relativeUrl}`;
  }
  
  return relativeUrl;
}

// USAGE in components:
import { getImageUrl } from '@/lib/image-url';
<img src={getImageUrl(photo.url)} alt="..." />
```

## Files Modified

### Backend
1. **ProductService.cs**
   - Added `IFileStorageService` dependency injection
   - Created `MapPhotoToDtoAsync()` method to populate photo URLs
   - Updated `MapToDtoAsync()` to use async photo mapping
   - Updated `MapToListDtoAsync()` to generate primary photo URLs
   - Updated all method calls to use async versions

2. **appsettings.json**
   - Added `FileStorage` configuration section

### Frontend
3. **lib/image-url.ts** (NEW)
   - Centralized `getImageUrl()` utility
   - Reads API base URL from `VITE_API_BASE_URL` environment variable
   - Handles both relative (dev) and absolute (production cloud) URLs
   - No hardcoded ports or hosts

4. **product-photo-upload.tsx**
   - Import and use `getImageUrl()` utility
   - Removed inline URL conversion logic

5. **catalog.tsx**
   - Import and use `getImageUrl()` utility  
   - Removed inline URL conversion logic

6. **product.service.ts**
   - Cleaned up debug logging

## Testing Verification

### Database Check ✅
```sql
SELECT "Id", "ProductId", "FileName", "CreatedAt" FROM "ProductPhotos";
-- Result: 2 photos exist with correct filenames
```

### File System Check ✅
```
uploads/products/20260108183523_dfe33579.jpg  (89,781 bytes)
uploads/products/20260108183425_9d9dd8d5.jpg  (36,080 bytes)
```

### API Response Check ✅
```
http://localhost:5056/api/files/products/20260108183425_9d9dd8d5.jpg
Status: 200 OK, Content-Type: image/jpeg
```

### Console Logs ✅
```
📷 ProductPhotoUpload - Received photos: Array(2)
📷 First photo URL: /api/files/products/20260108183425_9d9dd8d5.jpg
📷 First photo URL (absolute): http://localhost:5056/api/files/products/...
```

## Result

✅ **Photos now display correctly in:**
- Product edit page (upload component)
- Product catalog (primary photo in cards)
- All photo management operations (set primary, delete)

## Lessons Learned

1. **Backend DTOs must be complete** - Always populate all fields including computed ones like URLs
2. **Frontend images need absolute URLs** - HTML img tags don't respect Axios baseURL
3. **Configuration matters** - File storage configuration must be present
4. **Test data flow end-to-end** - Check DB → API → Frontend → Browser rendering
5. **Console logging is invaluable** - Helped identify exactly where data was lost

## Future Improvements

Consider:
- Add Vite proxy configuration to avoid absolute URL workaround
- Cache file storage service calls for better performance
- Add CDN support for production deployments
- Consider returning absolute URLs from backend directly
