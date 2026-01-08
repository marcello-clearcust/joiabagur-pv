# Change: Add Payment Method Management

## Why

The system requires payment method management to support diverse payment options across different points of sale. Each location may accept different payment methods (cash, cards, digital payments), and the system needs to validate available payment methods during sales transactions. This is Epic EP6, which enables flexible payment processing across the multi-location retail system.

## What Changes

- **NEW** `payment-method-management` capability: Complete CRUD operations for payment methods, point-of-sale assignments, and sales validation

### Breaking Changes

None - this is new functionality that extends the existing system.

## Impact

- **Affected specs:**
  - `backend` (existing) - will leverage existing API patterns and validation
  - `point-of-sale-management` (existing from EP8) - will integrate with point-of-sale assignments
  - `payment-method-management` (new capability)

- **Affected code:**
  - Backend: Domain layer (PaymentMethod, PointOfSalePaymentMethod entities, services), Infrastructure (EF Core repositories), API (controllers, validation)
  - Database: PaymentMethod and PointOfSalePaymentMethod tables with seed data
  - Sales validation logic (integration with existing sale processing)

## User Stories Covered

- HU-EP6-001: Configurar métodos de pago disponibles en el sistema
- HU-EP6-002: Asignar métodos de pago a punto de venta
- HU-EP6-003: Activar/desactivar método de pago en punto de venta

## Dependencies

- EP8 (Point of Sale Management) - completed (PointOfSale entity required)
- Basic sales validation framework (can be integrated later)

## Success Criteria

- Administrators can configure payment methods system-wide
- Payment methods can be assigned to specific points of sale
- Sales validation properly checks available payment methods
- Predefined payment methods are seeded correctly
- API follows existing REST patterns and error handling

## Deferred Integrations

**Note**: Some integration tasks are deferred to EP3 (Sales Registration):
- Sales validation integration (using `IsPaymentMethodAvailableAsync`)
- Prevent deactivation of payment methods with active sales
- E2E tests for sales with payment methods

**See**: `openspec/DEFERRED_TASKS.md` for complete list and implementation guidance when building EP3.