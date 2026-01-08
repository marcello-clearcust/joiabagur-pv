# Change: Extend Frontend Testing Practices

## Why

The existing `frontend-testing` spec establishes basic testing infrastructure, but lacks detailed requirements for test utilities, test data factories, query priority conventions, and structured test patterns. The comprehensive testing guide (`Documentos/testing-frontend.md`) provides detailed practices that should be formalized as requirements to ensure consistent testing implementation across the frontend codebase.

## What Changes

- Add requirements for test utilities structure (custom render with providers, test data factories)
- Formalize query priority conventions as a requirement
- Add requirements for test structure patterns (AAA pattern)
- Add requirements for test documentation organization
- Extend test conventions with detailed patterns and examples

## Impact

- Affected specs: `frontend-testing` (extended capability)
- Affected code:
  - `frontend/src/test/utils/` - test utilities structure
  - `frontend/src/test/utils/test-data.ts` - test data factories
  - `frontend/src/test/utils/render.tsx` - custom render utility
  - `Documentos/Testing/Frontend/` - testing documentation structure

