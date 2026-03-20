## Context

Stock import (`StockImportService`) currently validates `Quantity >= 0` during row validation and only processes rows where `Quantity > 0` during import. Manual adjustment (`InventoryService.AdjustStockAsync`) already supports negative changes with the same "stock cannot go below zero" guard. This means the business rule already exists — the import path simply blocks it at the validation layer.

The import uses an all-or-nothing transaction: rows are validated first, then applied inside `BeginTransactionAsync` / `CommitTransactionAsync`. This transactional structure is preserved and extended with a pre-commit stock check.

## Goals / Non-Goals

**Goals:**

- Accept negative integers in the `Quantity` column of stock import Excel files.
- Ensure that no import row causes final stock to drop below zero; if any row would, fail the entire import before committing.
- Keep `MovementType.Import` for all import-originated movements (both additions and subtractions).
- Update the downloadable Excel template to describe the new behavior and remove the `>= 0` data-validation rule.
- Update frontend copy to inform users that negative quantities are supported.
- Maintain all existing behavior for positive and zero quantities unchanged.

**Non-Goals:**

- Adding a new `MovementType` enum value (the existing `Import` value covers both directions).
- Changing the API contract (endpoint signature, request/response DTOs).
- Adding per-row partial success (the import remains all-or-nothing).
- Implementing import for other entities (products, etc.).

## Decisions

### D1: Keep `MovementType.Import` for both positive and negative import rows

**Decision**: Use `MovementType.Import` for all Excel-import movements regardless of sign.

**Rationale**: `MovementType.Import` identifies the *origin* of the movement (Excel file), not its direction — `QuantityChange` already carries the sign. Using `Adjustment` for negative import rows would conflate two distinct origins and make movement-history filtering unreliable. This matches how `Sale` handles all sale movements regardless of quantity.

**Alternative considered**: Use `Adjustment` for negative rows — rejected because it loses the information that the movement came from an import file, making audit trail less useful.

### D2: Two-pass validation in `ImportAsync` — validate all rows against projected stock before any DB write

**Decision**: After parsing rows, compute the projected final stock for each (product, POS) pair by summing all row deltas for that SKU against the current DB quantity. If any projected quantity is < 0, add a validation error and abort before the mutation loop.

**Rationale**: This prevents partial writes and gives the user a complete list of problems in one shot. It also avoids hitting the DB mutation path at all when the file is invalid.

**Sequence**:

```
Client → API Controller → StockImportService.ImportAsync
  1. Parse & validate format/SKU (existing)
  2. Load current inventory quantities for all SKUs in file
  3. Compute projected quantity per SKU (current + sum of deltas)
  4. If any projected < 0 → return errors, no transaction opened
  5. Begin transaction → apply deltas → commit
```

### D3: Aggregate duplicate-SKU quantities for stock-floor check

**Decision**: Although duplicate SKUs in a single file are already rejected by the existing validation, the stock-floor check will still aggregate by SKU defensively in case future changes relax the duplicate rule.

**Rationale**: Defense in depth — the projected-stock calculation groups by normalized SKU so that if two rows for the same SKU were ever allowed, the check would still be correct.

### D4: Error message format for negative-stock failures

**Decision**: Use Spanish, consistent with existing validation messages. Format: `"Fila {row}: El stock resultante para '{SKU}' sería negativo (actual: {current}, cambio: {delta}, resultado: {projected})."` Each failing row produces its own error entry.

**Rationale**: Matches the existing `ImportError` structure (RowNumber, Field, Message, Value) and the Spanish-language convention used throughout the API.

### D5: `Quantity == 0` rows — preserve existing no-op behavior

**Decision**: Rows with `Quantity == 0` continue to be skipped (no movement created, no stock change). They pass validation but produce no side effect, identical to today's behavior.

**Rationale**: Zero-quantity rows are harmless and users may include them as placeholders. Changing this behavior is out of scope.

## Risks / Trade-offs

**[Risk] Concurrent import and manual adjustment race condition** → The existing transaction isolation level (default `ReadCommitted` in PostgreSQL/EF Core) means a concurrent manual adjustment between the stock-floor check and the commit could theoretically allow stock to go negative. → *Mitigation*: At the expected scale (2-3 concurrent users), this is extremely unlikely. If needed in the future, a `SELECT ... FOR UPDATE` or serializable isolation could be added. Documenting this as a known limitation.

**[Risk] Large files with many negative rows produce many error messages** → *Mitigation*: The existing validation already produces per-row errors for SKU/format issues. Adding stock-floor errors follows the same pattern. The frontend already renders error lists.

**[Trade-off] All-or-nothing import for mixed files** → A file with 100 valid additions and 1 invalid subtraction fails entirely. This is the existing behavior for other validation errors and is intentional for data consistency. Users can fix the offending row and re-upload.
