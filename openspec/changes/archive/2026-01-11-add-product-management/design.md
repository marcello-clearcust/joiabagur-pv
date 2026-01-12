## Context

Product Management is a core feature that serves as the foundation for inventory tracking, sales registration, and AI image recognition. The system requires:
- Product catalog with SKU-based identification
- Photo management for AI training and product identification
- Excel import for bulk catalog operations
- Collection/category organization

**Stakeholders**: Administrators (catalog management), Operators (product lookup during sales)

**Constraints**:
- Free-tier cloud optimization (connection pooling, pagination)
- Mobile-first operator experience
- ~500 product catalog size

## Goals / Non-Goals

**Goals**:
- Enable bulk product import/update via Excel with UPSERT by SKU
- Provide Excel template download to prevent format errors
- Support product photos with primary photo designation
- Automatic collection creation during import
- Atomic transactions (all-or-nothing import)
- Clear validation error reporting before import confirmation (validate all rows first, show all errors)

**Non-Goals**:
- Product photo upload (covered by HU-EP1-004)
- Product CRUD UI (covered by HU-EP1-002, HU-EP1-003)
- Product search/filter (covered by HU-EP1-006)
- AI image recognition integration (covered by EP4)

## Decisions

### Decision 1: Entity Design
- **What**: Product, ProductPhoto, Collection as separate entities with BaseEntity inheritance
- **Why**: Follows existing domain patterns, enables independent photo management

### Decision 2: File Storage Strategy Pattern
- **What**: IFileStorageService with LocalFileStorageService (dev) and CloudFileStorageService (prod) implementations
- **Why**: Enables local development without cloud costs, seamless production deployment

### Decision 3: Excel Processing Library
- **What**: Use ClosedXML (already in project dependencies)
- **Why**: Lightweight, well-documented, supports .xlsx and .xls formats

### Decision 4: UPSERT Strategy
- **What**: Load all existing SKUs, compare in-memory, perform batch insert/update
- **Why**: Minimizes database roundtrips, efficient for ~500 product scale

### Decision 5: Transaction Handling
- **What**: Wrap entire import in database transaction with rollback on any error
- **Why**: Ensures data consistency, aligns with user story requirement for atomicity

### Decision 6: Excel Template Download
- **What**: Provide downloadable Excel template with exact column names (SKU, Name, Description, Price, Collection) to prevent format errors
- **Why**: Reduces user errors, ensures consistent format, improves user experience
- **Implementation**: Backend endpoint generates template file, frontend provides download button
- **Consistency**: Same pattern applies to Inventory Management Excel import

### Decision 7: Shared Excel Template Utility
- **What**: Create shared `IExcelTemplateService` interface and implementation for consistent template generation across Product and Inventory imports
- **Why**: DRY principle, ensures consistent formatting, easier maintenance, single place to update template logic
- **Implementation**: Shared service in Application layer, used by both ProductImportService and StockImportService
- **Benefits**: Consistent header formatting, data validation rules, example data patterns

### Decision 8: Template Quality and Formatting
- **What**: Excel templates include data validation rules, protected header row, proper formatting (bold headers, number formats), and instructions
- **Why**: Reduces user errors, improves UX, guides users on correct format
- **Implementation**: Use ClosedXML features for data validation, cell protection, and formatting

### Alternatives Considered
- **Individual row processing**: Rejected - too many DB roundtrips
- **Bulk copy operations**: Rejected - PostgreSQL COPY not well-suited for UPSERT logic
- **Background job processing**: Rejected - overkill for ~500 products, adds complexity

## Risks / Trade-offs

| Risk | Mitigation |
|------|------------|
| Large Excel files may timeout | Implement file size limit (10MB), validate row count before processing |
| Memory pressure from loading all products | Pagination not needed for 500 products; can add streaming if scale increases |
| Collection name conflicts | Case-insensitive matching when checking for existing collections |

## Migration Plan

1. Run database migration to create Product, ProductPhoto, Collection tables
2. Deploy API with new endpoints
3. Deploy frontend import page
4. No data migration needed (new feature)

**Rollback**: Drop new tables, redeploy previous API/frontend versions

## Open Questions

None - requirements are clear from user story and tickets.

