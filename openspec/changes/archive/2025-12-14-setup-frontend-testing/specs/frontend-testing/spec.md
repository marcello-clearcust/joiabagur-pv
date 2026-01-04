# Frontend Testing Specification

## ADDED Requirements

### Requirement: Unit Test Infrastructure

The frontend SHALL provide a unit testing infrastructure using Vitest that enables developers to write and run isolated tests for utilities, hooks, and pure functions.

#### Scenario: Run unit tests

- **WHEN** a developer executes `npm run test`
- **THEN** Vitest runs all test files matching `**/*.{test,spec}.{ts,tsx}`
- **AND** results are displayed in the terminal with pass/fail status

#### Scenario: Watch mode for development

- **WHEN** a developer executes `npm run test:watch`
- **THEN** Vitest runs in watch mode
- **AND** re-runs affected tests when source files change

#### Scenario: Coverage reporting

- **WHEN** a developer executes `npm run test:coverage`
- **THEN** Vitest generates a coverage report
- **AND** reports are available in text, HTML, and lcov formats
- **AND** the minimum coverage target is 70%

---

### Requirement: Component Test Infrastructure

The frontend SHALL provide component testing capabilities using React Testing Library that enables testing React components with user-centric queries and interactions.

#### Scenario: Render components with providers

- **WHEN** a test uses the custom `render()` utility
- **THEN** the component is wrapped with all required providers (AuthProvider, etc.)
- **AND** the test can query elements using accessible queries (getByRole, getByLabelText)

#### Scenario: Simulate user interactions

- **WHEN** a test uses `userEvent` from @testing-library/user-event
- **THEN** user interactions (click, type, select) behave like real user actions
- **AND** async state updates are properly awaited

#### Scenario: Test form validation

- **WHEN** a test submits a form with React Hook Form and Zod validation
- **THEN** validation errors are accessible via screen queries
- **AND** the test can verify form submission behavior

---

### Requirement: API Mocking Infrastructure

The frontend SHALL provide API mocking capabilities using MSW (Mock Service Worker) that enables testing components that depend on API calls without hitting real endpoints.

#### Scenario: Mock API responses

- **WHEN** a component makes an API request during a test
- **THEN** MSW intercepts the request
- **AND** returns a mocked response defined in handlers

#### Scenario: Test error scenarios

- **WHEN** a test needs to verify error handling behavior
- **THEN** the test can override handlers to return error responses (401, 500, etc.)
- **AND** error states are properly rendered

#### Scenario: Reset handlers between tests

- **WHEN** a test modifies handlers for a specific scenario
- **THEN** handlers are reset to default state after the test completes
- **AND** subsequent tests are not affected

---

### Requirement: End-to-End Test Infrastructure

The frontend SHALL provide E2E testing capabilities using Playwright that enables testing complete user flows across multiple pages and interactions.

#### Scenario: Run E2E tests

- **WHEN** a developer executes `npm run test:e2e`
- **THEN** Playwright runs all E2E tests in `e2e/` directory
- **AND** tests run in Chromium browser by default

#### Scenario: E2E with UI mode

- **WHEN** a developer executes `npm run test:e2e:ui`
- **THEN** Playwright opens interactive UI mode
- **AND** developers can step through tests and debug failures

#### Scenario: Multi-browser testing

- **WHEN** E2E tests run in CI
- **THEN** tests can be configured to run in Chromium, Firefox, and WebKit
- **AND** failures in any browser are reported

---

### Requirement: Test Conventions

The frontend testing infrastructure SHALL enforce consistent conventions for test organization and naming.

#### Scenario: Colocated test files

- **WHEN** a developer creates a test for a component
- **THEN** the test file is placed next to the source file
- **AND** the test file uses the naming pattern `{name}.test.{ts,tsx}`

#### Scenario: Descriptive test names

- **WHEN** a developer writes a test
- **THEN** the test description follows the pattern `should [behavior] when [condition]`
- **AND** tests are organized in `describe` blocks by component/function name

#### Scenario: Accessible queries prioritized

- **WHEN** a component test queries the DOM
- **THEN** accessible queries (getByRole, getByLabelText) are preferred
- **AND** getByTestId is used only as a last resort

---

### Requirement: CI Test Integration

The frontend testing infrastructure SHALL integrate with GitHub Actions to run tests automatically on pull requests.

#### Scenario: Unit tests in CI

- **WHEN** a pull request is opened or updated
- **THEN** the CI workflow installs dependencies
- **AND** runs unit and component tests
- **AND** generates coverage report

#### Scenario: Cache dependencies

- **WHEN** the CI workflow runs
- **THEN** npm dependencies are cached between runs
- **AND** subsequent runs are faster

#### Scenario: Report test failures

- **WHEN** any test fails in CI
- **THEN** the workflow fails
- **AND** failure details are visible in the PR checks
