## Why

Stock import from Excel currently only accepts quantities ≥ 0, treating every imported value as an addition to existing stock. This forces administrators to use manual adjustment (one product at a time) whenever they need to subtract units — a tedious workflow when correcting counts for multiple products at once. Manual adjustment already supports negative changes with the same "stock cannot go below zero" guard, so the business rule is already proven; the import pathway simply doesn't expose it yet.

## What Changes

- **Remove the `Quantity >= 0` validation** in `StockImportService.ValidateRowsAsync` so negative integers are accepted.
- **Process negative-quantity rows** in `ImportAsync` (today rows with `Quantity <= 0` are skipped). Apply `inventory.Quantity += row.Quantity` for all non-zero rows and validate the resulting stock ≥ 0 across all rows before committing.
- **Fail the entire import** if any row would produce negative final stock, providing a clear per-row error message (Spanish, consistent with existing messages such as "El stock no puede ser negativo").
- **Update the Excel template** (`GenerateTemplate`): remove the `MinValue = 0` constraint, update column description and sheet instructions to explain that positive values add stock and negative values subtract it.
- **Update `MovementType.Import` documentation** to reflect that it covers both additions and subtractions via Excel import (keep the single enum value; no new value needed).
- **Update unit and integration tests** to cover negative-quantity success, negative-quantity-exceeds-stock failure, and zero-quantity no-op.
- **Update frontend import page copy** and type descriptions to inform users that negative quantities are allowed and that final stock cannot go below zero.
- **Update specs and docs** (`inventory-management/spec.md`, `README`) to reflect the new behavior.

## Capabilities

### New Capabilities

*(none — this change extends an existing capability)*

### Modified Capabilities

- `inventory-management`: The "Stock Import from Excel" requirement changes to accept negative `Quantity` values (subtract from stock) with the constraint that resulting stock must remain ≥ 0. The "Download Excel template" scenario changes to remove the `Quantity >= 0` data-validation rule and update descriptions. The "Stock Validation" requirement gains a new scenario for import-originated decreases.

## Impact

- **Backend service**: `StockImportService` (Application layer) — validation and import logic changes.
- **Domain enum**: `MovementType.Import` XML doc update only (no value change).
- **Excel template**: Generated `.xlsx` changes (column description, data validation, instructions).
- **Tests**: `StockImportServiceTests` (unit), `InventoryIntegrationTests` (integration) — new and modified test cases.
- **Frontend**: `import.tsx` page copy, `inventory.service.ts` / `inventory.types.ts` descriptions.
- **Docs**: `openspec/specs/inventory-management/spec.md`, `README.md`.
- **API contract**: No change — the endpoint remains multipart upload; only the accepted data values change.
