## 1. Frontend - Product Creation Form

- [x] 1.1 Create ProductCreatePage component with Metronic layout and form structure (T-EP1-002-002)
- [x] 1.2 Add form fields: SKU (text), Name (text), Description (textarea), Price (number), Collection (dropdown) (T-EP1-002-002)
- [x] 1.3 Integrate React Hook Form with Zod validation schema (T-EP1-002-002)
- [x] 1.4 Add SKU uniqueness validation with async API check (T-EP1-002-002)
- [x] 1.5 Add collection dropdown populated from collections API (T-EP1-002-002)
- [x] 1.6 Add price input with number formatting and positive value validation (T-EP1-002-002)
- [x] 1.7 Add form submission with loading state and error handling (T-EP1-002-002)
- [x] 1.8 Add success feedback with options to create another or navigate to list (T-EP1-002-002)
- [x] 1.9 Add route /products/create and navigation menu entry (T-EP1-002-002)

## 2. Backend - Integration Tests

- [x] 2.1 Create ProductCreationTests class with WebApplicationFactory and Testcontainers setup (T-EP1-002-003)
- [x] 2.2 Test successful product creation with all fields (T-EP1-002-003)
- [x] 2.3 Test validation errors for missing required fields (SKU, Name, Price) (T-EP1-002-003)
- [x] 2.4 Test SKU uniqueness constraint returns 409 Conflict (T-EP1-002-003)
- [x] 2.5 Test price validation (must be > 0) (T-EP1-002-003)
- [x] 2.6 Test collection assignment (existing and non-existing collection) (T-EP1-002-003)
- [x] 2.7 Test authentication (401 for unauthenticated) and authorization (403 for Operator role) (T-EP1-002-003)
- [x] 2.8 Verify database state after creation (IsActive, CreatedAt) (T-EP1-002-003)

## 3. Frontend - Component Tests

- [x] 3.1 Set up test file with MSW handlers for products and collections APIs (T-EP1-002-004)
- [x] 3.2 Test form renders with all fields and labels (T-EP1-002-004)
- [x] 3.3 Test required field validation (SKU, Name, Price) (T-EP1-002-004)
- [x] 3.4 Test SKU uniqueness validation with mock API response (T-EP1-002-004)
- [x] 3.5 Test price validation (positive number required) (T-EP1-002-004)
- [x] 3.6 Test collection dropdown populates from API (T-EP1-002-004)
- [x] 3.7 Test successful form submission and success feedback (T-EP1-002-004)
- [x] 3.8 Test API error handling and display (T-EP1-002-004)
- [x] 3.9 Test loading states during submission (T-EP1-002-004)

