# Change: Add Inventory Management

## Why

The system requires comprehensive inventory management capabilities to track product stock across multiple points of sale, enable bulk operations via Excel import, support manual adjustments with full audit trails, and control product visibility for operators. This capability is foundational for sales registration (EP3) and must be implemented before sales can be processed.

**Business Need:**
- Administrators need to assign products to points of sale and manage stock quantities
- Operators need visibility only to products assigned to their points of sale (regardless of quantity)
- Stock tracking requires full audit trail for compliance and reconciliation
- Bulk operations via Excel are essential for efficient inventory management

## What Changes

- **NEW Capability**: `inventory-management` - Complete inventory tracking and management system
- **ADDED**: Product assignment/unassignment to points of sale (manual and implicit via Excel import)
- **ADDED**: Stock import from Excel with quantity addition and implicit assignment (includes downloadable template)
- **ADDED**: Stock visualization by point of sale (operator-restricted) and centralized view (admin-only)
- **ADDED**: Manual inventory adjustments with reason tracking and audit trail
- **ADDED**: Complete movement history with filtering and pagination
- **ADDED**: Non-negative stock validation at application level
- **INTEGRATION**: Works with existing `access-control` capability (product filtering by inventory assignment)
- **INTEGRATION**: Prepares foundation for `sales-management` (EP3) which will generate automatic inventory movements

## Impact

- **Affected specs**: 
  - New capability: `inventory-management`
  - Integration point: `access-control` (already references inventory filtering, no changes needed)
- **Affected code**: 
  - New domain entities: `Inventory`, `InventoryMovement` (already in data model)
  - New services: `IInventoryService`, `IInventoryMovementService`
  - New API controllers: `InventoryController`
  - New frontend modules: Inventory management pages and components
  - Excel processing: Extends existing Excel import patterns from product management
- **Dependencies**: 
  - Requires `product-management` (EP1) - products must exist in catalog
  - Requires `point-of-sale-management` (EP8) - points of sale must exist
  - Requires `access-control` (EP7) - for operator filtering
- **Breaking changes**: None (new capability)

