# Change: Initialize Frontend Structure with Metronic Template

## Why

The frontend currently contains the raw Metronic template with 39 demo layouts, demo pages, and demo sidebar menus. This proposal establishes the project-specific structure based on the technical analysis documented in `Documentos/Propuestas/analisis-metronic-frontend.md`, setting up Layout 8 (sidebar navigation) as the foundation and creating the folder structure for module development.

## What Changes

- Configure Layout 8 as the default layout with project-specific sidebar menu
- Replace demo routing with project-structured routes organized by epic
- Create folder structure for `services/`, `providers/`, `types/`, and `pages/` by module
- Configure environment variables for backend API connection
- Set up base HTTP client service with interceptor support
- Create placeholder pages for dashboard and modules
- Remove/cleanup unused demo layouts and pages (keeping only Layout 8 and UI components)

## Impact

- Affected specs: Creates new `frontend` capability spec
- Affected code:
  - `frontend/src/routing/app-routing-setup.tsx` - Complete replacement
  - `frontend/src/config/layout-8.config.tsx` - Menu configuration
  - `frontend/src/` - New folders: `services/`, `providers/`, `types/`, `pages/auth/`, `pages/dashboard/`
  - `frontend/.env.example` - New file for environment variables
- Dependencies: None (foundational change)
- Related changes: Prepares structure for `add-authentication-user-management` frontend implementation
