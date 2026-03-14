## Context
Sales currently submit one `CreateSaleRequest` per action from the manual sales page. Operators need a multi-line checkout that preserves current business rules (stock validation, POS/payment authorization, manual price constraints) while keeping backend consistency and mobile-friendly UX.

## Goals / Non-Goals
- Goals:
  - Enable a persistent cart-based flow for composing multi-line sales.
  - Commit multi-line sales atomically with one backend operation.
  - Preserve existing authorization and stock validation guarantees.
  - Prevent duplicate submissions and improve post-creation traceability.
- Non-Goals:
  - Editing quantity/price from cart lines (remove-only in this phase).
  - Mixed POS per checkout.
  - Partial success mode (some lines succeed and others fail).
  - New image-to-cart authoring flow (image page only links to cart in this phase).

## Decisions
- Decision: Introduce `POST /api/sales/bulk` with all-or-nothing transaction semantics.
  - Rationale: Keeps inventory and sales consistency across multi-line checkout.
  - Alternative considered: Client sends N single-sale calls; rejected due to partial failure risk and poor UX.

- Decision: Cart state is frontend-managed (Context + local storage) with a fixed TTL of 10 hours of inactivity.
  - Rationale: Fast UX, no new server persistence, survives reloads.
  - Alternative considered: Server-side draft cart; rejected for MVP complexity.

- Decision: Enforce a single POS and payment method per checkout.
  - Rationale: Matches current business constraints and simplifies validation.
  - Alternative considered: Mixed payment/POS by line; rejected as out-of-scope.

- Decision: Add idempotency and batch grouping metadata for bulk checkout.
  - Rationale: Prevent accidental duplicate charges and improve audit/reporting.
  - Alternative considered: Frontend-only double-click prevention; rejected because retries/network races still duplicate.

## Risks / Trade-offs
- Risk: Stored cart lines become stale versus live stock.
  - Mitigation: Revalidate stock per line before enabling checkout and again on backend.
- Risk: Larger bulk payloads may increase validation cost.
  - Mitigation: Keep line schema aligned with existing `CreateSaleRequest` validation and short-circuit on first failing invariant.
- Risk: Idempotency store design may add implementation complexity.
  - Mitigation: Scope to deterministic key + payload fingerprint and bounded retention window.

## Migration Plan
1. Ship backend bulk API and tests first.
2. Ship frontend cart context and cart page behind route-level availability.
3. Replace manual repeated submissions with add-to-cart plus bulk checkout UX.
4. Monitor validation and duplicate-submission behavior in QA; then promote.

## Open Questions
- Should idempotency retention be 24h or configurable by environment?
- Should `BulkOperationId` be exposed in sales history filter in same change or follow-up?
