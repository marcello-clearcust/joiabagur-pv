## MODIFIED Requirements

### Requirement: Stock Import from Excel

The system SHALL allow administrators to import stock quantities from Excel files. Quantity values MAY be positive (add stock) or negative (subtract stock). Positive quantities are added to existing Inventory records; negative quantities are subtracted. If a product is not yet assigned to the point of sale, the system SHALL create a new Inventory record (implicit assignment). The point of sale is selected in the UI before file upload; no POS column is required in the Excel file. The import is all-or-nothing: if any row would cause stock to drop below zero, the entire file SHALL be rejected with per-row error details.

#### Scenario: Import stock for existing inventory (positive quantity)

- **GIVEN** a product already has an Inventory record for the selected point of sale
- **WHEN** administrator imports Excel with a row containing a positive Quantity for that product's SKU
- **THEN** system adds imported quantity to existing quantity
- **AND** creates InventoryMovement record with type "Import" and positive QuantityChange
- **AND** updates Inventory.Quantity and LastUpdatedAt

#### Scenario: Import stock with negative quantity (stock decrease)

- **GIVEN** a product has an Inventory record with Quantity = 10 at the selected point of sale
- **WHEN** administrator imports Excel with a row containing Quantity = -3 for that product's SKU
- **THEN** system subtracts 3 from existing quantity (resulting in 7)
- **AND** creates InventoryMovement record with type "Import" and QuantityChange = -3
- **AND** sets QuantityBefore = 10 and QuantityAfter = 7
- **AND** updates Inventory.LastUpdatedAt

#### Scenario: Reject import when negative quantity would cause negative stock

- **GIVEN** a product has an Inventory record with Quantity = 2 at the selected point of sale
- **WHEN** administrator imports Excel with a row containing Quantity = -5 for that product's SKU
- **THEN** system validates all rows before committing any changes
- **AND** returns error for that row: "Fila {row}: El stock resultante para '{SKU}' sería negativo (actual: 2, cambio: -5, resultado: -3)."
- **AND** no Inventory or InventoryMovement records are modified
- **AND** the entire import is rejected (no partial application)

#### Scenario: Reject import when multiple rows for different products cause negative stock

- **GIVEN** product A has Quantity = 5 and product B has Quantity = 1
- **WHEN** administrator imports Excel with Quantity = -3 for product A (valid) and Quantity = -4 for product B (invalid)
- **THEN** system returns an error for the product B row
- **AND** does not apply the product A change either (all-or-nothing)

#### Scenario: Implicit assignment via import (positive quantity only)

- **GIVEN** a product exists in the global catalog but is not assigned to the selected point of sale
- **WHEN** administrator imports Excel with a positive Quantity for that product's SKU
- **THEN** system creates Inventory record with the imported quantity
- **AND** sets IsActive = true (implicit assignment)
- **AND** creates InventoryMovement record with type "Import"
- **AND** product becomes visible to operators
- **AND** system indicates implicit assignment occurred

#### Scenario: Reject negative quantity for unassigned product

- **GIVEN** a product exists in the global catalog but has no Inventory record at the selected point of sale
- **WHEN** administrator imports Excel with a negative Quantity for that product's SKU
- **THEN** system returns error for that row indicating stock would be negative (current stock is 0)
- **AND** does not create an Inventory record

#### Scenario: Zero quantity row produces no stock change

- **GIVEN** a product has an Inventory record at the selected point of sale
- **WHEN** administrator imports Excel with Quantity = 0 for that product's SKU
- **THEN** system does not modify the Inventory quantity
- **AND** does not create an InventoryMovement record
- **AND** the row is silently skipped (no error, no warning)

#### Scenario: Handle invalid SKU in import

- **WHEN** Excel contains SKU not found in global catalog
- **THEN** system validates file before import
- **AND** returns error listing invalid SKUs with format: "Row X: SKU 'ABC-123' not found in catalog"
- **AND** groups all SKU errors together
- **AND** does not import invalid records
- **AND** suggests verifying product was imported in EP1

#### Scenario: Excel format validation

- **WHEN** administrator uploads Excel file
- **THEN** system validates required columns (SKU, Quantity) with exact column name matching
- **AND** validates Quantity is a valid integer (positive, negative, or zero)
- **AND** validates SKU exists in Product catalog
- **AND** validates all rows before processing any
- **AND** shows all validation errors with detailed format (row number, field name, error message)
- **AND** groups errors by type (missing columns, invalid data, business rule violations)
- **AND** shows preview of file contents before import confirmation
- **AND** processes valid rows only after user confirmation

#### Scenario: Download Excel template

- **WHEN** administrator requests stock import template
- **THEN** system provides downloadable Excel template file via GET /api/inventory/import-template endpoint
- **AND** template contains exact column headers: "SKU" and "Quantity" (header row protected/locked)
- **AND** template includes example rows with both positive and negative quantity samples
- **AND** template includes data validation rules (Quantity must be a whole number, SKU text format)
- **AND** template includes instructions explaining: positive values add stock, negative values subtract stock, final stock cannot be negative
- **AND** template has proper formatting (bold headers, number format for Quantity column)

#### Scenario: Import confirmation with summary

- **WHEN** file validation passes and administrator clicks import
- **THEN** system shows confirmation dialog with summary
- **AND** displays total rows to process
- **AND** displays breakdown (e.g., "X products will have stock added, Y products will have stock subtracted, Z products will be assigned")
- **AND** requires explicit confirmation before processing
