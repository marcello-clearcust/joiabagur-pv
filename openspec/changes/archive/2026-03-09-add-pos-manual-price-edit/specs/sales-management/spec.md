## MODIFIED Requirements
### Requirement: Sales Registration with Dual Entry Methods

The system SHALL allow operators to register sales using two methods: AI-assisted image recognition (with photo attached) or manual product selection (with optional photo). Both methods validate stock availability, payment method assignment, operator authorization, and point-of-sale price policy before creating sale records.

#### Scenario: Create sale with image recognition successfully

- **WHEN** authenticated operator captures product photo
- **AND** selects product from AI suggestions
- **AND** enters quantity (>= 1)
- **AND** selects payment method assigned to point of sale
- **AND** product has sufficient stock at point of sale
- **THEN** system creates Sale record using effective sale price rules (official Product.Price by default, optional override only if POS allows manual price edit)
- **AND** creates SalePhoto record with compressed photo (JPEG 80%, <= 2MB)
- **AND** creates InventoryMovement record with type "Sale" (via inventory-management integration)
- **AND** updates Inventory.Quantity atomically in same transaction
- **AND** returns success with sale ID and low stock warning if applicable

#### Scenario: Create manual sale without photo

- **WHEN** authenticated operator searches and selects product by SKU or name
- **AND** enters quantity (>= 1)
- **AND** selects payment method assigned to point of sale
- **AND** product has sufficient stock at point of sale
- **THEN** system creates Sale record using effective sale price rules
- **AND** SalePhoto is null (no photo attached)
- **AND** creates InventoryMovement and updates stock atomically
- **AND** returns success with sale ID

#### Scenario: Create manual sale with optional photo

- **WHEN** authenticated operator manually selects product
- **AND** optionally attaches photo (e.g., for documentation purposes)
- **AND** enters quantity and selects payment method
- **AND** product has sufficient stock
- **THEN** system creates Sale using effective sale price rules
- **AND** creates SalePhoto with compressed photo
- **AND** creates InventoryMovement and updates stock atomically

#### Scenario: Reject sale with insufficient stock

- **WHEN** operator attempts to create sale
- **AND** requested quantity > available stock at point of sale
- **THEN** system validates stock before creating sale record
- **AND** returns 400 Bad Request with error: "Stock insuficiente. Disponible: X, Solicitado: Y"
- **AND** does not create sale, photo, or inventory movement
- **AND** operator can cancel or reduce quantity

#### Scenario: Double stock validation for concurrency safety

- **WHEN** operator selects product and enters quantity
- **THEN** system performs first validation to show available stock in form
- **AND** displays "Stock disponible: X unidades"
- **WHEN** operator confirms sale
- **THEN** system performs second validation immediately before transaction commit
- **AND** validates stock has not changed since first check
- **AND** if stock changed (another sale occurred), returns error: "Stock cambió durante la venta. Disponible ahora: X, Solicitado: Y"
- **AND** operator can retry with updated stock information

#### Scenario: Concurrent sales of last unit

- **WHEN** two operators attempt to sell last unit of product simultaneously
- **AND** both see "Stock disponible: 1" (first validation passed for both)
- **AND** Operator A confirms sale first (stock becomes 0)
- **AND** Operator B confirms sale second
- **THEN** Operator B's second validation fails
- **AND** returns error: "Stock cambió. Disponible: 0, Solicitado: 1"
- **AND** Operator A's sale succeeds, Operator B's sale is rejected

#### Scenario: Reject sale for unassigned product

- **WHEN** operator attempts to create sale
- **AND** product has no Inventory record at point of sale (IsActive = false or missing)
- **THEN** system returns 400 Bad Request with error: "El producto no está asignado a este punto de venta"
- **AND** does not create sale record
- **NOTE**: Uses IStockValidationService from inventory-management

#### Scenario: Reject sale with unavailable payment method

- **WHEN** operator attempts to create sale
- **AND** payment method is not assigned to point of sale OR IsActive = false
- **THEN** system returns 400 Bad Request with error: "El método de pago no está disponible en este punto de venta"
- **AND** does not create sale record
- **NOTE**: Uses payment-method-management validation integration

#### Scenario: Reject sale from unauthorized operator

- **WHEN** operator attempts to create sale at point of sale not assigned to them
- **THEN** system returns 403 Forbidden
- **AND** provides error message indicating unauthorized access
- **NOTE**: Uses access-control integration

#### Scenario: Reject manual price when POS disallows overrides

- **WHEN** operator submits a sale request with explicit price
- **AND** selected point of sale has AllowManualPriceEdit = false
- **THEN** system returns 400 Bad Request with validation error
- **AND** does not create sale record

#### Scenario: Validate manual price is positive when provided

- **WHEN** operator submits a sale request with manual price <= 0
- **THEN** system returns 400 Bad Request with validation error
- **AND** does not create sale record

#### Scenario: Low stock warning after sale

- **WHEN** sale is created successfully
- **AND** remaining stock after sale <= Inventory.MinimumThreshold (if configured)
- **THEN** system returns success response with lowStockWarning flag = true
- **AND** includes remaining stock quantity in response
- **AND** frontend displays non-blocking toast: "⚠️ Quedan solo X unidades de este producto"
- **NOTE**: Warning does not block sale, only informs operator

#### Scenario: Validate quantity is positive

- **WHEN** operator attempts to create sale with quantity <= 0
- **THEN** system returns 400 Bad Request with validation error
- **AND** does not create sale record

#### Scenario: Price snapshot on sale creation

- **WHEN** sale is created
- **THEN** system captures current Product.Price at time of sale as official price reference
- **AND** stores effective price in Sale.Price
- **AND** stores PriceWasOverridden = true and OriginalProductPrice = official Product.Price when override is applied
- **AND** stores PriceWasOverridden = false and OriginalProductPrice = null when override is not applied
- **AND** subsequent product price changes do not affect historical sale pricing fields

## ADDED Requirements
### Requirement: Sales Override Indicator in History and Details

The system SHALL expose and display a clear indicator when a sale used a manually overridden price.

#### Scenario: Override badge in sales history list

- **WHEN** user requests sales history
- **AND** a returned sale has PriceWasOverridden = true
- **THEN** API response includes the override flag
- **AND** frontend shows a visible indicator such as "Precio modificado" for that sale row

#### Scenario: Override details in sale detail view

- **WHEN** user requests a specific sale detail
- **AND** the sale has PriceWasOverridden = true
- **THEN** API response includes PriceWasOverridden and OriginalProductPrice
- **AND** frontend shows the overridden sale price and original product price reference
