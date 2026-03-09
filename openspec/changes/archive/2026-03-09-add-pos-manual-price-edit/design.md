## Context
The current sales flow always snapshots `Product.Price` into `Sale.Price`. New business rules require optional manual price edits per point of sale, plus auditability for overridden prices, while preserving current behavior when editing is not enabled.

## Goals / Non-Goals
- Goals:
  - Gate manual sale price edits with a POS-level configuration (`AllowManualPriceEdit`).
  - Keep existing pricing behavior unchanged when the flag is disabled.
  - Store explicit audit metadata when the submitted sale price differs from the catalog price.
  - Make overridden sales visible in history/detail views.
- Non-Goals:
  - Implement discount policies, approval workflows, or max/min override ranges.
  - Recalculate historical sales when product prices change.
  - Add new report endpoints in this change.

## Decisions
- Decision: Add `AllowManualPriceEdit` to `PointOfSale` and expose it in POS read models used by sales UI.
  - Why: The setting belongs to store-level operational policy and must be available to clients before sale submission.
- Decision: Accept optional `price` in sale creation requests; backend computes effective sale price based on POS policy.
  - Why: Backend remains source of truth and prevents client-side bypass.
- Decision: Persist `PriceWasOverridden` and nullable `OriginalProductPrice` in `Sale`.
  - Why: This keeps immutable transaction evidence for audit and reporting without re-deriving from product history.

## Price Resolution Rules
1. Read current `Product.Price` as `officialPrice`.
2. If POS does not allow manual edits:
   - Ignore/Reject manual price input per API contract.
   - Persist `Sale.Price = officialPrice`, `PriceWasOverridden = false`, `OriginalProductPrice = null`.
3. If POS allows manual edits and a request price is provided:
   - Validate `price > 0`.
   - If `price != officialPrice`, persist:
     - `Sale.Price = price`
     - `PriceWasOverridden = true`
     - `OriginalProductPrice = officialPrice`
   - Else persist standard values (`PriceWasOverridden = false`, `OriginalProductPrice = null`).
4. If POS allows manual edits and no request price is provided:
   - Persist standard values from `officialPrice`.

## Risks / Trade-offs
- Risk: Client sends a manual price even when the flag is disabled.
  - Mitigation: Enforce API validation against current POS policy on the server.
- Risk: Decimal comparison edge cases can incorrectly mark overrides.
  - Mitigation: Use consistent decimal precision and direct decimal equality semantics in domain logic.
- Risk: UI confusion between editable and non-editable contexts.
  - Mitigation: Show/hide price input strictly from `allowManualPriceEdit` and keep visible official total when not editable.

## Migration Plan
- Add nullable/boolean columns to `Sale` and boolean column to `PointOfSale`.
- Backfill existing sales with `PriceWasOverridden = false` and `OriginalProductPrice = null`.
- Default `AllowManualPriceEdit` to `false` for existing points of sale.
- Rollback strategy: remove UI behavior first, then API acceptance, then columns in reverse migration.

## Open Questions
- Should API reject submitted `price` when POS disallows edits, or ignore it and proceed with official price? (Proposal assumes explicit rejection for clearer client feedback.)
