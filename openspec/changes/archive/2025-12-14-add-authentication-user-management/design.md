# Design: Authentication and User Management

## Context

This system manages jewelry sales across multiple points of sale. Security is critical to protect sales and inventory data. The system has two user roles:

- **Administrator**: Full system access, manages users and configuration
- **Operator**: Restricted to assigned points of sale, can register sales and view inventory

The authentication system must be:
- Secure (JWT with proper token management)
- Mobile-friendly (operators work on mobile devices)
- Simple (small scale: 2-3 concurrent users, ~10 total users)

## Goals / Non-Goals

### Goals
- Secure authentication with JWT tokens
- Role-based authorization (Admin vs Operator)
- Point-of-sale-based data filtering for operators
- Session persistence (8-hour token expiry for work shifts)
- Rate limiting for login attempts

### Non-Goals
- OAuth/social login (not needed for internal system)
- Multi-factor authentication (Phase 2 consideration)
- Password reset via email (admin resets passwords manually)
- User self-registration (admin creates users)

## Decisions

### 1. Custom User Management vs ASP.NET Core Identity

**Decision:** Use custom implementation instead of ASP.NET Core Identity (`UserManager`/`RoleManager`).

**Rationale:**
- Data model already defined with specific `User` entity structure
- Only 2 roles needed (Admin, Operator) — no complex role hierarchy
- Small scale system (~10 users, 2-3 concurrent) optimized for free-tier
- No social login, email confirmation, or 2FA in MVP scope
- Full control over entity design and lighter weight
- BCrypt already specified in backend spec for password hashing

**What we implement:**
- Custom `User` entity matching existing data model
- BCrypt.Net-Next for password hashing
- Custom JWT token generation and validation
- Custom refresh token management with database storage
- Simple role enum (Admin, Operator) instead of role tables

**Alternatives Considered:**
- ASP.NET Core Identity: Rejected — adds 7 tables, requires `IdentityUser` inheritance, heavier than needed

### 2. Authentication Strategy: JWT with Refresh Tokens

**Decision:** Use JWT access tokens (short-lived) with refresh tokens (long-lived).

**Rationale:**
- Stateless authentication scales well
- Refresh tokens allow session renewal without re-login
- Standard approach for REST APIs

**Configuration:**
- Access token expiry: 1 hour
- Refresh token expiry: 8 hours (work shift duration)
- BCrypt work factor: 12 (already specified in backend spec)

### 2. Token Storage: HTTP-only Cookies

**Decision:** Store tokens in HTTP-only cookies instead of localStorage.

**Rationale:**
- Protection against XSS attacks
- Automatic inclusion in requests
- Better security for mobile browsers

**Alternatives Considered:**
- localStorage: Rejected - vulnerable to XSS
- sessionStorage: Rejected - lost on tab close

### 3. Authorization Model: RBAC + Resource-Based

**Decision:** Combine role-based access control with resource-based filtering.

**Approach:**
1. **Role check (middleware):** Is user Admin or Operator?
2. **Resource check (service layer):** For operators, filter data by assigned PointOfSale

**Implementation:**
```csharp
// Middleware level - role check
[Authorize(Roles = "Admin")]

// Service level - resource filtering
public async Task<List<Sale>> GetSalesAsync(ClaimsPrincipal user, Guid? pointOfSaleId)
{
    if (user.IsInRole("Admin"))
        return await _repository.GetAllSalesAsync(pointOfSaleId);
    
    var assignedPosIds = await GetUserAssignedPointOfSaleIds(user);
    return await _repository.GetSalesByPointOfSaleAsync(assignedPosIds, pointOfSaleId);
}
```

### 4. User-PointOfSale Assignment Strategy

**Decision:** Soft-delete pattern for assignment history.

**Approach:**
- Use `IsActive` flag instead of physical delete
- Track `AssignedAt` and `UnassignedAt` timestamps
- Allow re-assignment (update existing record or create new)

**Business Rules:**
- Operators must have at least one active assignment
- Administrators don't need assignments (implicit full access)
- Validate assignment before any point-of-sale operation

### 5. Rate Limiting Strategy

**Decision:** Per-IP rate limiting for login endpoint.

**Configuration:**
- 5 failed attempts per 15 minutes per IP
- Response: 429 Too Many Requests
- No account lockout (admin unlocks via IsActive flag)

**Implementation:** ASP.NET Core rate limiting middleware

### 6. Audit Logging

**Decision:** Track login attempts and user modifications.

**What to log:**
- Successful logins: Update `User.LastLoginAt`
- Failed logins: Log to application logs (not database for MVP)
- User changes: Track via `UpdatedAt` field

**Phase 2 consideration:** Dedicated audit table for security-sensitive operations.

## API Design

### Authentication Endpoints

```
POST /api/auth/login
  Request: { username, password }
  Response: { userId, username, firstName, lastName, role }
  Sets: Access token cookie, Refresh token cookie

POST /api/auth/refresh
  Requires: Valid refresh token cookie
  Response: 200 OK (new tokens set in cookies)

POST /api/auth/logout
  Clears: Access token cookie, Refresh token cookie
  Response: 204 No Content

GET /api/auth/me
  Requires: Valid access token
  Response: { userId, username, firstName, lastName, role, assignedPointOfSales }
```

### User Management Endpoints

```
GET /api/users
  Requires: Admin role
  Response: [{ id, username, firstName, lastName, email, role, isActive }]

GET /api/users/{userId}
  Requires: Admin role
  Response: { id, username, firstName, lastName, email, role, isActive, assignedPointOfSales }

POST /api/users
  Requires: Admin role
  Request: { username, password, firstName, lastName, email?, role }
  Response: { id, username, firstName, lastName, email, role, isActive }

PUT /api/users/{userId}
  Requires: Admin role
  Request: { firstName, lastName, email?, role, isActive }
  Response: { id, username, firstName, lastName, email, role, isActive }

PUT /api/users/{userId}/password
  Requires: Admin role
  Request: { newPassword }
  Response: 204 No Content
```

### User Assignment Endpoints

```
GET /api/users/{userId}/point-of-sales
  Requires: Admin role
  Response: [{ pointOfSaleId, name, code, assignedAt, isActive }]

POST /api/users/{userId}/point-of-sales/{pointOfSaleId}
  Requires: Admin role
  Response: { pointOfSaleId, name, code, assignedAt, isActive }

DELETE /api/users/{userId}/point-of-sales/{pointOfSaleId}
  Requires: Admin role
  Response: 204 No Content
```

## Data Flow

### Login Flow

```
1. User submits credentials (username + password)
2. Backend validates credentials against database
   - Find user by username
   - Verify password with BCrypt
   - Check IsActive = true
3. If valid:
   - Update LastLoginAt
   - Generate JWT access token (claims: userId, username, role)
   - Generate refresh token
   - Set HTTP-only cookies
   - Return user info
4. If invalid:
   - Log failed attempt
   - Return 401 with generic message
```

### Authorization Flow (Operator accessing sales)

```
1. Request arrives with access token cookie
2. JWT middleware validates token
3. Authorization middleware checks role requirements
4. Controller receives request
5. Service layer:
   - Gets user's assigned PointOfSale IDs
   - Filters query to only include assigned PointOfSales
6. Return filtered data
```

## Risks / Trade-offs

### Risk: Token Theft

**Mitigation:**
- HTTP-only cookies prevent XSS access
- Short access token expiry limits exposure window
- HTTPS required in production (CORS already specified)

### Risk: Operator without assignments

**Mitigation:**
- Validate at least one assignment when creating operator
- Warn when deactivating last assignment
- Deactivated operators can't login

### Trade-off: Refresh Token in Cookie vs Database

**Decision:** Store refresh token in database for revocation capability.

**Impact:** Additional database query on refresh, but enables:
- Immediate token revocation
- Single active session enforcement (future)

## Migration Plan

### Phase 1: Backend Foundation
1. Create User entity and repository
2. Implement authentication service (login, token generation)
3. Implement JWT middleware
4. Add auth endpoints

### Phase 2: User Management
1. Implement user CRUD service
2. Add user management endpoints
3. Add role-based authorization

### Phase 3: Assignments (after EP8)
1. Create UserPointOfSale entity
2. Implement assignment service
3. Add assignment endpoints
4. Implement resource-based authorization filtering

### Rollback

- Database: Migrations are reversible
- API: New endpoints, no breaking changes
- Frontend: Feature flag to disable auth module

## Open Questions

1. **Password complexity requirements?**
   - Current: Minimum 8 characters
   - Consider: Require uppercase, number, symbol?
   - **Recommendation:** Keep simple for MVP, enhance in Phase 2

2. **Session management: allow multiple sessions?**
   - Current: Yes, multiple devices allowed
   - Consider: Single session per user?
   - **Recommendation:** Allow multiple for MVP (operators may use multiple devices)

3. **Remember username on login screen?**
   - User story mentions optional "remember last user"
   - **Recommendation:** Store username in localStorage (not password), implement in frontend
