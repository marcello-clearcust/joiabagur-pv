## 1. Specification
- [x] 1.1 Confirm and approve deltas for `point-of-sale-management` and `sales-management`.

## 2. Backend
- [x] 2.1 Add `AllowManualPriceEdit` to `PointOfSale` (entity, persistence, DTOs, validators, mappings).
- [x] 2.2 Add `PriceWasOverridden` and `OriginalProductPrice` to `Sale` (entity, migration, DTOs/mappings).
- [x] 2.3 Update sales create contract to accept optional `price` and validate against POS policy (`allowManualPriceEdit` + `price > 0`).
- [x] 2.4 Implement server-side price resolution and audit persistence rules for overridden vs non-overridden sales.
- [x] 2.5 Include override indicator fields in sales history/detail API responses.

## 3. Frontend
- [x] 3.1 Add `AllowManualPriceEdit` control to POS create/edit forms for administrators.
- [x] 3.2 Update manual and image-assisted sales forms to show editable price only when selected POS allows it.
- [x] 3.3 Keep default sale price prefilled from product official price when edit is enabled.
- [x] 3.4 Show "Precio modificado" indicator in sales history and sale detail views.

## 4. Validation
- [x] 4.1 Add/adjust backend unit and integration tests for POS flag behavior, sales price validation, and audit fields.
- [x] 4.2 Add/adjust frontend tests for conditional price input rendering and override badge visibility.
- [x] 4.3 Run `openspec validate add-pos-manual-price-edit --strict`.
