# Procedimiento para generar y priorizar Tickets de Trabajo a partir de una User Story

Este documento define el **procedimiento** y la **estructura** para desglosar una User Story en tickets de trabajo (tareas técnicas/funcionales) y ordenarlos.

---

## 0. Referencias del Proyecto

Antes de generar tickets, es recomendable revisar:

- **Arquitectura:** `Documentos/arquitectura.md` - Stack tecnológico y estructura del proyecto.
- **Modelo C4:** `Documentos/modelo-c4.md` - Componentes y relaciones del sistema (ver sección "Nivel 3: Componentes" para entender las capas).
- **Modelo de datos:** `Documentos/modelo-de-datos.md` - Entidades, relaciones e índices.
- **Épicas:** `Documentos/epicas.md` - Épicas del MVP y orden de implementación.
- **User Stories:** `Documentos/Historias/` - Historias de usuario detalladas.
- **Procedimiento User Stories:** `Documentos/Procedimientos/Procedimiento-UserStories.md` - Formato de User Stories.
- **Análisis Metronic Frontend:** `Documentos/Propuestas/analisis-metronic-frontend.md` - Análisis del template Metronic React, componentes disponibles, mapeo por épicas y guía de integración (ver secciones 3, 6 y 7 para componentes UI y plan de adaptación).
- **Testing Backend:** `Documentos/testing-backend.md` - Stack de testing (xUnit, Moq, FluentAssertions), guías detalladas por tema en `Documentos/Testing/Backend/`.
- **Testing Frontend:** `Documentos/testing-frontend.md` - Stack de testing (Vitest, React Testing Library, MSW, Playwright), guías detalladas por tema en `Documentos/Testing/Frontend/`.

---

## 1. Objetivo

- Convertir una User Story en un conjunto de **tickets accionables**.  
- Alinear los tickets con la **arquitectura del sistema** según el modelo C4 definido en `Documentos/modelo-c4.md`.  
- Homogeneizar el formato para facilitar la planificación, seguimiento y trazabilidad.
- **Importante:** Los tickets de trabajo deben redactarse en **inglés** para mantener consistencia con el código y facilitar la colaboración internacional.

---

## 2. Formato estándar de Ticket de Trabajo

**Nota importante:** Todos los tickets deben redactarse en **inglés** (título, descripción, criterios de aceptación, etc.).

Cada ticket debe seguir el siguiente formato:

1. **Título Claro y Conciso** (en inglés)  
   - Un resumen breve que refleje la esencia de la tarea.  
   - Debe ser entendible por cualquier miembro del equipo sin leer toda la descripción.

2. **Descripción Detallada** (en inglés)  
   - **Propósito**:  
     - Explicar por qué es necesaria la tarea y qué problema resuelve.  
   - **Detalles Específicos**:  
     - Requerimientos concretos.  
     - Restricciones técnicas o de negocio.  
     - Decisiones de diseño relevantes.  
     - Referencias a documentación técnica (modelo C4, modelo de datos, arquitectura).  

3. **Criterios de Aceptación** (en inglés)  
   - **Expectativas Claras**:  
     - Lista de condiciones que deben cumplirse para considerar el ticket "Hecho".  
   - **Pruebas de Validación**:  
     - Pasos o pruebas que se deben ejecutar (manuales o automatizadas) para validar el resultado.

4. **Prioridad**  
   - Indicar la **urgencia/impacto relativa del ticket** (Alta, Media, Baja).  
   - Se suele heredar de la prioridad de la User Story, pero se puede ajustar si algún ticket es especialmente crítico (por ejemplo, bloquea a otros equipos).

5. **Estimación de Esfuerzo**  
   - **Story Points (Fibonacci)** o **tiempo estimado** (horas/días) según la práctica del equipo.  
   - Ejemplo: 3 SP (S), 5 SP (M), 8 SP (L).

6. **Asignación** (opcional en definición inicial)  
   - Persona o equipo responsable (Backend, Frontend, QA, DevOps, etc.).

7. **Etiquetas o Tags**  
   - Para clasificar el ticket por:  
     - **Épica asociada** (ej. `EP1`, `EP7`).  
     - **User Story asociada** (ej. `HU-EP1-001`, `HU-EP7-001`).  
     - **Módulo funcional** (ej. `product-module`, `sale-module`, `auth-module`, `inventory-module`).  
     - **Capa técnica** (ej. `domain`, `infrastructure`, `application`, `api`, `frontend`, `tests`).  
     - **Tipo de trabajo** (ej. `feature`, `bug`, `refactor`, `test`, `infra`, `docs`).  
     - **Tecnología específica** (ej. `entity-framework`, `jwt`, `tensorflow`, `postgresql`, `s3`).

8. **Comentarios y Notas** (opcional)  
   - Espacio para discusión, aclaraciones, decisiones durante el desarrollo.

9. **Enlaces o Referencias**  
   - User Story asociada (ruta relativa al archivo en `Documentos/Historias/`).  
   - Documentación funcional/técnica (referencias a `Documentos/modelo-c4.md`, `Documentos/modelo-de-datos.md`, `Documentos/arquitectura.md`).  
   - Otros tickets relacionados (IDs de tickets dependientes).

10. **Historial de Cambios** (opcional)  
    - Registro de cambios en el ticket (estado, prioridad, reasignaciones).  
    - Suele mantenerse en la herramienta (Jira, Azure DevOps, etc.).

---

## 3. Procedimiento para generar tickets a partir de una User Story

### Paso 1 – Revisar la User Story y su contexto

- Leer la historia completa (descripción, criterios de aceptación, notas, tareas de alto nivel).  
- Revisar el modelo C4 en `Documentos/modelo-c4.md` (especialmente la sección "Nivel 3: Componentes") para identificar qué **capas y componentes** se ven afectados según la arquitectura del proyecto.
- Verificar el orden de implementación de épicas definido en `Documentos/epicas.md` (sección "Orden de Implementación") para asegurar que se respetan las dependencias entre épicas.

### Paso 2 – Identificar las áreas de trabajo (por capas / módulos)

Consultar `Documentos/modelo-c4.md` para entender la estructura de capas del proyecto. Típicamente, separar tickets por:

- **Dominio (Core)**: Entidades, Value Objects, reglas de negocio, interfaces de servicios de dominio.
- **Infraestructura**: Repositorios concretos (Entity Framework Core), DbContext, Migraciones, File Storage Service (local/S3/Blob).
- **Aplicación (Services)**: Servicios de aplicación (Product Service, Sale Service, etc.), validaciones de negocio.
- **API (Controllers)**: Endpoints REST (ASP.NET Core), DTOs, validación de entrada, manejo de errores, middleware (JWT, CORS).
- **Frontend (SPA)**: Módulos funcionales (Product Module, Sale Module, etc.), componentes UI basados en Metronic React (ver `Documentos/Propuestas/analisis-metronic-frontend.md` sección 3 para componentes disponibles), servicios API Client, ML Model Handler (TensorFlow.js/ONNX.js).  
  - **Componentes Metronic disponibles:** Consultar sección 3 del análisis de Metronic para identificar componentes UI específicos (data-grid, file-upload, form, card, etc.) que pueden reutilizarse en lugar de crear componentes desde cero.
  - **Layout:** El proyecto utiliza Layout 8 de Metronic (ver análisis sección 2). Los tickets de frontend deben considerar la estructura del layout existente.
  - **Mapeo por épicas:** Consultar sección 6 del análisis de Metronic para ver qué componentes Metronic se recomiendan para cada épica.
- **Tests**: 
  - **Backend:** Unitarios (servicios, validadores), integración (API, repositorios con Testcontainers). Ver `Documentos/testing-backend.md`.
  - **Frontend:** Unitarios (hooks, utilities), componentes (React Testing Library), E2E (Playwright). Ver `Documentos/testing-frontend.md`.
  - **ML:** Tests de reconocimiento de imágenes (precisión, rendimiento).

**Tickets de Infraestructura y DevOps:**

Además de las capas funcionales, considerar tickets para:

- **Configuración de entorno de desarrollo:**
  - Docker Compose para desarrollo local.
  - Configuración de PostgreSQL en contenedor.
  - Configuración de almacenamiento local para desarrollo.
  
- **Migraciones de base de datos:**
  - Crear migraciones de Entity Framework Core.
  - Seeders de datos iniciales (métodos de pago predefinidos).
  - Scripts de inicialización de base de datos.
  
- **Configuración de CI/CD:**
  - GitHub Actions para build y tests.
  - Configuración de despliegue (AWS/Azure).
  - Health checks y monitoreo básico.
  
- **Configuración de almacenamiento:**
  - Implementación de File Storage Service (local vs cloud).
  - Configuración de S3/Blob Storage para producción.
  - Manejo de pre-signed URLs si es necesario.

**Ejemplo de descomposición** (basado en HU-EP1-001: Importar productos desde Excel):
- Ticket de dominio: Entidad Product y Collection (si no existen).
- Ticket de infraestructura: Repositorio de Product con Entity Framework Core.
- Ticket de aplicación: Product Service con lógica de importación Excel y validaciones.
- Ticket de aplicación: Excel Import Service (procesamiento de archivos Excel).
- Ticket de API: Endpoint POST /api/products/import con validación de entrada.
- Ticket de frontend: Página de importación usando componente `file-upload.tsx` de Metronic (ver análisis Metronic sección 6.1) con preview y validación.
- Ticket de frontend: Visualización de errores usando componente `alert.tsx` o `toast` de Metronic y confirmación con `dialog.tsx`.
- Ticket de tests: Tests unitarios del servicio de importación.
- Ticket de tests: Tests de integración del endpoint de importación.

### Paso 3 – Crear los tickets siguiendo el formato estándar

Para cada área identificada:

1. Redactar un **título claro en inglés** (ej.: "Create Product and Collection domain entities").  
2. Describir **propósito** y **detalles específicos en inglés**:  
   - Qué hay que hacer.  
   - Qué restricciones hay.  
   - Qué decisiones de diseño se esperan.  
   - Referencias a documentación técnica (modelo C4, modelo de datos, arquitectura).
   - **Para tickets de frontend:** Especificar qué componentes Metronic se utilizarán (consultar `Documentos/Propuestas/analisis-metronic-frontend.md` sección 3 y 6 para identificar componentes apropiados).
3. Definir **criterios de aceptación en inglés** y **pruebas de validación**.  
4. Estimar una **prioridad relativa** (Alta/Media/Baja) dentro de la User Story.  
5. Asignar una primera **estimación de esfuerzo** (SP / horas). Ver sección 4.6 para guía de estimación.  
   - **Nota para frontend:** El uso de componentes Metronic existentes puede reducir la estimación comparado con crear componentes desde cero (ver análisis Metronic sección 11 para ventajas).
6. Añadir **tags** y **enlaces** a la User Story y documentación relacionada.  
   - **Para tickets de frontend:** Incluir tag `metronic` y referencias a secciones específicas del análisis de Metronic cuando sea relevante.

### Paso 3.5 – Vincular tickets con User Story origen

Cada ticket debe hacer referencia explícita a la User Story de la que proviene:

- **En la descripción del ticket:** Incluir referencia a la User Story (ej: "This ticket is part of HU-EP1-001").
- **En los tags:** Incluir tag con el ID de la User Story (ej: `HU-EP1-001`).
- **En los criterios de aceptación:** Los criterios del ticket deben alinearse con los criterios de aceptación de la User Story.
- **En las referencias:** Incluir enlace al archivo de la User Story en `Documentos/Historias/`.

Esto facilita la trazabilidad y permite verificar que todos los tickets necesarios para completar una User Story están creados.

### Paso 4 – Verificar dependencias entre tickets

- Identificar dependencias técnicas entre tickets:
  - **Dependencias de capa:** Dominio → Infraestructura → Aplicación → API → Frontend
  - **Dependencias de funcionalidad:** Ej: Registro de ventas requiere productos, inventario y métodos de pago
  - **Dependencias de épica:** Respetar el orden de implementación de épicas definido en `Documentos/epicas.md` (sección "Orden de Implementación")
  
- Documentar dependencias en:
  - **Campo de dependencias:** Listar IDs de tickets dependientes (ej: "Depends on T-EP1-001, T-EP1-002").
  - **En la descripción:** Explicar por qué existe la dependencia y qué se necesita del ticket dependiente.
  - **En los criterios de aceptación:** Incluir validación de que los tickets dependientes están completos.
  
- Crear un diagrama de dependencias (opcional pero recomendado) para visualizar el orden de desarrollo.

Ejemplo típico de cadena de dependencias:
- T1 (Dominio) → T2 (BD) → T3 (Aplicación) → T4 (API) → T5/T6 (Frontend) → T7 (Tests integrales).

---

## 4. Procedimiento para ordenar/priorizar los Tickets

### Criterios de ordenación

1. **Respetar el orden de épicas:** Verificar el orden de implementación definido en `Documentos/epicas.md` (sección "Orden de Implementación"). Los tickets de épicas anteriores deben completarse antes de comenzar con épicas dependientes.
2. **Tickets fundacionales** (modelo de dominio, BD, casos de uso) primero dentro de cada épica.  
3. **Tickets que desbloquean otros** (APIs, contratos) tienen prioridad.  
4. Posteriormente, **UI/UX** y **tests** una vez la funcionalidad base existe.  
5. Evitar que tickets dependientes entren en un sprint en el que sus prerrequisitos aún no están planificados o terminados.

### Ejemplo de orden para una User Story típica

1. Ticket Dominio (modelo).  
2. Ticket Base de datos (migraciones).  
3. Ticket Caso de uso (aplicación).  
4. Ticket API.  
5. Ticket UI principal.  
6. Ticket validaciones y manejo de errores UI.  
7. Ticket tests automatizados (backend + frontend).

---

## 4.5. Consideraciones para desarrollo basado en tickets

Dado que el desarrollo del código se realizará directamente a partir de los tickets de trabajo, es importante:

### Nivel de detalle adecuado

- **Criterios de aceptación específicos:** Cada ticket debe tener criterios de aceptación claros y verificables en inglés.
- **Decisiones técnicas documentadas:** Si hay decisiones de diseño importantes, documentarlas en la descripción del ticket.
- **Referencias a documentación:** Incluir enlaces a:
  - Modelo C4 (`Documentos/modelo-c4.md`) para entender la arquitectura.
  - Modelo de datos (`Documentos/modelo-de-datos.md`) para entender las entidades.
  - Arquitectura (`Documentos/arquitectura.md`) para entender el stack tecnológico.
  - **Para tickets de frontend:** Análisis Metronic (`Documentos/Propuestas/analisis-metronic-frontend.md`) para identificar componentes UI reutilizables y estructura de páginas.

### Tickets independientes

- Cada ticket debe ser **independiente** en la medida de lo posible.
- Si un ticket depende de otro, documentarlo claramente en la sección de dependencias.
- Evitar tickets que requieran múltiples épicas simultáneamente (respetar el orden de épicas).

### Testing como parte del desarrollo

- Incluir tickets de testing para cada funcionalidad desarrollada.
- Los tests deben cubrir:
  - Casos felices (happy path).
  - Casos de error y validaciones.
  - Casos límite (edge cases).
- Considerar tests unitarios, de integración y E2E según corresponda.

### Consideraciones específicas para tickets de Frontend con Metronic

Al crear tickets de frontend, consultar `Documentos/Propuestas/analisis-metronic-frontend.md` para:

- **Identificar componentes reutilizables:** Antes de crear un componente desde cero, verificar si Metronic ya proporciona uno (sección 3 del análisis). Ejemplos:
  - `data-grid.tsx` para tablas avanzadas (productos, inventario, ventas)
  - `file-upload.tsx` para subida de fotos y archivos Excel
  - `form.tsx` para formularios con validación (react-hook-form)
  - `card.tsx` para mostrar productos y sugerencias
  - `dialog.tsx` para modales y confirmaciones
  
- **Mapeo por épicas:** Consultar sección 6 del análisis para ver qué componentes Metronic se recomiendan específicamente para cada épica del proyecto.

- **Estructura de páginas:** Seguir la estructura propuesta en la sección 7.3 del análisis (`pages/products/`, `pages/inventory/`, etc.).

- **Layout y routing:** Los tickets deben considerar que el proyecto usa Layout 8 de Metronic (ver análisis sección 2). Los componentes deben integrarse correctamente con el sidebar y header del layout.

- **Servicios HTTP:** Los tickets de frontend deben considerar la estructura de servicios propuesta en la sección 7.4 del análisis (crear servicios por módulo: `products.service.ts`, `inventory.service.ts`, etc.).

- **Tipos TypeScript:** Los tickets deben definir o referenciar tipos TypeScript para DTOs del backend (ver análisis sección 7.5 para estructura de tipos).

- **Dependencias adicionales:** Si el ticket requiere funcionalidades específicas (ML, Excel, etc.), consultar sección 9 del análisis para identificar dependencias necesarias.

### Consideraciones específicas para tickets de Testing Backend

Al crear tickets de testing backend, consultar `Documentos/testing-backend.md` (índice principal) y `Documentos/Testing/Backend/` (guías detalladas) para:

- **Identificar tipo de test necesario:**
  - **Tests unitarios:** Servicios, validadores, lógica de negocio aislada (mockear dependencias con Moq).
  - **Tests de integración:** Repositorios con BD real (Testcontainers + PostgreSQL), endpoints API (WebApplicationFactory).
  - **Tests de autenticación:** Escenarios JWT, roles, tokens expirados/inválidos.

- **Stack y convenciones del proyecto:**
  - **Framework:** xUnit 2.9.x
  - **Mocking:** Moq 4.20.x
  - **Assertions:** FluentAssertions 7.x
  - **Datos de prueba:** Bogus 35.x
  - **Contenedores:** Testcontainers 4.x (PostgreSQL)
  - **Nomenclatura:** `Método_Escenario_ResultadoEsperado` (ej: `CreateSale_WithInsufficientStock_ShouldThrowException`)
  - **Estructura:** AAA (Arrange, Act, Assert)

- **Guías detalladas por tema:**

  | Tema | Documento | Cuándo consultar |
  |------|-----------|------------------|
  | Configuración inicial | `Testing/Backend/01-configuracion.md` | Setup de proyectos de test, paquetes NuGet |
  | Tests unitarios + Bogus | `Testing/Backend/02-tests-unitarios.md` | Tests de servicios, [Theory], generación de datos |
  | Testcontainers + PostgreSQL | `Testing/Backend/03-testcontainers.md` | Tests de integración con BD real |
  | Mocking EF Core | `Testing/Backend/04-mocking-efcore.md` | Mockear DbContext, DbSet, transacciones |
  | Autenticación JWT | `Testing/Backend/05-autenticacion-jwt.md` | Tests de login, tokens, roles, endpoints protegidos |
  | GitHub Actions CI/CD | `Testing/Backend/06-github-actions.md` | Workflows de CI/CD, matriz de tests |
  | Cobertura de código | `Testing/Backend/07-cobertura-codigo.md` | Coverlet, reportes, umbrales mínimos |
  | Validaciones | `Testing/Backend/08-validaciones.md` | FluentValidation.TestHelper, DataAnnotations |
  | Archivos/Uploads | `Testing/Backend/09-archivos-uploads.md` | Excel (ClosedXML), MockFileSystem, S3 |

- **Tags recomendados para tickets de testing:**
  - Tipo: `test`, `unit-test`, `integration-test`
  - Tecnología: `xunit`, `testcontainers`, `moq`, `fluentassertions`
  - Capa: `tests`

- **Criterios de aceptación típicos para tickets de testing:**
  - Tests cubren casos felices (happy path)
  - Tests cubren casos de error y validaciones
  - Tests cubren casos límite (edge cases)
  - Nomenclatura sigue convención `Método_Escenario_ResultadoEsperado`
  - Cobertura de código cumple umbral mínimo (70% recomendado)

### Consideraciones específicas para tickets de Testing Frontend

Al crear tickets de testing frontend, consultar `Documentos/testing-frontend.md` (índice principal) y `Documentos/Testing/Frontend/` (guías detalladas) para:

- **Identificar tipo de test necesario:**
  - **Tests unitarios:** Hooks, utilities, helpers y funciones puras.
  - **Tests de componentes:** Componentes React con React Testing Library (queries accesibles, user events).
  - **Tests E2E:** Flujos completos con Playwright (autenticación, CRUD, navegación).

- **Stack y convenciones del proyecto:**
  - **Test Runner:** Vitest 2.x
  - **Testing de Componentes:** React Testing Library 16.x
  - **Simulación de Usuario:** @testing-library/user-event 14.x
  - **Mocking de API:** MSW (Mock Service Worker) 2.x
  - **Tests E2E:** Playwright 1.x
  - **Nomenclatura:** `should [comportamiento esperado] when [condición]`
  - **Estructura:** Arrange, Act, Assert
  - **Queries:** Priorizar queries accesibles (`getByRole`, `getByLabelText`) sobre `getByTestId`

- **Guías detalladas por tema:**

  | Tema | Documento | Cuándo consultar |
  |------|-----------|------------------|
  | Configuración inicial | `Testing/Frontend/01-configuracion.md` | Setup de Vitest, Playwright, estructura de carpetas |
  | Tests unitarios | `Testing/Frontend/02-tests-unitarios.md` | Tests de hooks, utilities, mocking con Vitest |
  | Tests de componentes | `Testing/Frontend/03-tests-componentes.md` | React Testing Library, formularios (React Hook Form + Zod), accesibilidad |
  | Mocking de API | `Testing/Frontend/04-mocking-api.md` | MSW handlers, escenarios de error, override en tests |
  | Tests E2E | `Testing/Frontend/05-tests-e2e.md` | Playwright, Page Objects, auth setup, multi-navegador |
  | GitHub Actions CI/CD | `Testing/Frontend/06-github-actions.md` | Workflows, caché, reportes, artifacts |
  | Cobertura de código | `Testing/Frontend/07-cobertura-codigo.md` | Vitest coverage, umbrales, Codecov |

- **Tags recomendados para tickets de testing frontend:**
  - Tipo: `test`, `unit-test`, `component-test`, `e2e-test`
  - Tecnología: `vitest`, `testing-library`, `msw`, `playwright`
  - Capa: `tests`, `frontend`

- **Criterios de aceptación típicos para tickets de testing frontend:**
  - Tests cubren casos felices (happy path)
  - Tests cubren casos de error y estados de carga
  - Tests cubren interacciones de usuario (clicks, typing, navigation)
  - Queries usan selectores accesibles (getByRole, getByLabelText)
  - Tests E2E cubren flujos críticos en múltiples navegadores
  - Cobertura de código cumple umbral mínimo (70% recomendado)

---

## 4.6. Guía de estimación para este proyecto

### Factores a considerar en la estimación:

- **Complejidad del stack:** 
  - Backend: ASP.NET Core con Entity Framework Core (familiar para equipos .NET).
  - Frontend: React 19 con TypeScript y Metronic React template (ver `Documentos/Propuestas/analisis-metronic-frontend.md`). Los componentes Metronic disponibles reducen el esfuerzo de desarrollo UI comparado con crear componentes desde cero.
  - ML: TensorFlow.js/ONNX.js (nuevo para el equipo, puede requerir investigación).
  
- **Integraciones externas:**
  - PostgreSQL (estándar).
  - S3/Blob Storage (requiere configuración adicional).
  
- **Optimizaciones para free-tier:**
  - Connection pooling limitado (5-10 conexiones).
  - Paginación obligatoria.
  - Caching en memoria.

### Escala sugerida (Story Points):

- **1 SP:** Tarea muy simple (ej: agregar campo a DTO).
- **2 SP:** Tarea simple (ej: crear endpoint básico sin lógica compleja).
- **3 SP:** Tarea estándar (ej: implementar servicio de aplicación con validaciones).
- **5 SP:** Tarea compleja (ej: importación de Excel con validaciones múltiples).
- **8 SP:** Tarea muy compleja (ej: reconocimiento de imágenes con ML).
- **13 SP:** Tarea extremadamente compleja (dividir en subtickets).

---

## 4.7. Estructura de almacenamiento de tickets

Los tickets creados deben guardarse siguiendo la siguiente estructura de carpetas:

```
Tickets/
├── EP1/
│   ├── HU-EP1-001/
│   │   ├── T-EP1-001-001.md (ticket 1)
│   │   ├── T-EP1-001-002.md (ticket 2)
│   │   └── ...
│   ├── HU-EP1-002/
│   │   └── ...
│   └── ...
├── EP2/
│   └── ...
└── ...
```

**Convención de nombres:**
- Carpeta de épica: `EP1`, `EP2`, `EP3`, etc.
- Carpeta de User Story: `HU-EP1-001`, `HU-EP1-002`, etc.
- Archivo de ticket: `T-EP[X]-[NNN]-[MMM].md` donde:
  - `EP[X]` es el código de la épica
  - `[NNN]` es el número de la User Story (001, 002, etc.)
  - `[MMM]` es el número secuencial del ticket dentro de la User Story (001, 002, etc.)

**Ejemplo:** `Tickets/EP1/HU-EP1-001/T-EP1-001-001.md`

---

## 5. Checklist rápido antes de dar por lista la descomposición en tickets

- [ ] Cada ticket tiene un título claro en inglés y una descripción entendible en inglés.  
- [ ] Los tickets cubren todas las tareas mencionadas en la User Story.  
- [ ] No hay tickets excesivamente grandes (si los hay, subdividir).  
- [ ] Las dependencias entre tickets están identificadas y documentadas.  
- [ ] Se han asignado prioridades y estimaciones de esfuerzo.  
- [ ] Cada ticket hace referencia explícita a la User Story origen (tag y enlace).  
- [ ] Los tickets respetan el orden de implementación de épicas definido en `Documentos/epicas.md`.  
- [ ] Los tickets están guardados en la estructura de carpetas correcta (`Tickets/EP[X]/HU-EP[X]-[NNN]/`).  
- [ ] Los archivos de tickets siguen la convención de nombres `T-EP[X]-[NNN]-[MMM].md`.
- [ ] **Para tickets de frontend:** Se ha verificado si existen componentes Metronic reutilizables antes de planificar crear componentes desde cero (consultar `Documentos/Propuestas/analisis-metronic-frontend.md` sección 3 y 6).
- [ ] **Para tickets de frontend:** Se han identificado los componentes Metronic específicos a utilizar y se han documentado en la descripción del ticket.
- [ ] **Para tickets de testing backend:** Se ha consultado la guía de testing (`Documentos/testing-backend.md`) para identificar el tipo de test apropiado.
- [ ] **Para tickets de testing backend:** Se ha referenciado la guía específica del tema (Testcontainers, JWT, validaciones, etc.) en `Documentos/Testing/Backend/`.
- [ ] **Para tickets de testing backend:** Los criterios de aceptación incluyen nomenclatura correcta y cobertura de casos (happy path, errores, edge cases).
- [ ] **Para tickets de testing frontend:** Se ha consultado la guía de testing (`Documentos/testing-frontend.md`) para identificar el tipo de test apropiado (unitario, componente, E2E).
- [ ] **Para tickets de testing frontend:** Se ha referenciado la guía específica del tema (MSW, Playwright, formularios, etc.) en `Documentos/Testing/Frontend/`.
- [ ] **Para tickets de testing frontend:** Los criterios de aceptación incluyen uso de queries accesibles y cobertura de interacciones de usuario.
