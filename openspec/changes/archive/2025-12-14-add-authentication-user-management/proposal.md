# Change: Add Authentication and User Management

## Why

The system requires authentication and user management capabilities to secure access and enable role-based operations. Operators need to access only their assigned points of sale, while administrators require full system access. This is the foundational Epic (EP7) that must be implemented before all other business features.

## What Changes

- **NEW** `auth` capability: JWT-based authentication with login, token refresh, and session management
- **NEW** `user-management` capability: User CRUD operations for Admin role, password security
- **NEW** `access-control` capability: Role-based access control (RBAC), point-of-sale assignment authorization

### Breaking Changes

None - this is new functionality for the system foundation.

## Impact

- **Affected specs:** 
  - `backend` (existing) - will leverage existing Password Security and Input Validation requirements
  - `auth` (new capability)
  - `user-management` (new capability)  
  - `access-control` (new capability)

- **Affected code:**
  - Backend: Domain layer (entities, services), Infrastructure (EF Core, repositories), API (controllers, middleware)
  - Frontend: Auth module, User module, HTTP interceptors, route guards
  - Database: User, UserPointOfSale tables with migrations

## User Stories Covered

- HU-EP7-001: Login with username and password
- HU-EP7-002: Create new user
- HU-EP7-003: Edit existing user
- HU-EP7-004: Assign operator to point of sale
- HU-EP7-005: Unassign operator from point of sale
- HU-EP7-006: Access control based on role and assignments

## Dependencies

- Requires `PointOfSale` entity to exist (EP8) for user assignments
- Password hashing already specified in `backend` spec (BCrypt)
- Input validation already specified in `backend` spec (FluentValidation)

## Notes

This is the first Epic in the implementation order. While it depends on EP8 (Points of Sale) for assignments, the auth and user CRUD can be implemented first with assignments added once PointOfSale entity exists.
