## MODIFIED Requirements

### Requirement: Project Routing Structure

The frontend SHALL implement a modular routing structure organized by business domain (epic), with lazy loading for optimal bundle size.

#### Scenario: Route Organization
- **WHEN** navigating to a module
- **THEN** URL reflects the module path (e.g., `/products`, `/inventory`, `/sales`)
- **AND** appropriate page component is rendered within Layout 8

#### Scenario: Default Route
- **WHEN** user navigates to root path `/`
- **THEN** user is redirected to `/dashboard`

#### Scenario: Lazy Loading
- **WHEN** page component is requested
- **THEN** component is loaded on demand
- **AND** initial bundle size remains under 500KB

#### Scenario: Dashboard role bifurcation
- **WHEN** authenticated user navigates to `/dashboard`
- **THEN** the dashboard page component determines user role from auth context
- **AND** renders AdminDashboard for Administrator role
- **AND** renders OperatorDashboard for Operator role

## ADDED Requirements

### Requirement: Operator Dashboard Layout

The frontend SHALL provide an OperatorLayout wrapper for the operator dashboard that hides the Metronic sidebar and displays a sticky bottom action bar, optimizing screen real estate for mobile/tablet POS use.

#### Scenario: Sidebar hidden on operator dashboard
- **WHEN** authenticated operator views the dashboard
- **THEN** the Layout 8 sidebar is hidden or collapsed
- **AND** the full viewport width is available for dashboard content

#### Scenario: Sidebar visible on other operator pages
- **WHEN** authenticated operator navigates away from the dashboard
- **THEN** the Layout 8 sidebar returns to its normal behavior
- **AND** standard navigation is available

#### Scenario: Admin layout unaffected
- **WHEN** authenticated administrator views the dashboard
- **THEN** the Layout 8 sidebar remains visible in its standard configuration
- **AND** no OperatorLayout wrapper is applied

### Requirement: Dashboard Chart Components

The frontend SHALL use Recharts for rendering line, bar, and donut chart components on the dashboard, keeping bundle impact minimal through lazy loading.

#### Scenario: Chart library loaded on demand
- **WHEN** user navigates to the dashboard
- **THEN** Recharts components are loaded as part of the lazy-loaded dashboard page
- **AND** chart library does not increase the initial bundle size

#### Scenario: Charts display with Euro formatting
- **WHEN** charts display monetary values on axes or tooltips
- **THEN** values are formatted using Intl.NumberFormat with locale 'es-ES' and currency 'EUR'
