## 1. Backend — Domain & Validation

- [x] 1.1 Update `MovementType.Import` XML doc comment in `backend/src/JoiabagurPV.Domain/Enums/MovementType.cs` to read "Stock change via Excel import (addition or subtraction)" instead of "Stock addition via Excel import".
- [x] 1.2 In `StockImportService.ValidateRowsAsync`, remove the `Quantity < 0` validation block that adds an error for negative quantities. Keep all other validations (duplicate SKU, SKU existence, inactive product).
- [x] 1.3 In `StockImportService.ImportAsync`, change the `if (row.Quantity > 0)` guard to `if (row.Quantity != 0)` so negative-quantity rows are also processed (zero-quantity rows remain a no-op).
- [x] 1.4 In `StockImportService.ImportAsync`, add a pre-commit stock-floor validation pass: after parsing rows and before the mutation loop, load current inventory quantities for all SKUs, compute the projected quantity per SKU (`current + sum of deltas`), and if any projected quantity is < 0, add per-row `ImportError` entries with the message format `"Fila {row}: El stock resultante para '{SKU}' sería negativo (actual: {current}, cambio: {delta}, resultado: {projected})."` and return without opening a transaction. Negative-quantity rows for products not yet assigned to the POS count as current = 0.
- [x] 1.5 Verify that the build succeeds (`dotnet build` in `backend/`) and no existing tests break (`dotnet test` in `backend/`).

## 2. Backend — Excel Template

- [x] 2.1 In `StockImportService.GenerateTemplate`, update `ExcelColumnConfig` for `ColQuantity`: remove `MinValue = 0`, update `Description` to `"Quantity to add or subtract (required). Positive values add stock, negative values subtract. Final stock cannot be negative."`.
- [x] 2.2 Update `ExcelTemplateConfig.Instructions` to explain that positive values add and negative values subtract, and that the import will fail if any row would result in negative stock.
- [x] 2.3 Add a third example row with a negative quantity (e.g., `{ ColSku, "BRAC-003" }, { ColQuantity, -5 }`).
- [x] 2.4 Verify template generation still works (unit test or manual check).

## 3. Backend — Unit Tests

- [x] 3.1 In `StockImportServiceTests.cs`, replace `ValidateAsync_WithNegativeQuantity_ShouldReturnError` with `ValidateAsync_WithNegativeQuantity_ShouldPass` (or rename): a negative quantity with a valid active SKU must pass format/row validation (stock-floor is checked only at import time).
- [x] 3.2 Add test `ImportAsync_WithNegativeQuantity_ShouldReduceStock`: given existing inventory with sufficient stock, import a negative-quantity row and assert that `Inventory.Quantity` is correctly reduced and an `InventoryMovement` with negative `QuantityChange` and `MovementType.Import` is created.
- [x] 3.3 Add test `ImportAsync_WithNegativeQuantityExceedingStock_ShouldReturnError`: given existing inventory with Quantity = 2, import Quantity = -5 and assert the import fails with a clear error message and no inventory/movement changes.
- [x] 3.4 Add test `ImportAsync_WithZeroQuantity_ShouldNotCreateMovement`: import a row with Quantity = 0 and assert no `InventoryMovement` is created and stock is unchanged.
- [x] 3.5 Add test `ImportAsync_NegativeQuantityForUnassignedProduct_ShouldReturnError`: import a negative quantity for a product not assigned to the POS and assert the import fails (current stock is effectively 0).
- [x] 3.6 Run the full test suite (`dotnet test` in `backend/`) and confirm all tests pass.

## 4. Backend — Integration Tests

- [x] 4.1 In `InventoryIntegrationTests.cs`, add test: upload Excel with negative `Quantity` against inventory with enough stock → assert HTTP 200, verify final inventory quantity via GET, and verify movement record exists with correct `QuantityChange`.
- [x] 4.2 Add integration test: upload Excel with negative `Quantity` that exceeds available stock → assert HTTP 400 (or the status code returned today for validation failures), verify inventory is unchanged.
- [x] 4.3 Run the full test suite and confirm all tests pass (including existing integration tests).

## 5. Frontend — UX Copy & Types

- [x] 5.1 In `frontend/src/pages/inventory/import.tsx`, update user-facing text: replace "Quantity" description text that says values are "to add" and "≥ 0" with copy explaining that values may be positive (add) or negative (subtract) and that the final stock cannot go below zero.
- [x] 5.2 In `frontend/src/types/inventory.types.ts` and `frontend/src/services/inventory.service.ts`, check for any user-facing descriptions or comments referencing "Quantity >= 0" or "add only" and update to reflect the new behavior.
- [x] 5.3 Verify the frontend builds (`npm run build` in `frontend/`) and existing frontend tests pass.

## 6. Docs & Specs

- [x] 6.1 Update `openspec/specs/inventory-management/spec.md` by applying the delta spec from this change (negative-quantity scenarios, updated template description, updated format validation).
- [x] 6.2 Update `README.md` if it describes stock import behavior (mention that negative quantities are now supported).
- [x] 6.3 Update `Documentos/Historias/HU-EP2-001.md` (if it exists) to reflect that the Quantity column now accepts negative values.
