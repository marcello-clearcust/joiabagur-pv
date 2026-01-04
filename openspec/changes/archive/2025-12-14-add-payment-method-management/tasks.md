# Add Payment Method Management - Tasks

> **⚠️ Deferred Integration Tasks**: Some tasks in this spec are deferred to future epics (EP3 - Sales Registration).  
> **See**: `openspec/DEFERRED_TASKS.md` for centralized tracking of all deferred tasks and when they should be implemented.

## Backend Implementation

### 1. Database Schema Updates
- [x] Add PaymentMethod entity (Id, Code, Name, Description, IsActive, CreatedAt, UpdatedAt)
- [x] Add PointOfSalePaymentMethod entity (Id, PointOfSaleId, PaymentMethodId, IsActive, CreatedAt, DeactivatedAt)
- [x] Add unique constraints (PaymentMethod.Code, PointOfSalePaymentMethod composite key)
- [x] Create Entity Framework migrations
- [x] Update database context with new entities

### 2. Domain Layer
- [x] Create PaymentMethod entity in Core layer
- [x] Create PointOfSalePaymentMethod entity
- [x] Create IPaymentMethodRepository and IPointOfSalePaymentMethodRepository interfaces
- [x] Implement repositories with EF Core
- [x] Create PaymentMethodService with business logic
- [x] Add validation rules (unique codes, required fields)

### 3. API Layer - Payment Method CRUD
- [x] Create PaymentMethodController with REST endpoints
- [x] Implement GET /api/payment-methods (admin only)
- [x] Implement POST /api/payment-methods (admin only - for future extensibility)
- [x] Implement PUT /api/payment-methods/{id} (admin only)
- [x] Implement PATCH /api/payment-methods/{id}/status (activate/deactivate, admin only)

### 4. API Layer - Point of Sale Assignments
- [x] Create PointOfSalePaymentMethod endpoints (added to PointOfSalesController)
- [x] Implement GET /api/point-of-sales/{id}/payment-methods (assigned methods)
- [x] Implement POST /api/point-of-sales/{id}/payment-methods/{methodId} (assign)
- [x] Implement DELETE /api/point-of-sales/{id}/payment-methods/{methodId} (unassign)
- [x] Implement PATCH /api/point-of-sales/{id}/payment-methods/{methodId}/status (activate/deactivate)

### 5. Seed Data
- [x] Create database seeder for predefined payment methods
- [x] Seed initial payment methods: CASH, BIZUM, TRANSFER, CARD_OWN, CARD_POS, PAYPAL
- [x] Ensure seed data runs in migrations
- [x] Make seed data idempotent (safe to run multiple times)

### 6. Sales Validation Integration
- [x] Update sales validation service to check payment method availability (IsPaymentMethodAvailableAsync added)
- [x] Add method to validate payment method is active for point of sale
- [ ] ⏸️ **DEFERRED** Integrate validation into sale creation workflow (requires EP3 - Sales system)
- [ ] ⏸️ **DEFERRED** Return appropriate error messages for invalid payment methods (requires EP3 - Sales system)

### 7. Authorization & Validation
- [x] Add [Authorize(Roles = "Admin")] to admin-only endpoints
- [x] Add model validation and error handling
- [x] Rate limiting inherited from global rate limiting configuration
- [x] Validate point of sale exists before assignments

### 8. Business Rules
- [ ] ⏸️ **DEFERRED** Prevent deactivation of payment methods assigned to active sales (requires EP3 - Sales system)
- [ ] ⏸️ **OPTIONAL** Ensure at least one payment method per point of sale (business decision - not required)
- [x] Validate payment method codes are unique and follow naming convention
- [x] Maintain audit trail for assignment changes (CreatedAt, UpdatedAt, DeactivatedAt tracked)

## Testing

### 9. Unit Tests
- [x] Test PaymentMethodService business logic
- [x] Test payment method repositories (via service tests with mocks)
- [x] Test validation rules and business rules
- [x] Test sales validation integration (IsPaymentMethodAvailableAsync)

### 10. Integration Tests
- [x] Test API endpoints with authentication
- [x] Test CRUD operations for payment methods
- [x] Test assignment operations
- [x] Test seed data functionality
- [ ] ⏸️ **DEFERRED** Test sales validation with payment methods (requires EP3 - Sales system)

### 11. End-to-End Tests
- [x] Test complete payment method management workflow
- [ ] ⏸️ **DEFERRED** Test sales creation with valid/invalid payment methods (requires EP3 - Sales system)
- [x] Test admin vs operator access controls

## Frontend Implementation

### 12. Frontend Services & Types
- [x] Create PaymentMethod types and interfaces
- [x] Create PaymentMethodService with API integration
- [x] Unit tests for PaymentMethodService

### 13. Frontend Components
- [x] Create PaymentMethodsPage with data grid
- [x] Create PaymentMethodFormDialog component
- [x] Implement CRUD operations (create, edit, status toggle)
- [x] Add show/hide inactive filter
- [x] Component tests for PaymentMethodsPage

### 14. Frontend E2E Tests
- [x] E2E tests for payment method list display
- [x] E2E tests for create payment method workflow
- [x] E2E tests for edit payment method workflow
- [x] E2E tests for status toggle (activate/deactivate)
- [x] E2E tests for error handling

## Documentation & Validation

### 15. Documentation Updates
- [x] API documentation available via Swagger/OpenAPI
- [ ] 📝 **DEFERRED** Update data model documentation (deferred to full system documentation phase)
- [x] Predefined payment methods documented in spec
- [ ] ⏸️ **DEFERRED** Sales validation documentation (requires EP3 - Sales system)

### 16. Validation & Deployment
- [x] Run openspec validate --strict
- [x] Test in development environment with seed data
- [ ] 🚀 **READY** Deploy to staging for testing (can be done now)
- [ ] ⏸️ **DEFERRED** Verify sales validation works correctly (requires EP3 - Sales system)

## Implementation Summary

### Completed Features ✅

**Backend (100% Complete)**
- ✅ PaymentMethod and PointOfSalePaymentMethod entities
- ✅ Complete CRUD API with authorization (admin only)
- ✅ Seed data for 6 predefined payment methods
- ✅ Point of sale assignment endpoints
- ✅ Sales validation service integration
- ✅ Unit tests (PaymentMethodService)
- ✅ Integration tests (24 tests in PaymentMethodsControllerTests.cs)

**Frontend (100% Complete)**
- ✅ PaymentMethod types and service
- ✅ PaymentMethodsPage with data grid
- ✅ PaymentMethodFormDialog for create/edit
- ✅ Show/hide inactive toggle
- ✅ Status activation/deactivation
- ✅ Unit tests for service
- ✅ Component tests for page
- ✅ E2E tests for complete workflow

**Test Coverage**
- Backend Unit Tests: ✅ Complete
- Backend Integration Tests: ✅ 24 tests covering all endpoints
- Frontend Service Tests: ✅ Complete
- Frontend Component Tests: ✅ Complete
- Frontend E2E Tests: ✅ Complete

### Deferred Items (Future Epics)

The following items are deferred because they require the sales system (EP3):
- Prevent deactivation of payment methods with active sales
- Sales creation with payment method validation
- End-to-end sales validation tests

Optional items not implemented:
- Ensure at least one payment method per point of sale (business decision)

### Next Steps

1. ✅ Run `openspec validate add-payment-method-management --strict`
2. ✅ Test all functionality in development environment
3. ⏳ Archive spec with `openspec archive add-payment-method-management --yes`
4. ⏳ Deploy to staging environment
5. ⏳ Verify with real data and multiple users

### Notes

- All acceptance criteria from proposal.md are met
- Integration with EP8 (Point of Sale) is working correctly
- Integration with EP7 (Authentication) is working correctly
- Frontend follows Metronic design patterns and is mobile-responsive
- API follows REST conventions and error handling patterns
- Seed data is idempotent and creates 6 predefined payment methods
