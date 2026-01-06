# Change: Add Manual Product Creation UI

## Why

Administrators need a user-friendly form to create individual products without using Excel import. This complements the bulk import feature by providing a streamlined interface for adding single products with real-time validation and immediate feedback.

## What Changes

- **Frontend**: Add ProductCreatePage component with form for manual product creation
- **Frontend**: Add real-time validation for SKU uniqueness, price, and required fields
- **Frontend**: Add collection dropdown populated from API
- **Backend Tests**: Add integration tests for POST /api/products endpoint
- **Frontend Tests**: Add component tests for product creation form

**Note**: The backend POST /api/products endpoint is already implemented as part of the `add-product-management` change (HU-EP1-001).

## Impact

- **Affected specs**: `product-management` (adds Manual Product Creation UI requirement)
- **Affected code**:
  - `frontend/src/pages/products/create.tsx` - new page component
  - `frontend/src/pages/products/create.test.tsx` - component tests
  - `backend/tests/JoiabagurPV.API.IntegrationTests/` - integration tests for product creation
- **Dependencies**: Requires completed `add-product-management` change (endpoint exists)

