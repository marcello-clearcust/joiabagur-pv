# Procedimiento para generar y priorizar Tickets de Trabajo a partir de una User Story

Este documento define el **procedimiento** y la **estructura** para desglosar una User Story en tickets de trabajo (tareas técnicas/funcionales) y ordenarlos.

---

## 1. Objetivo

- Convertir una User Story en un conjunto de **tickets accionables**.  
- Alinear los tickets con la **arquitectura del sistema** (por ejemplo, modelo C4: Dominio, Aplicación, Infraestructura, API, UI).  
- Homogeneizar el formato para facilitar la planificación, seguimiento y trazabilidad.

---

## 2. Formato estándar de Ticket de Trabajo

Cada ticket debe seguir el siguiente formato:

1. **Título Claro y Conciso**  
   - Un resumen breve que refleje la esencia de la tarea.  
   - Debe ser entendible por cualquier miembro del equipo sin leer toda la descripción.

2. **Descripción Detallada**  
   - **Propósito**:  
     - Explicar por qué es necesaria la tarea y qué problema resuelve.  
   - **Detalles Específicos**:  
     - Requerimientos concretos.  
     - Restricciones técnicas o de negocio.  
     - Decisiones de diseño relevantes.  

3. **Criterios de Aceptación**  
   - **Expectativas Claras**:  
     - Lista de condiciones que deben cumplirse para considerar el ticket “Hecho”.  
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
     - Módulo (ej. `jobs-module`, `candidate-module`).  
     - Capa (ej. `backend`, `frontend`, `infra`, `api`).  
     - User Story asociada (ej. `HU1`).  
     - Tipo de trabajo (ej. `feature`, `bug`, `refactor`, `tests`).

8. **Comentarios y Notas** (opcional)  
   - Espacio para discusión, aclaraciones, decisiones durante el desarrollo.

9. **Enlaces o Referencias**  
   - User Story asociada.  
   - Documentación funcional/técnica.  
   - Diagramas (por ejemplo, C4).  
   - Otros tickets relacionados.

10. **Historial de Cambios** (opcional)  
    - Registro de cambios en el ticket (estado, prioridad, reasignaciones).  
    - Suele mantenerse en la herramienta (Jira, Azure DevOps, etc.).

---

## 3. Procedimiento para generar tickets a partir de una User Story

### Paso 1 – Revisar la User Story y su contexto

- Leer la historia completa (descripción, criterios de aceptación, notas, tareas de alto nivel).  
- Revisar el modelo de arquitectura (por ejemplo C4) para identificar qué **capas y componentes** se ven afectados.

### Paso 2 – Identificar las áreas de trabajo (por capas / módulos)

Típicamente, separar tickets por:

- **Dominio**: modelos de negocio, reglas, entidades, value objects.  
- **Infraestructura**: base de datos, migraciones, repositorios concretos.  
- **Aplicación**: casos de uso, command/query handlers, servicios de aplicación.  
- **API / Integraciones**: endpoints REST/GraphQL, adaptadores externos.  
- **Frontend / SPA / UI**: pantallas, componentes, flujos de usuario.  
- **Tests / QA**: baterías de pruebas automatizadas, test de regresión, contratos.

Ejemplo de descomposición (basado en HU1):
- Ticket de modelo de dominio (JobOffer, Plantilla).  
- Ticket de migraciones de BD.  
- Ticket de caso de uso (Command Handler).  
- Ticket de API REST.  
- Ticket de pantalla de UI.  
- Ticket de validaciones de formulario.  
- Ticket de tests automatizados.

### Paso 3 – Crear los tickets siguiendo el formato estándar

Para cada área identificada:

1. Redactar un **título claro** (ej.: “Definir entidades de dominio JobOffer y JobOfferTemplate”).  
2. Describir **propósito** y **detalles específicos**:  
   - Qué hay que hacer.  
   - Qué restricciones hay.  
   - Qué decisiones de diseño se esperan.  
3. Definir **criterios de aceptación** y **pruebas de validación**.  
4. Estimar una **prioridad relativa** (Alta/Media/Baja) dentro de la User Story.  
5. Asignar una primera **estimación de esfuerzo** (SP / horas).  
6. Añadir **tags** y **enlaces** a la User Story y documentación relacionada.

### Paso 4 – Verificar dependencias entre tickets

- Identificar si alguno de los tickets depende de otro (por ejemplo, la API depende del caso de uso y del modelo, la UI depende de la API, etc.).  
- Documentar la dependencia en:  
  - La descripción.  
  - O una sección de notas (ej.: “Depende de T1 y T2”).

Ejemplo típico de cadena de dependencias:
- T1 (Dominio) → T2 (BD) → T3 (Aplicación) → T4 (API) → T5/T6 (Frontend) → T7 (Tests integrales).

---

## 4. Procedimiento para ordenar/priorizar los Tickets

### Criterios de ordenación

1. **Tickets fundacionales** (modelo de dominio, BD, casos de uso) primero.  
2. **Tickets que desbloquean otros** (APIs, contratos) tienen prioridad.  
3. Posteriormente, **UI/UX** y **tests** una vez la funcionalidad base existe.  
4. Evitar que tickets dependientes entren en un sprint en el que sus prerrequisitos aún no están planificados o terminados.

### Ejemplo de orden para una User Story típica

1. Ticket Dominio (modelo).  
2. Ticket Base de datos (migraciones).  
3. Ticket Caso de uso (aplicación).  
4. Ticket API.  
5. Ticket UI principal.  
6. Ticket validaciones y manejo de errores UI.  
7. Ticket tests automatizados (backend + frontend).

---

## 5. Checklist rápido antes de dar por lista la descomposición en tickets

- [ ] Cada ticket tiene un título claro y una descripción entendible.  
- [ ] Los tickets cubren todas las tareas mencionadas en la User Story.  
- [ ] No hay tickets excesivamente grandes (si los hay, subdividir).  
- [ ] Las dependencias entre tickets están identificadas.  
- [ ] Se han asignado prioridades y estimaciones de esfuerzo.  
- [ ] Cada ticket hace referencia explícita a la User Story origen.
