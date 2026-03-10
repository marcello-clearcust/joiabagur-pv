# Change: Add sales cart with atomic bulk checkout

## Why
The current sales flow only supports single-sale registration per submission, which forces repeated manual work when operators need to register multiple products in one customer interaction. This increases friction, raises the chance of inconsistent payment/POS selections, and makes stock race conditions more likely across repeated submissions.

## What Changes
- Add backend capability for bulk sale creation through `POST /api/sales/bulk` using a single transaction (all-or-nothing behavior).
- Add frontend sales cart flow (`/sales/cart`) with persisted cart state, line removal, stock revalidation, and checkout confirmation.
- Enforce shared POS and payment method constraints across all lines in a cart checkout.
- Support global checkout note propagated to each sale line at confirmation time.
- Keep image-photo support per line only for lines originating from image-recognition flow data.
- Add cart visibility entry points in manual sales and image sales pages plus a cart count badge in sales navigation.
- Add stricter checkout safety and operations reliability with:
  - Idempotent bulk submission support (`Idempotency-Key`) to avoid accidental duplicates.
  - Batch traceability (`BulkOperationId`) to group independent sale records created in one checkout.
  - Cart persistence policy with expiration (TTL = 10 hours) to avoid stale checkouts after long inactivity.

## Impact
- Affected specs:
  - `sales-management`
  - `frontend`
- Affected code (expected):
  - Backend: `SalesController`, `ISalesService`, `SalesService`, DTOs and validators for bulk requests
  - Frontend: sales pages (`new`, `new-image`, new `cart` page), cart context provider, routes, sales service, shared sales types, sales layout/nav badge
