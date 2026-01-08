## MODIFIED Requirements

### Requirement: Component Test Infrastructure

The frontend SHALL provide component testing capabilities using React Testing Library that enables testing React components with user-centric queries and interactions.

#### Scenario: Render components with providers

- **WHEN** a test uses the custom `render()` utility from `src/test/utils/render.tsx`
- **THEN** the component is wrapped with all required providers (AuthProvider, Router, ThemeProvider, etc.)
- **AND** the test can query elements using accessible queries (getByRole, getByLabelText)
- **AND** the custom render utility exports a function that extends React Testing Library's render with provider wrappers

#### Scenario: Simulate user interactions

- **WHEN** a test uses `userEvent` from @testing-library/user-event
- **THEN** user interactions (click, type, select) behave like real user actions
- **AND** async state updates are properly awaited
- **AND** userEvent is initialized with `userEvent.setup()` before interactions

#### Scenario: Test form validation

- **WHEN** a test submits a form with React Hook Form and Zod validation
- **THEN** validation errors are accessible via screen queries
- **AND** the test can verify form submission behavior
- **AND** form validation errors are tested using accessible queries (getByRole with error role or getByText for error messages)

---

### Requirement: Test Conventions

The frontend testing infrastructure SHALL enforce consistent conventions for test organization, naming, query priority, and structure patterns.

#### Scenario: Colocated test files

- **WHEN** a developer creates a test for a component, hook, or utility
- **THEN** the test file is placed next to the source file
- **AND** the test file uses the naming pattern `{name}.test.{ts,tsx}`
- **AND** test files are excluded from production builds via Vitest configuration

#### Scenario: Descriptive test names

- **WHEN** a developer writes a test
- **THEN** the test description follows the pattern `should [behavior] when [condition]`
- **AND** tests are organized in `describe` blocks by component/function name
- **AND** test descriptions clearly express the expected behavior and conditions

#### Scenario: Accessible queries prioritized

- **WHEN** a component test queries the DOM
- **THEN** queries follow the priority order: getByRole, getByLabelText, getByPlaceholderText, getByText, getByDisplayValue, getByAltText, getByTitle, getByTestId
- **AND** getByTestId is used only as a last resort when no accessible query is available
- **AND** tests prioritize queries that reflect how users interact with the application

#### Scenario: AAA test structure pattern

- **WHEN** a developer writes a test
- **THEN** the test follows the Arrange-Act-Assert (AAA) pattern
- **AND** the Arrange section prepares test data and renders components
- **AND** the Act section executes user interactions or function calls
- **AND** the Assert section verifies expected outcomes
- **AND** comments clearly separate each section when needed for readability

---

## ADDED Requirements

### Requirement: Test Utilities Structure

The frontend SHALL provide a structured test utilities directory (`src/test/utils/`) that contains reusable testing helpers and utilities.

#### Scenario: Custom render utility

- **WHEN** a test imports the custom render utility from `src/test/utils/render.tsx`
- **THEN** the utility wraps React Testing Library's render function
- **AND** automatically provides all required context providers (AuthProvider, Router, ThemeProvider)
- **AND** accepts optional overrides for provider values
- **AND** returns the same API as React Testing Library's render function

#### Scenario: Test data factories

- **WHEN** a test needs test data for entities (Product, User, Sale, etc.)
- **THEN** the test imports factory functions from `src/test/utils/test-data.ts`
- **AND** factory functions accept optional overrides to customize generated data
- **AND** factory functions generate realistic test data with required fields populated
- **AND** factories support builder patterns for complex test scenarios

#### Scenario: Test utilities organization

- **WHEN** a developer needs test utilities
- **THEN** utilities are organized in `src/test/utils/` directory
- **AND** `render.tsx` contains the custom render function
- **AND** `test-data.ts` contains factory functions for test data generation
- **AND** `index.ts` exports all utilities for convenient importing

---

### Requirement: Test Documentation Structure

The frontend testing infrastructure SHALL maintain comprehensive documentation organized in `Documentos/Testing/Frontend/` that guides developers through testing practices.

#### Scenario: Configuration documentation

- **WHEN** a developer needs to set up the testing environment
- **THEN** documentation exists at `Documentos/Testing/Frontend/01-configuracion.md`
- **AND** the documentation covers stack selection, installation, Vitest configuration, Playwright configuration, and project structure

#### Scenario: Testing guides by type

- **WHEN** a developer needs guidance on writing specific types of tests
- **THEN** documentation exists for unit tests (`02-tests-unitarios.md`), component tests (`03-tests-componentes.md`), API mocking (`04-mocking-api.md`), and E2E tests (`05-tests-e2e.md`)
- **AND** each guide includes examples, patterns, and best practices

#### Scenario: CI/CD and coverage documentation

- **WHEN** a developer needs to configure CI/CD or understand coverage requirements
- **THEN** documentation exists for GitHub Actions (`06-github-actions.md`) and code coverage (`07-cobertura-codigo.md`)
- **AND** the documentation covers workflow configuration, caching strategies, and coverage thresholds

#### Scenario: Main testing guide reference

- **WHEN** a developer needs an overview of the testing strategy
- **THEN** the main guide exists at `Documentos/testing-frontend.md`
- **AND** the guide provides quick start instructions, documentation index, project structure, implementation checklist, and conventions
- **AND** the guide references detailed guides in `Documentos/Testing/Frontend/`

---

### Requirement: MSW Server Configuration

The frontend SHALL configure MSW (Mock Service Worker) server in the test setup to enable API mocking across all tests.

#### Scenario: MSW server initialization

- **WHEN** tests run
- **THEN** the MSW server is initialized in `src/test/setup.ts` using `server.listen()`
- **AND** unhandled requests are configured to warn developers (fail fast for integration tests)
- **AND** handlers are reset between tests using `server.resetHandlers()`
- **AND** the server is closed after all tests complete using `server.close()`

#### Scenario: MSW handlers organization

- **WHEN** a developer needs to add or modify API mocks
- **THEN** handlers are organized in `src/test/mocks/handlers.ts`
- **AND** handlers are grouped by API resource (products, users, sales, etc.)
- **AND** handlers can be overridden per test using `server.use()` for specific scenarios

#### Scenario: MSW server export

- **WHEN** tests need to access the MSW server
- **THEN** the server is exported from `src/test/mocks/server.ts`
- **AND** the server configuration includes default handlers from `handlers.ts`
- **AND** the server can be imported and used in individual tests when needed

