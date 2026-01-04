## Context

The Metronic React template has been integrated into `frontend/src/` with all 39 demo layouts. According to `openspec/project.md`, the project uses:
- **Layout 8** (sidebar navigation) as the primary layout
- **React 19** + **TypeScript** + **Vite**
- Module organization aligned with epics (Auth, Products, Inventory, Sales, etc.)

The Metronic analysis (`Documentos/Propuestas/analisis-metronic-frontend.md`) confirms Layout 8 is suitable and all necessary UI components are available.

## Goals / Non-Goals

### Goals
- Establish a clean, maintainable frontend structure ready for feature development
- Configure Layout 8 with project-specific sidebar menu (organized by epic)
- Create consistent folder organization for services, types, and providers
- Set up base HTTP client for backend API communication
- Remove demo clutter while preserving reusable Metronic components

### Non-Goals
- Implementing authentication (covered by `add-authentication-user-management`)
- Creating actual business pages (each epic will handle their own pages)
- Adding ML frameworks for image recognition (covered by EP4)
- Removing all unused layout folders (only routing/config cleanup; source layouts preserved for reference)

## Decisions

### Decision 1: Use Layout 8 as Sole Application Layout
**Why**: Layout 8 provides sidebar navigation which is optimal for admin dashboards and matches the project requirements. Other layouts are demo alternatives.

### Decision 2: Organize Pages by Epic/Module
**Structure**:
```
pages/
├── auth/           # EP7 - Login page (public)
├── dashboard/      # Main dashboard (protected)
├── products/       # EP1
├── inventory/      # EP2
├── sales/          # EP3
├── returns/        # EP5
├── payment-methods/# EP6
├── users/          # EP7 - User management
├── points-of-sale/ # EP8
└── reports/        # EP9
```
**Why**: Aligns with epic structure and makes navigation intuitive for developers.

### Decision 3: Centralized API Service with Axios
**Why**: Axios provides interceptors for JWT token injection, error handling, and request/response transformation. This matches the backend JWT authentication strategy.

### Decision 4: Role-Based Sidebar Navigation
**Administrator menu**:
- Dashboard
- Inventario
- Ventas
- Devoluciones
- Reportes
- Configuración (submenu: Productos, Métodos de Pago, Usuarios, Puntos de Venta)

**Operator menu**:
- Dashboard
- Ventas
- Devoluciones
- Inventario

**Why**: Operators only need access to sales-related functions at their assigned point of sale. Administrators have full access including configuration and reports. This aligns with RBAC requirements from EP7.

**Implementation**: A `useMenu` hook will read user role from auth context and return the appropriate menu configuration.

### Decision 5: Keep UI Components, Remove Demo Pages
**What to keep**: `components/ui/`, `components/layouts/layout-8/`, `hooks/`, `lib/`
**What to remove**: Routing for layouts 1-7, 9-39; demo page components
**Why**: Preserves all reusable UI components while eliminating demo navigation confusion.

### Decision 6: Environment-based Configuration
**Variables**:
- `VITE_API_BASE_URL` - Backend API URL
- `VITE_APP_NAME` - Application name for branding
**Why**: Vite's built-in env variable support with `VITE_` prefix.

## Risks / Trade-offs

### Risk: Breaking Template Updates
**Impact**: Future Metronic updates may conflict with customizations
**Mitigation**: Keep original files as reference, document all customizations, avoid modifying core UI components

### Risk: Large Initial Cleanup
**Impact**: Removing demo code could accidentally remove needed components
**Mitigation**: Only modify routing and config files; preserve all component folders initially

## Migration Plan

1. **Phase 1**: Create new folders and base files (non-breaking)
2. **Phase 2**: Update routing to use Layout 8 only with project routes
3. **Phase 3**: Update sidebar menu configuration
4. **Phase 4**: Clean up demo page imports from routing

No rollback needed as this is initial setup; git provides version history.

## Open Questions

- None at this stage; the Metronic analysis document provides comprehensive guidance.
