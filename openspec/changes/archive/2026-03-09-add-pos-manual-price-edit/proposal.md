# Change: Add Manual Sale Price Editing by Point of Sale

## Why
Operators need controlled flexibility to adjust sale prices at checkout for specific points of sale without changing the official catalog price. The business also needs an audit trail when a sale uses a manual price override.

## What Changes
- Add a per-point-of-sale boolean setting `AllowManualPriceEdit` that only administrators can configure.
- Expose `allowManualPriceEdit` in point-of-sale retrieval responses used by sales flows.
- Allow sales requests to send an optional manual `price` only when the selected POS has `AllowManualPriceEdit = true`.
- Persist manual price audit data in sales using `PriceWasOverridden` and `OriginalProductPrice`.
- Show a visible indicator in sales history and sale detail when a sale used an overridden price.

## Impact
- Affected specs: `point-of-sale-management`, `sales-management`
- Affected code:
  - Backend: `PointOfSale` entity/DTOs, `Sale` entity/DTOs, sales request validation, sales service price resolution logic, POS retrieval endpoints.
  - Frontend: POS create/edit screens, manual sale and image-assisted sale forms, sales history/detail UI indicators.
