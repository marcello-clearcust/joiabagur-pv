## 1. Implementation
- [x] 1.1 Define backend DTOs/contracts for bulk checkout (`CreateBulkSalesRequest`, result payload, optional idempotency metadata) and align validation rules with single-sale constraints.
- [x] 1.2 Implement `POST /api/sales/bulk` in `SalesController` and `ISalesService`/`SalesService` with all-or-nothing transaction behavior.
- [x] 1.3 Enforce cross-line invariants in backend (same POS, same payment method, stock validation per line, optional global note propagation).
- [x] 1.4 Add batch traceability metadata (`BulkOperationId`) to created sales and response contract.
- [x] 1.5 Add backend idempotency handling for retried bulk submissions using `Idempotency-Key`.
- [x] 1.6 Implement frontend cart state management (Context + storage persistence + TTL expiration policy).
- [x] 1.7 Create `/sales/cart` page with list, remove action, totals, payment method selection, stock status, and checkout confirmation dialog.
- [x] 1.8 Update `/sales/new` to support "Añadir al carrito" with existing stock checks and shared POS/payment constraints.
- [x] 1.9 Update `/sales/new/image` and sales layout/navigation to expose "Ver carrito" and cart badge count.
- [x] 1.10 Add frontend service/types for bulk checkout request/response and cart line models.

## 2. Validation
- [x] 2.1 Backend unit/integration tests: successful bulk checkout, insufficient stock rollback, mixed POS rejection, mixed payment rejection, idempotent retry behavior.
- [x] 2.2 Frontend tests: cart persistence/expiration, add/remove flows, insufficient stock blocking, confirmation dialog payload.
- [ ] 2.3 End-to-end scenario: compose cart from manual sales page, checkout bulk sale, verify cart clears and redirection succeeds.
- [ ] 2.4 Run `openspec validate add-sales-cart-bulk-checkout --strict`.
