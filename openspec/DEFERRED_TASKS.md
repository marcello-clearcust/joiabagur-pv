# Deferred Integration Tasks

This document tracks tasks that were deferred during implementation of completed specs, organized by the future epic/spec that will complete them.

## Tasks Deferred to EP3 (Sales Registration)

### From: add-payment-method-management (EP6)

**Epic**: EP3 - Sales Registration  
**When**: During implementation of `add-sales-registration` spec  
**Priority**: HIGH - Required for sales functionality

#### Sales Validation Integration
- [ ] Integrate payment method validation into sale creation workflow
  - **Location**: Sale creation endpoint in SalesController
  - **Service**: Use `PaymentMethodService.IsPaymentMethodAvailableAsync()`
  - **Spec Reference**: `openspec/changes/archive/2025-12-14-add-payment-method-management/specs/payment-method-management/spec.md`
  - **Test Coverage Required**: Integration tests for sales with valid/invalid payment methods

- [ ] Return appropriate error messages for invalid payment methods during sales
  - **Error Message**: "El método de pago no está disponible en este punto de venta"
  - **HTTP Status**: 400 Bad Request
  - **Spec Reference**: Requirement "Sales Payment Method Validation"

#### Business Rules
- [ ] Prevent deactivation of payment methods assigned to active sales
  - **Business Rule**: Cannot deactivate payment method if it has sales in last 30 days (configurable)
  - **Service Method**: Add validation in `PaymentMethodService.ChangeStatusAsync()`
  - **Error Message**: "No se puede desactivar método de pago con ventas recientes"
  - **Alternative**: Allow deactivation but warn about existing sales

#### Testing
- [ ] Test sales validation with payment methods
  - **Test Type**: Integration tests
  - **Coverage**: Valid payment method, invalid payment method, inactive payment method
  - **File**: Create SalesControllerTests.cs or extend existing tests

- [ ] Test sales creation with valid/invalid payment methods
  - **Test Type**: E2E tests (Playwright)
  - **Scenarios**:
    - Create sale with available payment method → Success
    - Create sale with unavailable payment method → Error 400
    - Create sale with inactive payment method → Error 400

### From: add-point-of-sale-management (EP8)

**Epic**: EP3 - Sales Registration  
**When**: During implementation of `add-sales-registration` spec  
**Priority**: MEDIUM - Optional validation

#### Operator Access Validation
- [ ] Ensure operators can only create sales for their assigned points of sale
  - **Already Implemented**: Access control framework exists
  - **Action Required**: Apply to sales endpoints
  - **Service**: Use existing `IPointOfSaleAuthorizationService`

---

## Tasks Deferred to Future Documentation Phase

### Full System Documentation

**When**: After all 9 epics are implemented  
**Priority**: LOW - Nice to have

#### From: add-payment-method-management
- [ ] Update comprehensive data model documentation
  - Include PaymentMethod and PointOfSalePaymentMethod entities
  - Add to ER diagrams

- [ ] Update sales validation documentation
  - Document payment method validation flow
  - Include error scenarios and handling

#### From: add-point-of-sale-management
- [ ] Update comprehensive data model documentation
  - Include PointOfSale, UserPointOfSale entities
  - Add to ER diagrams

- [ ] Update architectural diagrams
  - Show point of sale access control flow
  - Document operator assignment patterns

---

## Completed Integrations

### From: add-payment-method-management
- [x] ✅ **COMPLETED**: Ensure at least one payment method per point of sale
  - **Implementation**: Validation added in PaymentMethodService
  - **Methods**: UnassignFromPointOfSaleAsync, ChangeAssignmentStatusAsync
  - **Error Messages**: 
    - "El punto de venta debe tener al menos un método de pago asignado" (unassign)
    - "El punto de venta debe tener al menos un método de pago activo" (deactivate)
  - **Test Coverage**: 5 new unit tests added to PaymentMethodServiceTests

---

## How to Use This Document

### When Implementing EP3 (Sales):

1. **Read this document first** before creating the sales spec
2. **Include deferred tasks** in the EP3 proposal and tasks.md
3. **Reference archived specs** for context:
   - `openspec/changes/archive/2025-12-14-add-payment-method-management/`
   - `openspec/changes/archive/2025-12-14-add-point-of-sale-management/`

4. **Create integration tests** that validate the deferred functionality

### When Creating Full System Documentation:

1. **Review all archived changes** in `openspec/changes/archive/`
2. **Consolidate documentation** from individual specs
3. **Update ER diagrams** with all entities
4. **Create architectural diagrams** showing complete system flow

### Tracking Progress:

- Mark items as complete in this file as they're implemented
- Move completed sections to a "Completed Integrations" section at bottom
- Keep this file updated as new deferred tasks are identified

---

## Quick Reference

### Deferred Task Count by Epic

| Epic | Spec | Deferred Tasks | Priority |
|------|------|----------------|----------|
| EP3 (Sales) | add-payment-method-management | 6 tasks | HIGH |
| EP3 (Sales) | add-point-of-sale-management | 1 task | MEDIUM |
| Future Docs | Both specs | 4 tasks | LOW |
| Optional | add-payment-method-management | 1 task | N/A |

### Total Deferred: 11 tasks
- **High Priority** (EP3): 7 tasks
- **Low Priority** (Docs): 4 tasks  
- **Completed**: 1 task (previously optional)

---

## Notes

- All deferred tasks are also documented in the archived `tasks.md` files
- This document provides a centralized view for planning future work
- Update this document when creating new specs with deferred tasks
- Delete or archive sections as deferred tasks are completed
