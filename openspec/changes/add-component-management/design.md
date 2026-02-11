## Context

Epic 10 introduces jewelry component management to the existing product management system. Components represent the materials, labor, and other elements that make up each jewelry product. This feature enables cost tracking, suggested pricing based on component totals, and margin analysis.

The system currently has 15 domain entities and no concept of product composition. This change adds 4 new entities and integrates component management into the existing Products module as an admin-only feature. Reports integrate into the existing Reports section (EP9).

### Stakeholders
- **Administrators**: Full access to component management, templates, and reports
- **Operators**: No visibility or access to any component feature

## Goals / Non-Goals

### Goals
- Track component costs and suggested sale prices per product
- Calculate total cost and suggested sale price from component assignments in real-time
- Enable quick product setup via reusable component templates with merge logic
- Provide margin analysis and component coverage reporting with Excel export
- Maintain strict separation between official price (`Product.Price`) and suggested price (calculated from components)

### Non-Goals
- Automatic price synchronization from master to existing assignments (sync is user-initiated only)
- Making component-based pricing the official product price (`Product.Price` remains the source of truth for sales)
- Exposing component information to operators or customers
- Inventory tracking for components themselves (components are not stock items)
- Component versioning or price history tracking (Phase 2 consideration)

## Decisions

### 1. Data Model: 4 New Entities

| Entity | Purpose | Key Fields |
|--------|---------|------------|
| `ProductComponent` | Master table | Id, Description (max 35), CostPrice?, SalePrice?, IsActive |
| `ProductComponentAssignment` | Product-component link | Id, ProductId (FK), ComponentId (FK), Quantity, CostPrice, SalePrice, DisplayOrder |
| `ComponentTemplate` | Template definitions | Id, Name, Description? |
| `ComponentTemplateItem` | Template-component link | Id, TemplateId (FK), ComponentId (FK), Quantity |

- **Rationale**: Follows existing patterns (UUID PKs, CreatedAt/UpdatedAt audit fields). Assignments carry their own prices (override pattern) independent of master. Unique constraint on (ProductId, ComponentId) in assignments and (TemplateId, ComponentId) in template items.
- **Alternative rejected**: Single denormalized table with component details embedded in product — poor data integrity and no reusability.

### 2. Override Pricing Pattern

- Each `ProductComponentAssignment` has its own CostPrice and SalePrice (mandatory for save)
- Master prices (`ProductComponent.CostPrice`/`SalePrice`) serve as defaults when creating assignments
- Changing master prices does NOT auto-update existing assignments
- User-initiated sync via "Apply master prices" button with confirmation dialog showing before/after
- **Rationale**: Prevents surprise price changes on existing products; sync is explicit and auditable.

### 3. Decimal Precision: 4 Places

- Quantity, CostPrice, SalePrice all use `decimal(18,4)` in the database
- Matches business requirement for fractional quantities (e.g., 0.5g gold) and precise pricing
- **Rationale**: Jewelry industry standard for material weights and precious metal pricing.

### 4. Frontend Integration: Subsection in Products Module

| Feature | Route | Location |
|---------|-------|----------|
| Component CRUD | `/products/components` | Products subsection |
| Template CRUD | `/products/component-templates` | Products subsection |
| Assignment UI | within `/products/{id}/edit` and `/products/create` | Product form section |
| Margin report | `/reports/product-margins` | Reports section |
| Coverage report | `/reports/products-without-components` | Reports section |

- **Rationale**: Cohesive UX — components are a product concern, not a standalone domain. Reports live in the existing Reports section alongside EP9 reports.

### 5. Admin-Only Visibility

- All component features hidden from operators via role check
- API endpoints protected with `[Authorize(Roles = "Admin")]`
- Frontend conditionally renders component sections based on user role
- **Rationale**: Components are cost/pricing data not relevant to operator workflows.

### 6. Drag-and-Drop Ordering

- `ProductComponentAssignment` uses `DisplayOrder` (int) for custom ordering
- Frontend uses `@dnd-kit/core` or similar for drag-and-drop reordering
- Order persisted on product save (not on each drag event)
- **Rationale**: Business wants to control visual component ordering per product.

### 7. Product Creation with Components

- Sequential approach: POST product first, then PUT components — avoids modifying the existing product creation endpoint contract
- Frontend handles the sequence transparently (create product → save components)
- **Rationale**: Minimal impact on existing `product-management` spec; component assignment is an orthogonal concern.

## Risks / Trade-offs

| Risk | Impact | Mitigation |
|------|--------|------------|
| Performance of real-time total calculation with many components | Low (typical: 5-15 components per product) | Calculate in frontend; API returns raw assignment data |
| Orphaned assignments when component is deactivated | Medium | Deactivated components keep existing assignments; only prevent new ones |
| Price sync could overwrite intentional overrides | Medium | Confirmation dialog with before/after preview; cancel option |
| Excel export for margin report slow for large catalogs | Low (~500 products) | Server-side pagination; export applies current filters |
| Template merge silently skips existing components | Low | UI clearly shows which components were added vs skipped |

## Migration Plan

1. Create EF migration for 4 new tables (non-destructive, additive only)
2. No existing data migration required (entirely new feature)
3. Rollback: Drop 4 new tables (no existing data affected)

## Open Questions

- None at this time — all design decisions captured from Epic 10 specification.
