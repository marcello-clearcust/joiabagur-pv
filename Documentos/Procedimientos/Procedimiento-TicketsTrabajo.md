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
- **Frontend (SPA)**: Módulos funcionales (Product Module, Sale Module, etc.), componentes UI, servicios API Client, ML Model Handler (TensorFlow.js/ONNX.js).
- **Tests**: Unitarios (backend), integración (API), E2E (frontend), tests de reconocimiento de imágenes.

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
- Ticket de frontend: Componente de carga de archivo con preview y validación.
- Ticket de frontend: Visualización de errores y confirmación de importación.
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
3. Definir **criterios de aceptación en inglés** y **pruebas de validación**.  
4. Estimar una **prioridad relativa** (Alta/Media/Baja) dentro de la User Story.  
5. Asignar una primera **estimación de esfuerzo** (SP / horas). Ver sección 4.6 para guía de estimación.
6. Añadir **tags** y **enlaces** a la User Story y documentación relacionada.

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

---

## 4.6. Guía de estimación para este proyecto

### Factores a considerar en la estimación:

- **Complejidad del stack:** 
  - Backend: ASP.NET Core con Entity Framework Core (familiar para equipos .NET).
  - Frontend: React/Vue/Angular con TypeScript (considerar curva de aprendizaje).
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
