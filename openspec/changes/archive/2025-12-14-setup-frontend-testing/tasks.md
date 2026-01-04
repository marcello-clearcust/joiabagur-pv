# Tasks: Setup Frontend Testing Infrastructure

## 1. Setup Initial - Install Dependencies and Configure Vitest

- [x] 1.1 Install Vitest and core testing dependencies
  - `vitest`, `@testing-library/react`, `@testing-library/jest-dom`, `@testing-library/user-event`, `jsdom`
- [x] 1.2 Configure Vitest in `vite.config.ts`
  - Add test configuration with jsdom environment
  - Configure setup files and coverage settings
  - Set include patterns for test files
- [x] 1.3 Create test setup file `src/test/setup.ts`
  - Import `@testing-library/jest-dom` matchers
  - Configure automatic cleanup after each test
- [x] 1.4 Add TypeScript types for Vitest globals
  - Update `tsconfig.json` or add reference in setup file
- [x] 1.5 Add test scripts to `package.json`
  - `test`, `test:watch`, `test:coverage`, `test:ui`

## 2. Test Infrastructure - Create Shared Test Utilities

- [x] 2.1 Create folder structure `src/test/`
  - `setup.ts`, `mocks/`, `utils/`, `__fixtures__/`
- [x] 2.2 Create custom render utility `src/test/utils/render.tsx`
  - Wrap with AuthProvider and other providers
  - Export custom render and re-export from @testing-library/react
- [x] 2.3 Create test data factories `src/test/utils/test-data.ts`
  - Factories for User, Product, Sale, etc.

## 3. MSW Setup - API Mocking Infrastructure

- [x] 3.1 Install MSW (Mock Service Worker)
  - `msw`
- [x] 3.2 Create MSW server setup `src/test/mocks/server.ts`
  - Configure test server for Vitest
- [x] 3.3 Create base API handlers `src/test/mocks/handlers.ts`
  - Mock auth endpoints (login, logout, me)
  - Mock users endpoint for initial coverage
- [x] 3.4 Integrate MSW with test setup
  - Start server before all tests
  - Reset handlers after each test
  - Close server after all tests

## 4. Initial Unit Tests - Hooks and Utilities

- [x] 4.1 Create tests for utility functions
  - `src/lib/utils.test.ts` - test cn() helper
  - `src/lib/helpers.test.ts` - test helper functions
- [x] 4.2 Create tests for custom hooks
  - `src/hooks/use-copy-to-clipboard.test.ts`

## 5. Initial Component Tests - UI Components

- [x] 5.1 Create test for Button component
  - `src/components/ui/button.test.tsx`
  - Test variants, sizes, disabled state, click handler
- [x] 5.2 Create test for Input component
  - `src/components/ui/input.test.tsx`
  - Test focus, change, placeholder, disabled state

## 6. Playwright E2E Setup

- [x] 6.1 Install Playwright
  - `@playwright/test`
- [x] 6.2 Create Playwright configuration `playwright.config.ts`
  - Configure base URL, browsers, timeout
  - Set up HTML reporter
- [x] 6.3 Create E2E folder structure `e2e/`
  - `app.spec.ts`, `fixtures/`
- [x] 6.4 Create initial E2E test
  - Basic app loading and navigation tests
- [x] 6.5 Add E2E scripts to `package.json`
  - `test:e2e`, `test:e2e:ui`, `test:e2e:headed`

## 7. CI/CD Integration - GitHub Actions

- [x] 7.1 Create GitHub Actions workflow `.github/workflows/test-frontend.yml`
  - Install dependencies with caching
  - Run unit/component tests with coverage
  - Upload coverage report as artifact
- [x] 7.2 Configure E2E tests in CI
  - Playwright with Chromium
  - Store test artifacts

## 8. Documentation and Final Verification

- [x] 8.1 Create README with testing instructions
  - How to run tests locally
  - Coverage thresholds
- [x] 8.2 Verify all tests pass
  - Run `npm run test` - 61 tests passing
  - Run `npm run test:coverage` - Coverage thresholds met
- [x] 8.3 Verify CI workflow configuration is complete

---

## Dependencies

- Phase 1 (Setup) must be completed before Phase 2-5
- Phase 3 (MSW) can run in parallel with Phase 4-5 but is required for component tests that call APIs
- Phase 6 (Playwright) is independent and can run in parallel with Phases 3-5
- Phase 7 (CI) requires Phases 1-5 to be complete for unit tests
- Phase 8 (Documentation) should be done last

## Validation Criteria

- [x] All test scripts work (`npm run test`, `npm run test:watch`, `npm run test:coverage`)
- [x] 61 unit tests pass for utilities/hooks/components
- [x] Playwright config exists and E2E tests are ready
- [x] Coverage report generates successfully (81.57% statements, 66.66% branches, 79.16% functions, 81.08% lines)
- [x] GitHub Actions workflow created for CI testing
