# Change: Add Returns Management (EP5)

## Why

The jewelry business needs to handle product returns from customers, maintaining traceability with original sales, updating inventory automatically, and providing audit capabilities. This is Epic 5 of the MVP scope, a complement to the sales management functionality.

## What Changes

- **NEW** `returns-management` capability with full return lifecycle support
- **BREAKING** Data model changes to `Return` entity (add fields, change relationship)
- **NEW** `ReturnSale` junction table for many-to-many Return-Sale relationship
- **NEW** `ReturnPhoto` entity for optional return photos
- **NEW** `ReturnCategory` enum for mandatory categorization
- **MODIFIED** `InventoryMovement` integration for automatic stock updates on return
- Backend: New ReturnsController, ReturnService, DTOs, validators
- Frontend: Return registration form, sale search/selection, return history view

## Impact

- **Affected specs**: 
  - `returns-management` (NEW)
  - `inventory-management` (integration for movement creation)
  - `sales-management` (sale search for return association)
  - `access-control` (operator POS restrictions)
- **Affected code**:
  - `backend/src/Domain/Entities/Return.cs` (modified)
  - `backend/src/Domain/Entities/ReturnSale.cs` (new)
  - `backend/src/Domain/Entities/ReturnPhoto.cs` (new)
  - `backend/src/Domain/Enums/ReturnCategory.cs` (new)
  - `backend/src/Infrastructure/Data/ApplicationDbContext.cs` (new configurations)
  - `backend/src/Infrastructure/Data/Migrations/` (new migration)
  - `backend/src/Application/Services/ReturnService.cs` (new)
  - `backend/src/Api/Controllers/ReturnsController.cs` (new)
  - `frontend/src/modules/returns/` (new module)
- **Affected documentation**:
  - `Documentos/modelo-de-datos.md`
  - `Documentos/Historias/HU-EP5-*.md`

## Key Design Decisions

1. **Many-to-many Return-Sale relationship**: A single return can be associated with multiple sales (e.g., customer bought 2 units on Monday, 3 units on Tuesday, returns 4 units total selecting from both sales)
2. **30-day return window**: Returns are only valid for sales within the last 30 days
3. **Same POS requirement**: Returns must be registered at the same point of sale where the original sale(s) occurred
4. **Mandatory category + optional reason**: Return category is required (Defectuoso, TamañoIncorrecto, NoSatisfecho, Otro), free-text reason is optional
5. **User-selected sale association**: System shows eligible sales, user explicitly chooses which sale(s) to associate with the return
6. **Price from sales**: Return value is calculated from associated sales' price snapshots, stored in ReturnSale junction table
