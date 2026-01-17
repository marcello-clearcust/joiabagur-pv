# Backend Troubleshooting Guide

This document captures common issues encountered during development and their solutions to help avoid similar problems in the future.

---

## Table of Contents

1. [PostgreSQL DateTime Kind Issues](#1-postgresql-datetime-kind-issues)
2. [Role-Based Access Control (RBAC) for Administrators](#2-role-based-access-control-rbac-for-administrators)
3. [Paginated API Endpoints and Data Lookups](#3-paginated-api-endpoints-and-data-lookups)
4. [Validation Endpoints Should Return Preview Data](#4-validation-endpoints-should-return-preview-data)
5. [Product Lookup from External Sources](#5-product-lookup-from-external-sources)

---

## 1. PostgreSQL DateTime Kind Issues

### Problem

When querying PostgreSQL with Entity Framework Core and Npgsql, DateTime values with `Kind=Unspecified` cause errors:

```
Cannot write DateTime with Kind=Unspecified to PostgreSQL timestamp with time zone
```

This typically happens when:
- Parsing dates from query strings (e.g., `?startDate=2025-12-18`)
- Dates without timezone info from frontend

### Root Cause

PostgreSQL's `timestamp with time zone` columns require DateTime values with explicit timezone information. When .NET parses a date string like `2025-12-18`, it creates a DateTime with `Kind=Unspecified`.

### Solution

Always convert DateTime values to UTC before using them in database queries:

```csharp
// In controller, before passing to service:
DateTime? startDateUtc = startDate.HasValue 
    ? DateTime.SpecifyKind(startDate.Value, DateTimeKind.Utc) 
    : null;

// For end dates, include the full day:
DateTime? endDateUtc = endDate.HasValue 
    ? DateTime.SpecifyKind(endDate.Value.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc) 
    : null;
```

### Affected Endpoints

- `GET /api/sales` - Sales history
- `GET /api/inventory/movements` - Inventory movement history
- Any endpoint accepting date range filters

### Prevention

- Create a helper method for date conversion
- Document date handling in API specifications
- Add integration tests with date filtering

---

## 2. Role-Based Access Control (RBAC) for Administrators

### Problem

Administrators were blocked from performing operations because the code always validated user-specific assignments (e.g., point of sale assignments).

Example error: `"Operator is not assigned to this point of sale"`

### Root Cause

Service methods that validate user permissions didn't receive or check the `isAdmin` flag, treating all users as operators.

### Solution

1. **Add `isAdmin` parameter to service methods:**

```csharp
// Interface
Task<Result> CreateSaleAsync(CreateSaleRequest request, Guid userId, bool isAdmin);

// Implementation
public async Task<Result> CreateSaleAsync(CreateSaleRequest request, Guid userId, bool isAdmin)
{
    // Skip permission checks for admins
    if (!isAdmin)
    {
        var isAssigned = await _userPointOfSaleRepository
            .GetAll()
            .AnyAsync(ups => ups.UserId == userId && 
                            ups.PointOfSaleId == request.PointOfSaleId &&
                            ups.IsActive);

        if (!isAssigned)
        {
            return Result.Failure("Operator is not assigned to this point of sale.");
        }
    }
    
    // Continue with operation...
}
```

2. **Pass `isAdmin` from controller:**

```csharp
var result = await _salesService.CreateSaleAsync(
    request, 
    userId, 
    _currentUserService.IsAdmin);
```

### Pattern to Follow

When implementing role-based filtering:
- **Admins**: Virtual access to all resources
- **Operators**: Access only to assigned resources

Check existing implementations for reference:
- `SalesService.GetSaleByIdAsync` - Correct pattern
- `SalesService.GetSalesHistoryAsync` - Correct pattern
- `PointOfSalesController.GetById` - Correct pattern

### Prevention

- Always consider admin vs operator access when creating new endpoints
- Review all permission checks to ensure admin bypass is implemented
- Add tests for both admin and operator access scenarios

---

## 3. Paginated API Endpoints and Data Lookups

### Problem

When looking up a specific item (e.g., product stock), using a paginated endpoint might not return the item if it's not in the first page of results.

Example: Stock showing as 0 when the product has stock, because the product wasn't in the first 50 items returned.

### Root Cause

Frontend code fetched paginated data and searched for a specific item:

```typescript
// WRONG: Product might not be in first 50 items
const stockData = await inventoryService.getStock(pointOfSaleId); // pageSize=50
const productStock = stockData.items.find(
  (item) => item.productId === selectedProduct.id
);
```

### Solution

Use specific lookup endpoints instead of paginated lists:

```typescript
// CORRECT: Get stock specifically for the product
const stockBreakdown = await inventoryService.getProductStockBreakdown(productId);
const posStock = stockBreakdown?.breakdown.find(
  (b) => b.pointOfSaleId === selectedPosId
);
```

### Prevention

- Create specific lookup endpoints for common use cases
- Don't rely on paginated endpoints for single-item lookups
- Consider adding search/filter parameters to paginated endpoints
- Document when to use paginated vs specific endpoints

---

## 4. Validation Endpoints Should Return Preview Data

### Problem

A validation endpoint (`POST /products/import/validate`) only validated data but didn't return counts of what would be created/updated, making the confirmation dialog useless.

### Root Cause

The `ValidateAsync` method validated file structure and row data but didn't query the database to calculate `createdCount` and `updatedCount`.

### Solution

Validation endpoints should return a preview of what the operation will do:

```csharp
public async Task<ImportResult> ValidateAsync(Stream stream)
{
    // ... validation logic ...

    // If validation passed, calculate counts
    if (!result.Errors.Any() && rowsData.Count > 0)
    {
        var skus = rowsData.Select(r => r.Sku).ToList();
        var existingProducts = await _productRepository.GetBySkusAsync(skus);
        
        result.CreatedCount = rowsData.Count(r => !existingProducts.ContainsKey(r.Sku));
        result.UpdatedCount = rowsData.Count(r => existingProducts.ContainsKey(r.Sku));
    }

    return result;
}
```

### Prevention

- Validation endpoints should return the same statistics as the actual operation
- Write tests that verify validation response includes counts
- Document expected response fields in API specs

---

## 5. Product Lookup from External Sources

### Problem

When navigating from one page to another with a pre-selected item (e.g., product from image recognition), the target page failed to find the item in its locally-loaded list.

Error: `"El producto seleccionado no está disponible en tus puntos de venta"`

### Root Cause

The target page loaded products from an API (which might be slow or incomplete), then searched the local list. If the item wasn't found, it showed an error without attempting a direct lookup.

### Solution

Always try a direct API lookup as fallback:

```typescript
useEffect(() => {
  const loadData = async () => {
    const productsData = await productService.getProducts();
    
    if (locationState?.productId) {
      // First try local list
      let product = productsData.find(p => p.id === locationState.productId);
      
      // Fallback: fetch directly by ID
      if (!product) {
        try {
          product = await productService.getProduct(locationState.productId);
        } catch (error) {
          console.warn('Could not fetch product by ID:', error);
        }
      }
      
      if (product) {
        setSelectedProduct(product);
      } else {
        toast.warning('Product not available');
      }
    }
  };
  loadData();
}, []);
```

### Prevention

- Always implement fallback lookups for pre-selected items
- Validate that passed IDs are non-empty before using them
- Add loading states to handle async lookups gracefully

---

## Quick Reference Checklist

When implementing new features, verify:

- [ ] DateTime values are converted to UTC before database queries
- [ ] Admin users bypass permission checks appropriately
- [ ] Specific lookup endpoints are used instead of paginated lists for single items
- [ ] Validation endpoints return preview statistics
- [ ] Pre-selected items have fallback direct lookups
- [ ] All role-based methods receive and check `isAdmin` parameter

---

## Related Documentation

- [Sales Management Spec](../sales-management/spec.md)
- [Inventory Management Spec](../inventory-management/spec.md)
- [Access Control Spec](../access-control/spec.md)
- [Backend Testing Spec](../backend-testing/spec.md)
