## 1. Spec Updates
- [x] 1.1 Add test utilities structure requirements to frontend-testing spec
- [x] 1.2 Add query priority conventions requirement
- [x] 1.3 Add test structure patterns requirement (AAA pattern)
- [x] 1.4 Add test data factories requirement
- [x] 1.5 Validate spec changes with `openspec validate extend-frontend-testing-practices --strict`

## 2. Implementation
- [x] 2.1 Create custom render utility in `src/test/utils/render.tsx` with all required providers
- [x] 2.2 Create test data factories in `src/test/utils/test-data.ts` for Product, User, PaymentMethod entities
- [x] 2.3 Update test setup to include MSW server configuration (fail fast with 'error')
- [x] 2.4 Add query priority documentation to test utilities (included in render.tsx comments)
- [x] 2.5 Create example tests demonstrating AAA pattern (`test/__examples__/aaa-pattern-example.test.tsx`)
- [x] 2.6 Verify all existing tests follow new conventions (`test/__examples__/CONVENTIONS_VERIFICATION.md`)

