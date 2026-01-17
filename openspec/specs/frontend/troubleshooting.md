# Frontend Troubleshooting Guide

This document captures common frontend issues encountered during development and their solutions.

---

## Table of Contents

1. [Data Loading Race Conditions](#1-data-loading-race-conditions)
2. [Paginated Data Lookups](#2-paginated-data-lookups)
3. [Empty or Invalid Data from Navigation State](#3-empty-or-invalid-data-from-navigation-state)
4. [Image Recognition Product Mapping](#4-image-recognition-product-mapping)

---

## 1. Data Loading Race Conditions

### Problem

When a page loads data from multiple sources and uses navigation state to pre-select items, the pre-selection may fail if the data isn't loaded yet.

### Root Cause

Navigation state is available immediately, but API data is loaded asynchronously. If code tries to find an item before the data is loaded, it will fail.

### Solution

Handle pre-selection inside the data loading callback, after data is available:

```typescript
useEffect(() => {
  const loadData = async () => {
    try {
      const data = await someService.getData();
      setData(data);
      
      // Pre-select AFTER data is loaded
      if (locationState?.itemId && data) {
        const item = data.find(i => i.id === locationState.itemId);
        if (item) {
          setSelectedItem(item);
        }
      }
    } finally {
      setLoading(false);
    }
  };
  loadData();
}, [locationState?.itemId]);
```

### Prevention

- Always handle pre-selection inside async data loading functions
- Use loading states to prevent UI from rendering prematurely
- Test navigation flows with pre-selected items

---

## 2. Paginated Data Lookups

### Problem

Looking for a specific item in paginated API results may fail if the item is not in the current page.

Example: Stock showing 0 because the product wasn't in the first 50 inventory items.

### Root Cause

```typescript
// PROBLEM: getStock returns paginated data (max 50 items)
const stockData = await inventoryService.getStock(posId);
const productStock = stockData.items.find(
  item => item.productId === productId
); // undefined if product not in first 50!
```

### Solution

Use specific lookup endpoints instead of searching paginated lists:

```typescript
// SOLUTION: Get stock specifically for the product
const stockBreakdown = await inventoryService.getProductStockBreakdown(productId);
const posStock = stockBreakdown?.breakdown.find(
  b => b.pointOfSaleId === posId
);
setAvailableStock(posStock?.quantity ?? 0);
```

### Prevention

- Review API calls to identify paginated endpoints
- Request specific lookup endpoints from backend if not available
- Never assume paginated lists contain all items

---

## 3. Empty or Invalid Data from Navigation State

### Problem

Navigation state may contain empty strings or invalid IDs that cause downstream errors.

Example: Image recognition passes `productId: ''` when enrichment fails, causing product lookup to fail.

### Root Cause

The image recognition enrichment may not find a matching product, leaving `productId` as an empty string:

```typescript
// enrichSuggestionsFromMappings may return empty productId
if (!productMappings[suggestion.productName]) {
  return suggestion; // productId stays as ''
}
```

### Solution

Always validate navigation state values before using them:

```typescript
// Check for truthy AND non-empty value
if (locationState?.productId && locationState.productId.trim() !== '') {
  // Safe to use productId
}
```

### Prevention

- Add console warnings when enrichment fails
- Validate all navigation state values before use
- Consider not passing invalid data in navigation state at all

---

## 4. Image Recognition Product Mapping

### Problem

Image recognition suggestions have empty `productId` because the class label from the ML model doesn't match any current product names.

### Root Cause

1. Model was trained with product names as class labels
2. Products were renamed or deleted after training
3. Enrichment function looks up by exact name match
4. No match found → `productId` stays empty

### Symptoms

- Console shows: `⚠️ Could not find product mapping for class label: "Old Product Name"`
- Suggestions display correctly but clicking them fails
- Error: "El producto seleccionado no está disponible"

### Solution

Add debug logging to identify mapping failures:

```typescript
function enrichSuggestionsFromMappings(
  suggestions: ProductSuggestion[],
  productMappings: Record<string, ProductLabelMapping>
): ProductSuggestion[] {
  console.log('Available product mappings:', Object.keys(productMappings));
  
  return suggestions.map(suggestion => {
    const product = productMappings[suggestion.productName];
    
    if (product) {
      console.log(`✅ Enriched "${suggestion.productName}" -> ${product.productId}`);
      return { ...suggestion, ...product };
    }
    
    console.warn(`⚠️ No mapping for: "${suggestion.productName}"`);
    return suggestion;
  });
}
```

### Resolution

When this happens, the ML model needs to be retrained to include current product names.

### Prevention

- Alert admins when model accuracy drops
- Implement model health monitoring
- Consider fuzzy matching for product name lookups
- Document that product renames require model retraining

---

## Common Patterns

### Fallback Pattern for Pre-Selected Items

Always implement a fallback API lookup when pre-selecting items from navigation state:

```typescript
// 1. First try to find in already-loaded data
let item = loadedData.find(d => d.id === stateId);

// 2. Fallback: fetch directly from API
if (!item && stateId) {
  try {
    item = await service.getById(stateId);
    // Optionally add to local list
    if (item) {
      setLoadedData(prev => [...prev, item]);
    }
  } catch (error) {
    console.warn('Direct lookup failed:', error);
  }
}

// 3. Only show error if both methods fail
if (!item) {
  toast.warning('Item not available');
}
```

### Validation Pattern for Navigation State

```typescript
interface LocationState {
  itemId?: string;
  someFlag?: boolean;
}

const locationState = location.state as LocationState | null;

// Safe access pattern
const hasValidItemId = locationState?.itemId && 
                        locationState.itemId.trim() !== '';
```

---

## Related Documentation

- [Image Recognition Spec](../image-recognition/spec.md)
- [Sales Management Spec](../sales-management/spec.md)
- [Frontend Testing Spec](../frontend-testing/spec.md)
