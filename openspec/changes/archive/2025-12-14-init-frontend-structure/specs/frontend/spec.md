## ADDED Requirements

### Requirement: Layout Configuration

The frontend SHALL use Metronic Layout 8 (sidebar navigation) as the primary application layout, providing consistent navigation structure across all authenticated pages.

#### Scenario: Default Layout Rendering
- **WHEN** user navigates to any authenticated route
- **THEN** Layout 8 renders with sidebar, header, and main content area
- **AND** sidebar displays project-specific navigation menu

#### Scenario: Responsive Behavior
- **WHEN** application is viewed on mobile device
- **THEN** sidebar collapses to hamburger menu
- **AND** navigation remains accessible via drawer

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

### Requirement: Sidebar Navigation Menu

The frontend SHALL display a role-based sidebar menu with icons and clear labels in Spanish, showing different navigation options based on user role.

#### Scenario: Administrator Menu Structure
- **WHEN** sidebar is visible for an Administrator user
- **THEN** menu displays: Dashboard, Inventario, Ventas, Devoluciones, Reportes, Configuración
- **AND** Configuración contains submenu: Productos, Métodos de Pago, Usuarios, Puntos de Venta
- **AND** each item has an appropriate icon

#### Scenario: Operator Menu Structure
- **WHEN** sidebar is visible for an Operator user
- **THEN** menu displays: Dashboard, Ventas, Devoluciones, Inventario
- **AND** no access to Configuración, Reportes, or administrative functions
- **AND** each item has an appropriate icon

#### Scenario: Active State
- **WHEN** user is on a specific module page
- **THEN** corresponding sidebar menu item is visually highlighted

#### Scenario: Menu Rendering Based on Auth Context
- **WHEN** user role changes or user logs in
- **THEN** sidebar menu re-renders with appropriate menu for the role

### Requirement: API Client Configuration

The frontend SHALL provide a centralized HTTP client service configured with the backend API base URL from environment variables.

#### Scenario: Environment-based Configuration
- **WHEN** application starts
- **THEN** API client reads base URL from `VITE_API_BASE_URL`
- **AND** all API requests use this base URL

#### Scenario: Request Interceptor Support
- **WHEN** API request is made
- **THEN** request passes through interceptor chain
- **AND** Authorization header can be added (when auth is implemented)

#### Scenario: Error Handling
- **WHEN** API request fails
- **THEN** error is captured by response interceptor
- **AND** appropriate error handling can be applied

### Requirement: Type Definitions

The frontend SHALL provide TypeScript type definitions for common API response patterns and shared domain types.

#### Scenario: API Response Types
- **WHEN** API response is received
- **THEN** response can be typed with generic wrapper types
- **AND** TypeScript provides autocomplete and type checking

#### Scenario: Pagination Types
- **WHEN** paginated data is requested
- **THEN** response includes typed pagination metadata
- **AND** page size is limited to 50 items maximum per project constraints

### Requirement: Environment Configuration

The frontend SHALL support environment-based configuration via `.env` files compatible with Vite.

#### Scenario: Development Environment
- **WHEN** running in development mode
- **THEN** `.env.development` values are loaded
- **AND** API points to local backend

#### Scenario: Environment Variable Prefix
- **WHEN** accessing environment variables
- **THEN** only variables prefixed with `VITE_` are exposed to client code
- **AND** sensitive values are not leaked

### Requirement: Folder Organization

The frontend SHALL maintain a consistent folder structure separating concerns: pages by module, shared services, reusable types, and context providers.

#### Scenario: Service Location
- **WHEN** creating a new API service
- **THEN** service is placed in `services/` directory
- **AND** follows naming convention `[module].service.ts`

#### Scenario: Type Location
- **WHEN** creating shared types
- **THEN** types are placed in `types/` directory
- **AND** domain-specific types are in `[domain].types.ts`

#### Scenario: Page Location
- **WHEN** creating module pages
- **THEN** pages are placed in `pages/[module]/` directory
- **AND** main page is named `page.tsx` or `index.tsx`
