# Procedimiento para generar y priorizar User Stories

Este documento define un **procedimiento reutilizable** y un **formato estándar** para crear y ordenar User Stories, siguiendo el enfoque aplicado en el sistema LTI (LTI-MO), pero siendo independiente del dominio.

---

## 1. Objetivo

Disponer de una guía práctica para:

- Transformar casos de uso, requisitos o flujos de negocio en **User Stories bien formadas**.
- Asegurar una **estructura homogénea** para todo el backlog.
- Priorizar las User Stories según:
  - Puntos de historia (esfuerzo).
  - Impacto en el usuario / valor de negocio.
  - Urgencia (mercado / feedback).
  - Complejidad.
  - Dependencias entre historias.

---

## 2. Formato estándar de User Story

Cada User Story debe seguir esta estructura base:

1. **Formato estándar (One-liner)**  
   > *Como* `[tipo de usuario]`, *quiero* `[realizar una acción]` *para* `[obtener un beneficio]`.

2. **Descripción**  
   - Explicación en lenguaje natural de la funcionalidad.  
   - Contexto de negocio y ejemplos de uso.

3. **Criterios de Aceptación** (formato BDD recomendado)  
   - Lista de casos en formato:  
     - **Dado que** `[contexto inicial]`  
       **cuando** `[acción realizada]`  
       **entonces** `[resultado esperado]`  
   - Cubrir casos positivos, negativos y bordes relevantes.

4. **Notas adicionales**  
   - Matices funcionales no capturados en los criterios.  
   - Limitaciones conocidas, decisiones de diseño, TODOs.

5. **Tareas**  
   - Lista de tareas de alto nivel necesarias para completar la historia (idealmente independientes de implementación concreta).  

6. **Estimaciones y atributos de priorización**  
   - **Puntos de historia (Story Points, Fibonacci)**  
   - **Impacto en usuario / Valor de negocio** (1–5).  
   - **Urgencia (mercado / feedback)** (1–5).  
   - **Complejidad / Esfuerzo** (1–5).  
   - **Riesgos y dependencias** (texto libre).

> Recomendación: dejar la estimación (puntos, impacto, urgencia, complejidad) para una **sesión específica de refinamiento**, no durante el primer borrador de la historia.

---

## 3. Procedimiento para generar User Stories a partir de casos de uso

### Paso 1 – Identificar actores y objetivos

- Extraer de los casos de uso:
  - **Actores principales** (ej. Reclutador, Manager, Candidato, Administrador).  
  - **Objetivo de negocio** de cada actor (qué quiere lograr).

### Paso 2 – Transformar objetivos en frases "Como… quiero… para…"

Para cada combinación *actor + objetivo*:

- Redactar la historia base en el formato:  
  > *Como* `[actor]`, *quiero* `[acción]` *para* `[beneficio medible / claro]`.

Ejemplos típicos:
- Como Reclutador, quiero crear ofertas a partir de plantillas para reducir el tiempo de publicación.  
- Como Manager, quiero registrar feedback de entrevistas para comparar candidatos mejor.

### Paso 3 – Aclarar dudas y validar con stakeholders

Antes de profundizar en los detalles de cada User Story, es fundamental **interactuar con los stakeholders** (product owner, usuarios finales, expertos de dominio) para:

- **Validar el entendimiento** de los objetivos y necesidades del negocio.
- **Aclarar ambigüedades** en los casos de uso o requisitos originales.
- **Confirmar el alcance** de cada historia (qué incluye y qué no).
- **Identificar casos de uso adicionales** que no estaban documentados inicialmente.
- **Verificar restricciones** técnicas, de negocio o regulatorias que puedan afectar la implementación.
- **Confirmar prioridades** relativas entre diferentes historias.

**Formato sugerido para la sesión de aclaraciones:**

1. Presentar las User Stories generadas en el Paso 2 (formato "Como… quiero… para…").
2. Para cada historia, plantear preguntas abiertas como:
   - ¿Esta historia captura correctamente tu necesidad?
   - ¿Hay algún escenario o caso de uso que falta?
   - ¿Qué validaciones o reglas de negocio específicas debemos considerar?
   - ¿Existen dependencias con otros procesos o sistemas?
   - ¿Hay restricciones de tiempo, seguridad o cumplimiento que debamos tener en cuenta?
3. Documentar las respuestas y actualizar las historias según corresponda.
4. Iterar hasta alcanzar un consenso sobre el alcance y los detalles de cada historia.

> **Nota importante:** Este paso puede requerir múltiples iteraciones. No es necesario resolver todas las dudas antes de continuar, pero sí es recomendable aclarar los aspectos críticos que puedan afectar significativamente el diseño o la implementación.

### Paso 4 – Detallar la descripción

- Añadir contexto:
  - ¿Cuándo se usa la funcionalidad?  
  - ¿En qué parte del flujo de negocio encaja?  
  - ¿Qué sistemas o módulos están implicados? (puede mencionarse el modelo C4: Dominio, Aplicación, SPA, APIs, etc.).

### Paso 5 – Definir criterios de aceptación (Given/When/Then)

- Definir al menos:
  - 2–3 **escenarios felices** (happy path).  
  - 1–2 **escenarios de error o borde** (validaciones, permisos, estados inválidos).

### Paso 6 – Añadir tareas de alto nivel

- Listar las tareas principales que luego se transformarán en **tickets de trabajo** (por ejemplo, separar por capas según C4: dominio, BD, casos de uso, API, UI).

### Paso 7 – Estimar y completar atributos de priorización

En una sesión de refinamiento con el equipo:

1. Estimar **puntos de historia (Fibonacci)**.  
2. Asignar **Impacto** (1–5), **Urgencia** (1–5), **Complejidad** (1–5).  
3. Documentar **Riesgos / Dependencias** (por ejemplo: “Depende de tener modelo de Candidato definido”).

---

## 4. Formato para la matriz de priorización de User Stories

Se recomienda mantener una tabla de backlog priorizado como la siguiente:

| Orden | ID   | User Story                                                     | Puntos | Impacto (1–5) | Urgencia (1–5) | Complejidad (1–5) | Dependencias clave                  |
|-------|------|-----------------------------------------------------------------|--------|---------------|----------------|-------------------|--------------------------------------|
| 1     | HU1  | Crear oferta de empleo a partir de plantillas                  | 5      | 5             | 5              | 3                 | Base para HU2, HU3                  |
| 2     | HU4  | Visualizar pipeline por etapas                                 | 5      | 5             | 5              | 3                 | Base para HU5, HU6                  |
| ...   | ...  | ...                                                             | ...    | ...           | ...            | ...               | ...                                  |

> Nota: la tabla anterior es un **ejemplo de formato**, no una lista fija.

---

## 5. Regla de dependencias para ordenar User Stories

Para ordenar las User Stories:

1. Ordenar primero por **Impacto** y **Urgencia** (de mayor a menor).  
2. Ajustar por **Complejidad** (si dos historias tienen impacto/urgencia similares, priorizar la menos compleja).  
3. Aplicar la **regla de dependencias**:

> Si una historia A es más prioritaria pero **depende** de otra historia B, entonces B debe aparecer **antes** en el orden de backlog.

Ejemplos:
- Si HU11 (comunicaciones automáticas por etapa) depende de HU5 (movimiento entre etapas), HU5 debe priorizarse por encima de HU11.  
- Si HU2 (publicar ofertas en canales) depende de HU1 (crear ofertas), HU1 debe ejecutarse primero.

---

## 6. Checklist rápido para revisar una User Story

Antes de dar por “lista para desarrollo” una User Story:

- [ ] Tiene la frase “Como…, quiero…, para…”.  
- [ ] Descripción suficiente para que el equipo la entienda sin el analista al lado.  
- [ ] Criterios de aceptación en formato Given/When/Then.  
- [ ] Tareas de alto nivel identificadas.  
- [ ] Puntos de historia estimados.  
- [ ] Impacto, Urgencia y Complejidad definidos.  
- [ ] Dependencias claras con otras User Stories.  
- [ ] No es demasiado grande (si lo es, dividir en varias historias).
