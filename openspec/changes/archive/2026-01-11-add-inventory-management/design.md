## Context

Inventory Management is a core capability that enables product assignment to points of sale, stock tracking, bulk operations via Excel import, and complete audit trails. This capability integrates with access control (product visibility filtering) and prepares the foundation for sales registration (EP3) which will automatically generate inventory movements.

**Stakeholders**: Administrators (inventory management), Operators (stock visibility)

**Constraints**:
- Free-tier cloud optimization (connection pooling, pagination)
- Mobile-first operator experience
- ~500 product catalog size
- Multiple points of sale (2-3 concurrent users)

**Dependencies**:
- Product Management (EP1) - products must exist in catalog
- Point of Sale Management (EP8) - points of sale must exist
- Access Control (EP7) - operator filtering logic

## Goals / Non-Goals

**Goals**:
- Enable product assignment/unassignment to points of sale with soft delete
- Support bulk stock import via Excel with implicit assignment
- Provide stock visualization (by POS and centralized)
- Enable manual adjustments with full audit trail
- Enforce non-negative stock validation
- Complete movement history with filtering and pagination

**Non-Goals**:
- Stock alerts/thresholds (Fase 2)
- Automatic reordering (Fase 2)
- Multi-warehouse transfers (Fase 2)
- Barcode scanning (future enhancement)
- Real-time stock synchronization (current design is sufficient for scale)

## Decisions

### Decision 1: Soft Delete Pattern for Unassignment

- **What**: Use `IsActive` flag on Inventory entity instead of hard delete
- **Why**: Preserves historical data for audit, enables reassignment without data loss, maintains referential integrity for InventoryMovement records
- **Trade-off**: Requires filtering by IsActive in queries, but enables complete audit trail

### Decision 2: Implicit Assignment via Excel Import

- **What**: Excel import creates Inventory records automatically if product not assigned
- **Why**: Reduces manual steps for bulk operations, aligns with business workflow
- **Trade-off**: May create assignments unintentionally, but provides clear messaging when this occurs

### Decision 3: Quantity Reset on Reassignment

- **What**: When reactivating previously unassigned product (IsActive = false → true), preserve previous quantity
- **Why**: Maintains historical accuracy, allows recovery of stock data
- **Alternative Considered**: Reset to 0 - rejected because it loses historical data
- **Decision**: Preserve quantity (confirmed by stakeholder)

### Decision 4: Excel Processing Library

- **What**: Use ClosedXML (already in project dependencies, used in product management)
- **Why**: Consistent with existing patterns, lightweight, supports .xlsx and .xls formats
- **Trade-off**: None - already proven in product import

### Decision 8: Excel Template Download

- **What**: Provide downloadable Excel template with exact column names (SKU, Quantity) to prevent format errors
- **Why**: Reduces user errors, ensures consistent format, improves user experience
- **Implementation**: Backend endpoint generates template file, frontend provides download button
- **Consistency**: Same pattern applies to Product Management Excel import

### Decision 9: Shared Excel Template Utility

- **What**: Create shared `IExcelTemplateService` interface and implementation for consistent template generation across Product and Inventory imports
- **Why**: DRY principle, ensures consistent formatting, easier maintenance, single place to update template logic
- **Implementation**: Shared service in Application layer, used by both ProductImportService and StockImportService
- **Benefits**: Consistent header formatting, data validation rules, example data patterns

### Decision 10: Template Quality and Formatting

- **What**: Excel templates include data validation rules, protected header row, proper formatting (bold headers, number formats), and instructions
- **Why**: Reduces user errors, improves UX, guides users on correct format
- **Implementation**: Use ClosedXML features for data validation, cell protection, and formatting

### Decision 5: Movement History Storage

- **What**: Store complete audit trail in InventoryMovement with QuantityBefore/QuantityAfter
- **Why**: Enables reconstruction of inventory state at any point, full compliance audit trail
- **Trade-off**: Table will grow over time, but enables powerful reporting and debugging

### Decision 6: Non-Negative Stock Validation

- **What**: Validate at application level (service layer), not database constraint
- **Why**: Provides better error messages, allows business logic flexibility
- **Trade-off**: Requires careful validation in all code paths, but enables clearer user feedback

### Decision 7: Pagination Strategy

- **What**: Mandatory pagination for all list endpoints (max 50 items/page)
- **Why**: Free-tier optimization, consistent with project conventions
- **Trade-off**: Requires pagination controls in UI, but essential for performance

## Risks / Trade-offs

### Risk 1: Excel Import Performance

- **Risk**: Large Excel files (>1000 rows) may cause timeout or memory issues
- **Mitigation**: Process in batches, validate file size before processing, provide progress feedback
- **Acceptance**: For MVP scale (~500 products), batch processing sufficient

### Risk 2: Concurrent Stock Updates

- **Risk**: Multiple users adjusting same product simultaneously could cause race conditions
- **Mitigation**: Use database transactions, optimistic concurrency (check LastUpdatedAt), or pessimistic locking for critical operations
- **Acceptance**: For 2-3 concurrent users, optimistic concurrency sufficient

### Risk 3: Movement History Table Growth

- **Risk**: InventoryMovement table will grow indefinitely, impacting query performance
- **Mitigation**: Index optimization (already defined in data model), pagination, consider archival strategy in Fase 2
- **Acceptance**: For MVP scale, current indexing sufficient

### Risk 4: Implicit Assignment Confusion

- **Risk**: Users may not realize Excel import creates assignments
- **Mitigation**: Clear messaging when implicit assignment occurs, documentation, UI indicators
- **Acceptance**: Acceptable trade-off for workflow efficiency

## Integration Points

### With Access Control

- **Integration**: Access control spec already defines product filtering by Inventory assignment
- **Impact**: No changes needed to access-control spec
- **Coordination**: Ensure Inventory.IsActive filtering aligns with access-control requirements

### With Sales Management (EP3 - Future)

- **Integration**: Sales will automatically create InventoryMovement records
- **Impact**: Sales capability will reference inventory-management requirements
- **Coordination**: Stock validation requirement will be shared

### With Product Management (EP1)

- **Integration**: Products must exist before assignment
- **Impact**: Validation ensures Product exists before creating Inventory record
- **Coordination**: Use Product.IsActive to prevent assigning inactive products

### With Point of Sale Management (EP8)

- **Integration**: Points of sale must exist before assignment
- **Impact**: Validation ensures PointOfSale exists and is active
- **Coordination**: Use PointOfSale.IsActive to prevent assigning to inactive POS

## Migration Plan

### Initial Data Setup

- **Step 1**: Create Inventory records for existing products (if any) via Excel import or manual assignment
- **Step 2**: Verify operator access filtering works correctly
- **Step 3**: Test stock import workflow

### Rollout Strategy

- **Phase 1**: Deploy backend APIs (assignment, import, adjustment)
- **Phase 2**: Deploy frontend admin pages
- **Phase 3**: Deploy operator stock view
- **Phase 4**: Enable movement history

### Rollback Plan

- **If issues**: Disable inventory endpoints, revert to previous version
- **Data safety**: Soft delete preserves data, no data loss on rollback

## Open Questions

1. ~~**Reassignment Quantity**: When reactivating previously unassigned product (IsActive = false → true), should quantity be preserved or reset to 0?~~ **RESOLVED**: Preserve quantity (confirmed by stakeholder)

2. ~~**Excel Column Names**: Should Excel import support flexible column names (e.g., "SKU" vs "Código") or require exact match?~~ **RESOLVED**: Require exact column names for MVP (confirmed by stakeholder)

3. ~~**Adjustment Reason Length**: What is maximum length for adjustment reason field?~~ **RESOLVED**: 500 characters (confirmed by stakeholder)

4. **Movement History Retention**: Should there be automatic archival of old movements? **Recommendation**: No automatic archival for MVP, manual archival in Fase 2 if needed.

5. ~~**Stock Import Validation**: Should import validate that all SKUs exist before processing any, or process valid rows and report errors?~~ **RESOLVED**: Validate all rows first, show all errors, then process valid rows if user confirms (confirmed by stakeholder)

6. ~~**Movement History Date Range Default**: Should movement history default to a specific date range?~~ **RESOLVED**: Default to last 30 days (confirmed by stakeholder)

