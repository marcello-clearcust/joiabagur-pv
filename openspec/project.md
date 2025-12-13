# Project Context

## Purpose

**Sistema de Gestión de Puntos de Venta para Joyería** - An integral management system for a jewelry business operating across multiple points of sale (own stores and third-party locations like hotels). The application enables inventory management, sales registration, and product identification through AI-powered image recognition.

### Key Objectives
- Centralized product catalog with photo-based product identification
- Multi-location inventory tracking with Excel import capabilities
- Sales registration with AI image recognition (3-5 suggestions ordered by confidence)
- Role-based access (Administrator full access, Operator restricted to assigned locations)
- Optimized for mobile use by operators at points of sale
- Free-tier cloud deployment (AWS/Azure)

### MVP Scope (Phase 1)
9 Epics with 35 User Stories covering:
- **EP1**: Product Management (7 stories)
- **EP2**: Inventory Management (5 stories)
- **EP3**: Sales Registration (2 stories)
- **EP4**: AI Image Recognition (1 story)
- **EP5**: Returns Management (3 stories)
- **EP6**: Payment Methods Management (3 stories)
- **EP7**: Authentication & User Management (6 stories)
- **EP8**: Points of Sale Management (4 stories)
- **EP9**: Queries & Reports (4 stories)

---

## Tech Stack

### Backend
- **Runtime**: .NET 10
- **Framework**: ASP.NET Core Web API
- **Language**: C#
- **ORM**: Entity Framework Core
- **Database**: PostgreSQL 15+
- **Authentication**: JWT (JSON Web Tokens)
- **Logging**: Serilog
- **Architecture**: Monolithic with layered separation (Domain → Infrastructure → Application → API)

### Frontend
- **Framework**: React 19
- **Language**: TypeScript
- **Build Tool**: Vite
- **UI Template**: Metronic React (Layout 8 - sidebar navigation)
- **UI Components**: Radix UI, Tailwind CSS, Lucide React icons
- **Forms**: React Hook Form + Zod validation
- **Tables**: TanStack Table (React Table)
- **HTTP Client**: Axios/Fetch
- **State Management**: Context API (or Zustand if needed)
- **ML Framework**: TensorFlow.js / ONNX.js (client-side image recognition)

### Infrastructure
- **Containers**: Docker, Docker Compose (development)
- **CI/CD**: GitHub Actions
- **Repository**: GitHub
- **Cloud**: AWS (ECS/App Runner, RDS, S3, CloudFront) or Azure (App Service, PostgreSQL, Blob Storage, CDN)
- **Target**: Free-tier optimized deployment

### Testing Stack

**Backend:**
- Test Framework: xUnit 2.9.x
- Mocking: Moq 4.20.x
- Assertions: FluentAssertions 7.x
- Test Data: Bogus 35.x
- Integration Tests: Testcontainers 4.x (PostgreSQL)

**Frontend:**
- Test Runner: Vitest 2.x
- Component Testing: React Testing Library 16.x
- User Events: @testing-library/user-event 14.x
- API Mocking: MSW (Mock Service Worker) 2.x
- E2E Testing: Playwright 1.x
- DOM Environment: jsdom 25.x

---

## Project Conventions

### Code Style

**Backend (C#/.NET):**
- Follow Microsoft C# coding conventions
- Use async/await for I/O operations
- Repository pattern for data access
- Service layer for business logic
- DTOs for API contracts
- FluentValidation for input validation
- Structured logging with Serilog

**Frontend (TypeScript/React):**
- Functional components with hooks
- TypeScript strict mode enabled
- Components colocated with tests (`component.tsx` + `component.test.tsx`)
- Services organized by domain module
- Custom hooks for reusable logic
- Prefer Metronic UI components over custom implementations

### Architecture Patterns

**Backend Layers (per Modelo C4):**
1. **Domain (Core)**: Entities, Value Objects, domain interfaces
2. **Infrastructure**: EF Core repositories, DbContext, migrations, File Storage Service
3. **Application (Services)**: Product Service, Sale Service, Inventory Service, etc.
4. **API (Controllers)**: REST endpoints, DTOs, middleware (JWT, CORS, logging)

**Frontend Modules (per Metronic analysis):**
- Auth Module (login, session, token management)
- Product Module (catalog, import, photos)
- Inventory Module (stock views, adjustments)
- Sale Module (registration, payment selection)
- Image Recognition Module (capture, ML inference, suggestions)
- Return Module (registration, history)
- Payment Method Module (configuration, assignment)
- Point of Sale Module (CRUD)
- User Module (management, assignments)
- Report Module (queries, filters)

**Shared Services:**
- File Storage Service (abstraction for local/S3/Blob)
- Stock Validation Service
- Payment Method Validation Service
- Excel Import Service

### Testing Strategy

**Backend Tests:**
- Nomenclature: `Method_Scenario_ExpectedResult` (e.g., `CreateSale_WithInsufficientStock_ShouldThrowException`)
- Structure: AAA (Arrange, Act, Assert)
- Minimum coverage target: 70%
- Unit tests for services and validators
- Integration tests with Testcontainers (PostgreSQL)
- JWT authentication tests for protected endpoints

**Frontend Tests:**
- Nomenclature: `should [behavior] when [condition]` (e.g., `should show error when API returns 401`)
- Prefer accessible queries (`getByRole`, `getByLabelText` over `getByTestId`)
- MSW for API mocking
- Playwright for E2E flows (authentication, CRUD operations)
- Minimum coverage target: 70%

### Git Workflow

- **Main branch**: `main` (production-ready)
- **Development branch**: `develop` (integration)
- **Feature branches**: `feature/[epic]-[description]`
- **Commits**: Conventional commits format
- **CI/CD**: GitHub Actions for build, test, and deploy

### Documentation

- **Language**: Technical documentation in English, User Stories in Spanish
- **Tickets**: Written in English for code consistency
- **Location**: `Documentos/` folder for all project documentation
- **User Stories**: `Documentos/Historias/HU-EP[X]-[NNN].md`
- **Work Tickets**: `Tickets/EP[X]/HU-EP[X]-[NNN]/T-EP[X]-[NNN]-[MMM].md`

---

## Domain Context

### Business Domain
- **Industry**: Jewelry retail
- **Operations**: Multiple points of sale (own stores + third-party locations like hotels)
- **Key Challenge**: Product identification accuracy - jewelry items can be difficult to distinguish
- **Solution**: AI-powered image recognition with manual confirmation

### Key Entities
- **Product**: SKU (unique), name, description, price, collection (optional)
- **ProductPhoto**: Multiple reference photos per product for ML training
- **PointOfSale**: Store locations with assigned operators and payment methods
- **User**: Admin (full access) or Operator (restricted to assigned locations)
- **Sale**: Transaction with price snapshot, payment method, optional photo
- **Inventory**: Stock quantity per product per location
- **InventoryMovement**: Full audit trail (Sale, Return, Adjustment, Import)
- **PaymentMethod**: Efectivo, Bizum, Transferencia, Tarjeta TPV propio, Tarjeta TPV punto de venta, PayPal
- **Return**: Linked to original sale, auto-updates inventory

### Business Rules
1. Operators can only access assigned points of sale
2. Sales require valid payment method assigned to the point of sale
3. Stock cannot be negative (validated at application level)
4. Price in Sale is a snapshot (not reference to current product price)
5. Products need at least one photo for image recognition
6. Only one photo can be marked as primary per product

---

## Important Constraints

### Free-tier Optimization
- **Database connections**: Max 5-10 simultaneous (connection pooling)
- **Pagination**: Mandatory for all lists (max 50 items/page)
- **Caching**: In-memory cache for frequently accessed data (products, payment methods)
- **Image compression**: Before upload to storage
- **Bundle size**: Frontend < 500KB initial load

### Performance Targets
- **Users**: 2-3 concurrent
- **Products**: ~500 catalog items
- **Response time**: Optimize for mobile operators

### Security Requirements
- JWT authentication with refresh tokens
- BCrypt password hashing with salt
- HTTPS required in production
- CORS configured per environment
- Role-based access control (RBAC)
- Pre-signed URLs for storage access

### Data Storage
- **Development**: Local filesystem (`./uploads/`) + PostgreSQL in Docker
- **Production**: S3/Blob Storage + managed PostgreSQL (RDS/Azure Database)
- **Strategy Pattern**: IFileStorageService abstraction for environment switching

---

## External Dependencies

### Cloud Services (Production)
- **PostgreSQL**: RDS (AWS) or Azure Database for PostgreSQL
- **Object Storage**: S3 (AWS) or Azure Blob Storage
- **CDN**: CloudFront (AWS) or Azure CDN
- **Container Hosting**: ECS Fargate/App Runner (AWS) or Azure App Service

### Third-party Libraries

**Backend:**
- Entity Framework Core (ORM)
- Serilog (logging)
- BCrypt.Net (password hashing)
- ClosedXML (Excel processing)

**Frontend:**
- Metronic React template (UI components, layouts)
- TensorFlow.js or ONNX.js (ML inference)
- xlsx/exceljs (Excel file reading)
- React Router v7 (routing)
- Sonner (toast notifications)
- next-themes (dark mode support)

### Development Tools
- Docker & Docker Compose
- GitHub Actions
- PostgreSQL 15+
- Node.js (for frontend)
- .NET 10 SDK

---

## Implementation Order

Epics must be implemented in this order due to dependencies:

1. **EP7**: Authentication & User Management (system foundation)
2. **EP8**: Points of Sale Management (required for inventory, sales)
3. **EP6**: Payment Methods Management (required for sales)
4. **EP1**: Product Management (required for sales, inventory)
5. **EP2**: Inventory Management (required for sales)
6. **EP3**: Sales Registration (core functionality)
7. **EP4**: AI Image Recognition (enhances sales)
8. **EP5**: Returns Management (complement)
9. **EP9**: Queries & Reports (analysis)

---

## Key Documentation References

- **Architecture**: `Documentos/arquitectura.md`
- **C4 Model**: `Documentos/modelo-c4.md`
- **Data Model**: `Documentos/modelo-de-datos.md`
- **Epics**: `Documentos/epicas.md`
- **Testing Backend**: `Documentos/testing-backend.md` + `Documentos/Testing/Backend/`
- **Testing Frontend**: `Documentos/testing-frontend.md` + `Documentos/Testing/Frontend/`
- **Metronic Analysis**: `Documentos/Propuestas/analisis-metronic-frontend.md`
- **Technical Clarifications**: `Documentos/Propuestas/aclaraciones-tecnicas.md`
- **User Story Procedure**: `Documentos/Procedimientos/Procedimiento-UserStories.md`
- **Work Ticket Procedure**: `Documentos/Procedimientos/Procedimiento-TicketsTrabajo.md`
