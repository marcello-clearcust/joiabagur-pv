# Sistema de Gestión de Puntos de Venta para Joyería

## Descripción General del Proyecto

Sistema de gestión integral para una joyería que opera en múltiples puntos de venta (propios y de terceros como hoteles y otras tiendas). La aplicación permite gestionar inventario, registrar ventas y facilitar la identificación de productos mediante reconocimiento de imágenes con inteligencia artificial.

### Características Principales

**Gestión de Productos e Inventario:**
- Catálogo centralizado de productos con información detallada (SKU, precio, descripción, colección)
- Gestión de stock por punto de venta con vista centralizada
- Importación y actualización de datos desde archivos Excel
- Edición manual de productos e inventario desde la plataforma web
- Asociación de fotos de referencia a cada producto para reconocimiento de imágenes

**Registro de Ventas:** ✅ **IMPLEMENTADO**
- Captura de ventas por punto de venta (manual o con IA)
- Asociación automática de fotos a cada transacción
- Registro de método de pago por cada venta
- Historial completo de ventas con trazabilidad
- Validación de stock en tiempo real antes de confirmar venta
- Actualización automática e inventario en transacción atómica
- Alertas de stock bajo (no bloqueantes)

**Reconocimiento de Productos con IA:** ✅ **IMPLEMENTADO**
- **Inferencia client-side** con TensorFlow.js (sin costos de servidor)
- Identificación de productos mediante cámara móvil en el punto de venta
- Generación de 3-5 sugerencias ordenadas por confianza (umbral 40%)
- Validación manual del operador antes de confirmar la venta
- Fallback automático a entrada manual si confianza baja
- Reducción de errores en la clasificación de productos vendidos

**Entrenamiento del Modelo de IA:** ✅ **INNOVACIÓN**
- **Entrenamiento en navegador** usando TensorFlow.js (sin Python)
- Un clic desde el dashboard de administración
- Aceleración GPU mediante WebGL 2.0
- Métricas de salud del modelo con alertas automáticas
- Progreso en tiempo real (epoch por epoch)
- Costo: $0 (usa GPU del administrador, no del servidor)

**Gestión de Métodos de Pago:**
- Lista general de métodos de pago disponibles (Efectivo, Bizum, Transferencia bancaria, Tarjeta TPV propio, Tarjeta TPV punto de venta, PayPal)
- Asignación de métodos de pago específicos a cada punto de venta
- Registro de método de pago en cada transacción

**Gestión de Usuarios:**
- Sistema de roles: Administradores (acceso completo) y Operadores (asociados a puntos de venta específicos)
- Autenticación mediante usuario y contraseña

**Funcionalidades Adicionales:**
- Gestión de devoluciones
- Ajustes manuales de inventario
- Historial detallado de ventas y movimientos de stock

### Arquitectura Técnica

- **Backend**: .NET 10 con Entity Framework Core, PostgreSQL
- **Frontend**: React 19 + TypeScript + Vite + TensorFlow.js
- **Despliegue en AWS** optimizado para free-tier (App Runner, RDS PostgreSQL, S3, CloudFront)
- **CI/CD automatizado** con GitHub Actions
- **Machine Learning**: TensorFlow.js con MobileNetV2 (transfer learning)
- **Entrenamiento**: Browser-based (0 dependencias Python, GPU via WebGL)
- **Gestión segura de secretos** con AWS Secrets Manager
- **Backups automáticos** de base de datos con retención de 7 días
- **Optimizado para móviles** - Cámara, gestos táctiles, inferencia local
- **Responsive** para administradores en cualquier dispositivo
- **Testing**: xUnit + Testcontainers (backend), Vitest + React Testing Library (frontend)
- **Moneda**: Euro (EUR, €) con formato español (es-ES)

---

## 🚀 Estado de Implementación

### ✅ Funcionalidades Implementadas (MVP Complete)

#### Gestión de Productos e Inventario
- ✅ Catálogo de productos con fotos de referencia
- ✅ Importación desde Excel
- ✅ Gestión de stock por punto de venta
- ✅ Vista centralizada de inventario
- ✅ Ajustes manuales de stock

#### Registro de Ventas
- ✅ **Venta Manual** (`/sales/new`)
  - Búsqueda de productos por SKU/nombre
  - Validación de stock en tiempo real
  - Selección de método de pago
  - Actualización automática de inventario
  - Foto opcional
  
- ✅ **Venta con IA** (`/sales/new/image`)
  - Captura de foto desde cámara móvil
  - Inferencia TensorFlow.js (client-side)
  - 3-5 sugerencias con % de confianza
  - Fallback automático a manual si confianza <40%
  - Validación de foto (dimensiones, brillo, ratio)

- ✅ **Historial de Ventas** (`/sales/history`)
  - Filtros: fecha, POS, producto, método de pago
  - Paginación (50 items/página)
  - Visualizador de fotos
  - Control de acceso por rol

#### Reconocimiento de Imágenes con IA
- ✅ **Dashboard de Modelo** (`/admin/ai-model` - Admin only)
  - Métricas de salud del modelo
  - Alertas de reentrenamiento (🔴 CRITICAL, 🟠 HIGH, 🟡 RECOMMENDED)
  - Historial de versiones
  - Estadísticas de catálogo y fotos

- ✅ **Entrenamiento en Navegador**
  - Un clic para entrenar
  - Descarga automática de fotos de productos
  - Transfer learning con MobileNetV2
  - Progreso en tiempo real (epoch/accuracy/loss)
  - Subida automática a servidor
  - Duración: 15-60 min (según GPU)

#### Gestión de Usuarios y Métodos de Pago
- ✅ Sistema de autenticación JWT
- ✅ Roles: Administrator y Operator
- ✅ Control de acceso por punto de venta
- ✅ Gestión de métodos de pago por POS

### 📊 Métricas de Calidad

- **Tests Backend**: 17/17 integration tests passing ✅
- **Cobertura Backend**: Critical paths 100% tested
- **Tests Frontend**: Manual testing complete, unit tests in progress
- **Documentación**: Implementation summary, OpenSpec docs, code comments

### 🔜 En Desarrollo

- Unit tests para servicios frontend
- E2E tests con Playwright
- CSV export para historial de ventas
- Checkpoint recovery para entrenamiento
- Documentación de usuario detallada

---

## Índice de Documentación

Esta sección proporciona acceso rápido a toda la documentación técnica del proyecto.

### Documentación Principal

- **[Épicas del MVP](Documentos/epicas.md)**: Descripción detallada de las épicas principales del MVP, incluyendo User Stories, orden de implementación y dependencias entre épicas.

- **[Arquitectura del Sistema](Documentos/arquitectura.md)**: Documentación completa de la arquitectura técnica, incluyendo stack tecnológico, configuración de desarrollo y producción, flujos de datos principales y consideraciones de seguridad.

- **[Modelo de Datos](Documentos/modelo-de-datos.md)**: Especificación completa del modelo de datos, incluyendo diagramas ER, descripción de entidades, relaciones, índices y optimizaciones para free-tier.

- **[Modelo C4](Documentos/modelo-c4.md)**: Arquitectura del sistema utilizando el modelo C4, proporcionando diferentes niveles de abstracción desde el contexto general hasta los componentes internos del backend y frontend.

- **[Testing Backend](Documentos/testing-backend.md)**: Guía completa de testing para el backend .NET 10 (xUnit, Moq, FluentAssertions), incluyendo tests unitarios, integración, CI/CD y guías detalladas por tema.

- **[Testing Frontend](Documentos/testing-frontend.md)**: Guía completa de testing para el frontend React 19 + TypeScript + Vite (Vitest, React Testing Library, MSW, Playwright), incluyendo tests unitarios, de componentes, E2E y guías detalladas por tema.

### Procedimientos

Documentación de procedimientos y metodologías de trabajo del proyecto:

- **[Procedimiento de User Stories](Documentos/Procedimientos/Procedimiento-UserStories.md)**: Metodología y formato para la creación y gestión de User Stories.

- **[Procedimiento de Tickets de Trabajo](Documentos/Procedimientos/Procedimiento-TicketsTrabajo.md)**: Proceso para la creación, asignación y seguimiento de tickets de trabajo.

### Propuestas

Documentos de propuestas técnicas y análisis:

- **[Aclaraciones Técnicas](Documentos/Propuestas/aclaraciones-tecnicas.md)**: Aclaraciones y decisiones técnicas importantes del proyecto.

- **[Análisis de Metronic Frontend](Documentos/Propuestas/analisis-metronic-frontend.md)**: Análisis y evaluación del framework Metronic para el frontend.

- **[Análisis y Migración Swagger a Scalar](Documentos/Propuestas/analisis-swagger.md)**: Análisis técnico y migración exitosa de Swagger/Swashbuckle a Scalar por incompatibilidad con .NET 10.

- **[Arquitecturas Propuestas](Documentos/Propuestas/arquitecturas-propuestas.md)**: Diferentes propuestas arquitectónicas evaluadas para el proyecto.

- **[Comparación AWS vs Azure](Documentos/Propuestas/comparacion-aws-azure-deploy.md)**: Análisis detallado de ambas plataformas cloud para el deploy de producción, con pros/contras, costos estimados y recomendación final.

### Guías de Implementación

Guías paso a paso para configuración y despliegue:

- **[Guía de Deploy AWS](Documentos/Guias/deploy-aws-production.md)**: Instrucciones completas para desplegar la aplicación en AWS (App Runner, RDS PostgreSQL, S3, CloudFront), incluyendo configuración de backups, CI/CD con GitHub Actions, y gestión de secretos.

- **[Resumen de Implementación](OPENSPEC_IMPLEMENTATION_SUMMARY.md)**: Resumen completo de la implementación de Ventas y Reconocimiento de Imágenes, incluyendo decisiones arquitectónicas, estado actual y siguientes pasos.

---

## 🚀 Quick Start

### Requisitos Previos

**Backend:**
- .NET 10 SDK
- PostgreSQL 14+
- Docker (opcional, para desarrollo)

**Frontend:**
- Node.js 20+ y npm
- Navegador moderno (Chrome 90+, Edge 90+, Safari 14+)

### Instalación y Ejecución

**1. Backend:**
```bash
cd backend/src/JoiabagurPV.API
dotnet restore
dotnet run
```
API disponible en: `http://localhost:5000`

**2. Frontend:**
```bash
cd frontend
npm install --legacy-peer-deps  # Due to React 19 compatibility
npm run dev
```
UI disponible en: `http://localhost:5173`

**3. Usuario por defecto:**
- Usuario: `admin`
- Contraseña: `Admin123!`
- ⚠️ Cambiar contraseña después del primer login

### Primeros Pasos

1. **Subir fotos de productos** (`/products`)
   - Necesitas al menos 3-5 fotos por producto
   - Fotos claras desde diferentes ángulos
   
2. **Entrenar modelo de IA** (`/admin/ai-model`)
   - Clic en "Entrenar Modelo"
   - Mantén la pestaña abierta (15-60 min)
   - Verás progreso en tiempo real

3. **Registrar primera venta**
   - Opción 1: `/sales/new` (manual)
   - Opción 2: `/sales/new/image` (con IA)

### Testing

**Backend:**
```bash
cd backend/src/JoiabagurPV.Tests
dotnet test
```

**Frontend:**
```bash
cd frontend
npm run test
```

---

## Casos de Uso - MVP (Fase 1)

### Gestión de Productos

1. **Importar productos desde Excel**
   - Cargar archivo Excel con listado de productos
   - Actualizar productos existentes haciendo match por SKU
   - Crear nuevos productos para SKUs no existentes
   - Validar datos antes de confirmar importación
   - **Asociar fotos a productos**: Después de la importación, permitir subir fotos de referencia para cada producto (esencial para el reconocimiento de imágenes)
   - Posibilidad de subir múltiples fotos por producto para mejorar la precisión del reconocimiento

2. **Crear/Editar producto manualmente**
   - Agregar nuevo producto con SKU, precio, descripción, colección
   - **Subir y asociar fotos de referencia** al producto (obligatorio para poder usar reconocimiento de imágenes)
   - Editar información de productos existentes
   - Gestionar fotos asociadas: agregar, eliminar o reemplazar fotos de productos existentes
   - Visualizar catálogo completo de productos con sus fotos asociadas

3. **Gestionar stock por punto de venta**
   - Importar stock desde Excel (sumar a cantidades existentes)
   - Visualizar stock actual por punto de venta
   - Vista centralizada de stock total y por ubicación
   - Realizar ajustes manuales de inventario

4. **Gestionar métodos de pago por punto de venta**
   - Configurar métodos de pago disponibles desde lista general (Efectivo, Bizum, Transferencia bancaria, Tarjeta TPV propio, Tarjeta TPV punto de venta, PayPal)
   - Asignar métodos de pago específicos a cada punto de venta
   - Cada punto de venta solo mostrará los métodos de pago asignados al registrar ventas

### Gestión de Ventas

5. **Registrar venta con reconocimiento de imagen**
   - Operador toma foto del producto vendido
   - Sistema genera 3-5 sugerencias de productos ordenadas por precisión
   - Operador selecciona el producto correcto
   - Operador selecciona método de pago (de los disponibles para ese punto de venta)
   - Sistema registra venta con: punto de venta, SKU, precio, foto, método de pago, usuario que realizó la venta
   - Actualización automática de stock

6. **Registrar venta manual (sin foto)**
   - Operador busca producto por SKU o descripción
   - Selecciona producto
   - Operador selecciona método de pago (de los disponibles para ese punto de venta)
   - Sistema registra venta con: punto de venta, SKU, precio, método de pago, usuario que realizó la venta
   - Actualización de stock correspondiente

7. **Gestionar devoluciones**
   - Registrar devolución de producto vendido
   - Incrementar stock al punto de venta correspondiente
   - Asociar devolución a venta original

### Gestión de Usuarios y Puntos de Venta

8. **Autenticación de usuarios**
   - Login con usuario y contraseña
   - Diferentes permisos según rol (admin/operador)
   - Operadores asociados a puntos de venta específicos

9. **Gestionar puntos de venta**
   - Crear/editar puntos de venta
   - Asignar operadores a puntos de venta
   - **Asignar métodos de pago disponibles a cada punto de venta** (solo administradores)
   - Administradores pueden acceder a todos los puntos de venta

### Consultas y Reportes

10. **Consultar historial de ventas**
    - Filtrar ventas por punto de venta, fecha, producto, método de pago
    - Visualizar detalles de cada venta (foto, SKU, precio, método de pago, fecha, operador)
    - Ver historial de movimientos de stock

11. **Consultar inventario**
    - Vista de stock por punto de venta
    - Vista centralizada de stock total
    - Búsqueda de productos en catálogo

---

## Casos de Uso - Segunda Fase (Fase 2)

### Reportes Avanzados

12. **Exportar reportes**
    - Generar reportes de ventas en formato Excel/PDF
    - Reportes de inventario por punto de venta
    - Reportes de productos más vendidos
    - Reportes por método de pago
    - Reportes personalizados con filtros avanzados

### Alertas y Notificaciones

13. **Alertas de stock bajo**
    - Configurar umbrales mínimos de stock por producto
    - Notificaciones automáticas cuando el stock está bajo
    - Alertas por punto de venta o centralizadas

### Mejoras en Reconocimiento de Imágenes

14. **Mejora continua del modelo de IA**
    - Entrenamiento incremental con nuevas fotos
    - Feedback del usuario para mejorar precisión
    - Análisis de productos con baja precisión en reconocimiento

### Funcionalidades Adicionales

15. **Gestión de colecciones**
    - Agrupar productos por colección
    - Reportes por colección
    - Filtros y búsquedas por colección

16. **Gestión de precios**
    - Historial de cambios de precio
    - Precios diferentes por punto de venta (si aplica)
    - Promociones y descuentos

17. **Dashboard analítico**
    - Gráficos de ventas por período
    - Productos más vendidos
    - Rendimiento por punto de venta
    - Análisis de ventas por método de pago
    - Métricas de precisión del reconocimiento de imágenes

18. **Sincronización offline**
    - Modo offline para operadores
    - Sincronización automática cuando hay conexión
    - Prevención de pérdida de datos

19. **API para integraciones**
    - API REST para integraciones futuras
    - Webhooks para eventos importantes
    - Integración con sistemas de contabilidad

