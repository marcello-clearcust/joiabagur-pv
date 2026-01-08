# Refactoring: Centralized Image URL Utility

## Problem
Initial implementation had hardcoded API URLs in multiple components:

```typescript
// ❌ Problemas:
const API_BASE_URL = 'http://localhost:5056/api';  // Hardcoded!
```

**Issues:**
- ❌ Hardcoded port 5056 - breaks if .env changes
- ❌ Duplicated logic in multiple components
- ❌ Difficult to maintain
- ❌ Not testable

## Solution

### Created Centralized Utility: `lib/image-url.ts`

**Benefits:**
- ✅ Reads from `VITE_API_BASE_URL` environment variable
- ✅ No hardcoded values
- ✅ Single source of truth
- ✅ Reusable across all components
- ✅ Fully unit tested (10 test cases)
- ✅ Works with any port configured in .env
- ✅ Compatible with cloud URLs (production)

### Implementation

```typescript
// lib/image-url.ts
export function getImageUrl(relativeUrl: string | undefined | null): string | undefined {
  if (!relativeUrl) return undefined;
  
  // Cloud URLs (production) - use as-is
  if (relativeUrl.startsWith('http://') || relativeUrl.startsWith('https://')) {
    return relativeUrl;
  }
  
  // Relative URLs (development) - convert to absolute
  if (relativeUrl.startsWith('/api/')) {
    const apiBaseUrl = import.meta.env.VITE_API_BASE_URL;
    const baseUrlWithoutApi = apiBaseUrl.replace(/\/api\/?$/, '');
    return `${baseUrlWithoutApi}${relativeUrl}`;
  }
  
  return relativeUrl;
}
```

### Usage in Components

**Before (Duplicated):**
```typescript
// product-photo-upload.tsx
const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5056/api';
const getPhotoUrl = (relativeUrl: string | undefined) => { ... };
<img src={getPhotoUrl(photo.url)} />

// catalog.tsx
const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5056/api';
const getPhotoUrl = (relativeUrl: string | undefined) => { ... };
<img src={getPhotoUrl(product.primaryPhotoUrl)} />
```

**After (Centralized):**
```typescript
// product-photo-upload.tsx
import { getImageUrl } from '@/lib/image-url';
<img src={getImageUrl(photo.url)} />

// catalog.tsx
import { getImageUrl } from '@/lib/image-url';
<img src={getImageUrl(product.primaryPhotoUrl)} />
```

## Test Coverage

Created `lib/image-url.test.ts` with 10 test cases:

✅ Handles null/undefined/empty  
✅ Returns absolute HTTP URLs as-is  
✅ Returns absolute HTTPS URLs as-is (cloud storage)  
✅ Converts relative API URLs using env config  
✅ Handles trailing slashes in API base URL  
✅ Works with different ports from .env  
✅ Works with production API URLs  
✅ Handles root-relative URLs  

**Test Results:** 10/10 passed ✅

## Configuration Flexibility

Now changing the API port is as simple as updating `.env.development`:

```env
# Change port here - no code changes needed!
VITE_API_BASE_URL=http://localhost:8080/api
```

The image URL utility will automatically use the new configuration.

## Production Compatibility

When backend returns cloud URLs, no code changes needed:

**Development:**
```typescript
// API returns: "/api/files/products/photo.jpg"
getImageUrl("/api/files/products/photo.jpg")
// Returns: "http://localhost:5056/api/files/products/photo.jpg"
```

**Production:**
```typescript
// API returns: "https://bucket.s3.amazonaws.com/products/photo.jpg"
getImageUrl("https://bucket.s3.amazonaws.com/products/photo.jpg")
// Returns: "https://bucket.s3.amazonaws.com/products/photo.jpg" (as-is)
```

## Summary

**Improvements:**
1. ✅ Removed all hardcoded URLs
2. ✅ Centralized in single utility
3. ✅ Fully tested with unit tests
4. ✅ Reads from environment configuration
5. ✅ Works with any port or host
6. ✅ Production-ready for cloud storage

**Files:**
- Created: `lib/image-url.ts` (utility)
- Created: `lib/image-url.test.ts` (tests)
- Updated: `product-photo-upload.tsx` (uses utility)
- Updated: `catalog.tsx` (uses utility)
