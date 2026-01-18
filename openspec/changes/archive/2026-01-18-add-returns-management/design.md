# Design: Returns Management

## Context

This design covers the implementation of returns management for the jewelry POS system. Returns are complementary to sales and must maintain full traceability. The system serves administrators and operators at multiple points of sale.

**Stakeholders**: Business owner (audit/traceability), Operators (daily returns handling), Administrators (management/reporting)

**Constraints**:
- Must integrate with existing inventory-management for stock updates
- Must respect existing access-control patterns (operators restricted to assigned POS)
- Free-tier optimized (pagination, connection pooling)
- Mobile-first for operators

## Goals / Non-Goals

### Goals
- Register returns with full traceability to original sale(s)
- Automatic stock increment on return registration
- Support partial returns (return subset of sold quantity)
- Support multi-sale association (return spans multiple sales)
- 30-day return window enforcement
- Same-POS validation for returns
- Mandatory categorization for analytics
- Optional photo attachment for documentation
- Role-based access (operators at assigned POS, admins anywhere)

### Non-Goals
- Refund processing (external payment system)
- Return approval workflow (returns are immediate)
- Return shipping/logistics tracking
- Customer identity tracking
- Return reason analytics dashboard (Phase 2)

## Decisions

### Decision 1: Many-to-Many Return-Sale Relationship

**What**: Replace single `Return.SaleId` FK with `ReturnSale` junction table.

**Why**: 
- Customer may buy same product across multiple visits/sales
- Single return can span quantities from multiple sales
- Need to track which specific sales were used for return validation
- Price varies per sale (snapshot), must preserve for accurate reporting

**Alternatives considered**:
- Single SaleId (rejected: doesn't support multi-sale returns)
- Comma-separated SaleIds (rejected: poor data integrity, query complexity)

### Decision 2: Price Stored in ReturnSale Junction

**What**: Store `UnitPrice` in `ReturnSale` table, copied from `Sale.Price` at return time.

**Why**:
- Each associated sale may have different price
- Preserves historical accuracy for financial reporting
- Enables calculation of total return value without joins

**Calculation**: Total return value = SUM(ReturnSale.Quantity * ReturnSale.UnitPrice)

### Decision 3: Quantity Validation Strategy

**What**: Two-phase validation:
1. Show eligible sales with available-to-return quantities
2. Validate total return quantity <= sum of selected sales' remaining quantities

**Available-to-return calculation**:
```
For each Sale in last 30 days at same POS with same Product:
  available = Sale.Quantity - SUM(ReturnSale.Quantity WHERE SaleId = Sale.Id)
```

### Decision 4: Return Category Enum

**What**: Mandatory `ReturnCategory` enum with values:
- `Defectuoso` (Defective)
- `TamañoIncorrecto` (Wrong Size)
- `NoSatisfecho` (Not Satisfied)
- `Otro` (Other)

**Why**: Enables analytics and pattern detection without complex text parsing.

### Decision 5: Same Transaction for Return + Inventory Update

**What**: Use database transaction to atomically create Return, ReturnSale records, InventoryMovement, and update Inventory.Quantity.

**Why**: Prevents orphaned returns or stock mismatches, same pattern used in sales-management.

## Data Model Changes

### Modified Entity: Return

```
Return {
    uuid Id PK
    uuid ProductId FK
    uuid PointOfSaleId FK
    uuid UserId FK "usuario que registra la devolución"
    int Quantity "cantidad total devuelta"
    enum ReturnCategory "Defectuoso, TamañoIncorrecto, NoSatisfecho, Otro"
    string? Reason "motivo libre opcional, max 500 chars"
    datetime ReturnDate
    datetime CreatedAt
    indexed(PointOfSaleId, ReturnDate)
    indexed(ProductId, ReturnDate)
}
```

**Removed**: `SaleId FK` (replaced by ReturnSale junction)
**Added**: `Quantity`, `ReturnCategory`

### New Entity: ReturnSale

```
ReturnSale {
    uuid Id PK
    uuid ReturnId FK
    uuid SaleId FK
    int Quantity "cantidad de esta venta incluida en la devolución"
    decimal UnitPrice "precio unitario snapshot de Sale.Price"
    datetime CreatedAt
    unique(ReturnId, SaleId)
    indexed(SaleId)
}
```

### New Entity: ReturnPhoto

```
ReturnPhoto {
    uuid Id PK
    uuid ReturnId FK
    string FilePath "S3/blob path"
    string FileName
    int FileSize "bytes"
    string MimeType
    datetime CreatedAt
}
```

### New Enum: ReturnCategory

```csharp
public enum ReturnCategory
{
    Defectuoso = 1,
    TamañoIncorrecto = 2,
    NoSatisfecho = 3,
    Otro = 4
}
```

## API Design

### Endpoints

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| GET | /api/returns | List returns with filters | Admin/Operator |
| GET | /api/returns/{id} | Get return details | Admin/Operator |
| POST | /api/returns | Create return | Admin/Operator |
| GET | /api/returns/eligible-sales | Get sales eligible for return | Admin/Operator |

### Create Return Request

```json
{
  "productId": "uuid",
  "pointOfSaleId": "uuid",
  "quantity": 2,
  "returnCategory": "Defectuoso",
  "reason": "Producto rayado", // optional
  "saleAssociations": [
    { "saleId": "uuid", "quantity": 1 },
    { "saleId": "uuid", "quantity": 1 }
  ],
  "photo": "base64..." // optional
}
```

### Eligible Sales Response

```json
{
  "eligibleSales": [
    {
      "saleId": "uuid",
      "saleDate": "2026-01-10T10:30:00Z",
      "quantity": 2,
      "availableForReturn": 1, // already returned 1
      "unitPrice": 150.00,
      "paymentMethod": "Efectivo"
    }
  ],
  "totalAvailableForReturn": 3
}
```

## Risks / Trade-offs

| Risk | Mitigation |
|------|------------|
| Complex multi-sale selection UX | Clear UI showing available quantities per sale, auto-select oldest sales first (FIFO) as default |
| Concurrent return conflicts | Optimistic locking, validate available quantities at transaction commit |
| Large number of eligible sales | Paginate eligible sales (max 50), show most recent first |
| Photo storage costs | Same compression as SalePhoto (JPEG 80%, max 2MB) |

## Migration Plan

1. Create new tables: `ReturnSale`, `ReturnPhoto`
2. Add new columns to `Return`: `Quantity`, `ReturnCategory`
3. Migrate existing Return records (if any): set Quantity=1, ReturnCategory=Otro, create ReturnSale from SaleId
4. Drop `Return.SaleId` column
5. Add indexes

**Rollback**: Keep SaleId column nullable during transition, can restore if needed.

## Open Questions

None - all questions resolved in proposal discussion.
