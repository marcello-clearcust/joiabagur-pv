# Change: Add Jewelry Component Management (EP10)

## Why
The jewelry business needs to track materials, labor, and other components that constitute each product in order to calculate production costs, suggested sale prices, and analyze profit margins. Currently there is no way to break down product costs by component, making margin analysis and pricing decisions manual and error-prone.

## What Changes
- **NEW** `component-master` capability: Master table for component definitions (description, optional cost/sale prices, activate/deactivate)
- **NEW** `component-assignment` capability: Assign components to products with quantities and price overrides, real-time totals, drag-and-drop ordering, price sync from master, and price deviation warnings — integrated into product create/edit pages (admin-only)
- **NEW** `component-templates` capability: Reusable component templates with quantities for quick product setup via merge logic
- **NEW** `component-reports` capability: Margin analysis report and products-without-components report with filtering, aggregation, and Excel export
- **NEW** 4 database entities: `ProductComponent`, `ProductComponentAssignment`, `ComponentTemplate`, `ComponentTemplateItem`
- **NEW** Backend services, controllers, and API endpoints for all CRUD and reporting operations
- **NEW** Frontend pages/sections integrated into the Products and Reports modules

## Impact
- Affected specs: `component-master` (new), `component-assignment` (new), `component-templates` (new), `component-reports` (new)
- Affected code:
  - Backend: 4 new entities, EF migrations, repositories, services, controllers (API layer)
  - Frontend: New subsections in Products module, new report pages in Reports module
  - Database: 4 new tables (additive, non-destructive)
- User Stories: HU-EP10-001 through HU-EP10-008
- Dependencies: EP7 (auth/roles), EP1 (product management), EP9 (reports section)
