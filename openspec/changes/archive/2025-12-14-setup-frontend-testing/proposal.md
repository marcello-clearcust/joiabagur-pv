# Change: Setup Frontend Testing Infrastructure

## Why

The frontend currently has no testing infrastructure in place. To ensure code quality, prevent regressions, and enable confident refactoring, we need to establish a comprehensive testing setup following the project's defined testing stack (Vitest, React Testing Library, MSW, Playwright).

## What Changes

- Install testing dependencies (Vitest, React Testing Library, MSW, Playwright, jsdom)
- Configure Vitest in `vite.config.ts` with jsdom environment
- Create test setup file with React Testing Library matchers
- Create test infrastructure (custom render, MSW handlers, test utilities)
- Configure Playwright for E2E testing
- Add test scripts to `package.json`
- Create initial example tests for existing components and hooks
- Set up GitHub Actions workflow for CI testing

## Impact

- Affected specs: `frontend-testing` (new capability)
- Affected code:
  - `frontend/package.json` - new dependencies and scripts
  - `frontend/vite.config.ts` - Vitest configuration
  - `frontend/src/test/` - new test infrastructure folder
  - `frontend/e2e/` - E2E test folder
  - `frontend/playwright.config.ts` - Playwright configuration
  - `.github/workflows/` - CI workflow for tests
