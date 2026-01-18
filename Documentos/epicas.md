# Épicas del MVP - Sistema de Gestión de Puntos de Venta para Joyería

Este documento describe las épicas principales del MVP (Fase 1) del sistema de gestión de puntos de venta para joyería. Cada épica agrupa funcionalidades relacionadas y contiene referencias a las User Stories que se desarrollarán en la carpeta `Documentos/Historias`.

---

## Épica 1: Gestión de Productos

**Descripción:**  
Permite gestionar el catálogo centralizado de productos de la joyería, incluyendo la creación, edición, importación desde Excel y asociación de fotos de referencia para el reconocimiento de imágenes.

**Alcance:**
- Importación masiva de productos desde archivos Excel con matching por SKU
- Creación y edición manual de productos (SKU, nombre, descripción, precio, colección)
- Gestión de fotos de referencia (subida, eliminación, ordenamiento, foto principal)
- Visualización del catálogo completo con fotos asociadas
- Validación de datos y manejo de errores en importaciones

**Entidades del modelo de datos relacionadas:**
- `Product` (SKU único, precio, descripción, colección)
- `ProductPhoto` (múltiples fotos por producto, ordenamiento, foto principal)
- `Collection` (agrupación opcional de productos)

**User Stories:**
- [HU-EP1-001: Importar productos desde Excel](Historias/HU-EP1-001.md)
- [HU-EP1-002: Crear producto manualmente](Historias/HU-EP1-002.md)
- [HU-EP1-003: Editar producto existente](Historias/HU-EP1-003.md)
- [HU-EP1-004: Subir fotos de referencia a producto](Historias/HU-EP1-004.md)
- [HU-EP1-005: Gestionar fotos de producto (eliminar, reordenar, marcar principal)](Historias/HU-EP1-005.md)
- [HU-EP1-006: Visualizar catálogo de productos](Historias/HU-EP1-006.md)
- [HU-EP1-007: Buscar productos por SKU o nombre](Historias/HU-EP1-007.md)

---

## Épica 2: Gestión de Inventario

**Descripción:**  
Gestiona la asignación de productos a puntos de venta y el stock de dichos productos, permitiendo asignaciones manuales, importaciones masivas, ajustes manuales y consultas del inventario actual.

**Regla de negocio clave:** La presencia de un registro en `Inventory` (independientemente de la cantidad) determina que el producto está asignado al punto de venta y es visible para los operadores asignados a dicho punto de venta.

**Alcance:**
- **Asignación manual de productos a puntos de venta** (crear registros en Inventory con cantidad inicial 0)
- **Desasignación de productos** de puntos de venta (soft delete con preservación de historial)
- Importación de stock desde Excel (suma a cantidades existentes, con asignación implícita si el producto no está en el inventario del POS)
- Visualización de stock por punto de venta
- Vista centralizada de stock total y por ubicación
- Ajustes manuales de inventario con trazabilidad
- Validación de stock no negativo

**Entidades del modelo de datos relacionadas:**
- `Inventory` (asignación de productos a POS y stock actual por producto y punto de venta)
- `InventoryMovement` (historial completo de movimientos con trazabilidad)
- `Product` (referencia al producto del catálogo global)
- `PointOfSale` (referencia al punto de venta)

**User Stories:**
- [HU-EP2-001: Importar stock desde Excel](Historias/HU-EP2-001.md)
- [HU-EP2-002: Visualizar stock por punto de venta](Historias/HU-EP2-002.md)
- [HU-EP2-003: Visualizar stock centralizado (todos los puntos de venta)](Historias/HU-EP2-003.md)
- [HU-EP2-004: Realizar ajuste manual de inventario](Historias/HU-EP2-004.md)
- [HU-EP2-005: Consultar historial de movimientos de inventario](Historias/HU-EP2-005.md)
- [HU-EP2-006: Asignar/desasignar productos a puntos de venta](Historias/HU-EP2-006.md)

---

## Épica 3: Registro de Ventas

**Descripción:**  
Permite registrar ventas en los puntos de venta, con actualización automática del inventario y trazabilidad completa de las transacciones.

**Alcance:**
- Registro de ventas con método de pago
- Actualización automática de stock al registrar venta
- Registro de foto asociada a la venta (cuando se usa reconocimiento de imagen)
- Validación de stock disponible antes de venta
- Validación de método de pago asignado al punto de venta
- Registro de usuario operador que realiza la venta

**Entidades del modelo de datos relacionadas:**
- `Sale` (venta con precio snapshot, cantidad, método de pago, fecha)
- `SalePhoto` (foto opcional asociada a la venta)
- `InventoryMovement` (movimiento automático generado por la venta)
- `PaymentMethod` y `PointOfSalePaymentMethod` (validación de métodos disponibles)
- `User` (operador que realiza la venta)

**User Stories:**
- [HU-EP3-001: Registrar venta con reconocimiento de imagen](Historias/HU-EP3-001.md) *(incluye validación de stock y selección de método de pago)*
- [HU-EP3-002: Registrar venta manual (sin foto)](Historias/HU-EP3-002.md) *(incluye validación de stock y selección de método de pago)*

---

## Épica 4: Reconocimiento de Imágenes con IA

**Descripción:**  
Sistema de identificación de productos mediante reconocimiento de imágenes usando inteligencia artificial, generando sugerencias ordenadas por precisión.

**Alcance:**
- Captura de foto del producto vendido
- Procesamiento de imagen mediante modelo de IA
- Generación de 3-5 sugerencias de productos ordenadas por precisión/confianza
- Visualización de sugerencias con fotos de referencia
- Validación manual del operador antes de confirmar

**Entidades del modelo de datos relacionadas:**
- `ProductPhoto` (fotos de referencia para entrenamiento/comparación)
- `SalePhoto` (foto capturada en el punto de venta)
- `Product` (productos candidatos sugeridos)

**Consideraciones técnicas:**
- Procesamiento de imágenes en cliente (móvil) usando TensorFlow.js/ONNX.js
- Modelo de IA entrenado con fotos de referencia de productos
- Generación de 3 sugerencias ordenadas por confianza/precisión
- Manejo de errores cuando no hay correspondencia fiable (redirigir a venta manual)
- Almacenamiento de fotos: sistema de archivos local en desarrollo, S3/Blob Storage en producción

**User Stories:**
- [HU-EP4-001: Reconocimiento de productos mediante imagen](Historias/HU-EP4-001.md) *(consolida captura, procesamiento, visualización y selección)*

---

## Épica 5: Gestión de Devoluciones

**Descripción:**  
Permite registrar devoluciones de productos vendidos, asociándolas a una o más ventas originales, incrementando el stock automáticamente y manteniendo trazabilidad completa. Soporta devoluciones parciales, categorización obligatoria y foto opcional.

**Alcance:**
- Registro de devolución asociada a una o más ventas originales (multi-venta)
- Soporte para devoluciones parciales (devolver parte de las unidades vendidas)
- Ventana de devolución de 30 días desde la venta
- Devolución obligatoria en el mismo punto de venta de la venta
- Categoría de devolución obligatoria (Defectuoso, Tamaño incorrecto, No satisfecho, Otro)
- Motivo de texto libre opcional (máximo 500 caracteres)
- Foto opcional de devolución (ej: foto del producto defectuoso)
- Incremento automático de stock en el punto de venta
- Generación automática de movimiento de inventario (tipo "Return")
- Validación de cantidad disponible (vendida - ya devuelta)
- Operadores pueden registrar devoluciones en sus puntos de venta asignados

**Entidades del modelo de datos relacionadas:**
- `Return` (devolución con cantidad, categoría y motivo)
- `ReturnSale` (relación muchos a muchos entre devoluciones y ventas, con cantidad y precio snapshot)
- `ReturnPhoto` (foto opcional asociada a la devolución)
- `InventoryMovement` (movimiento automático de tipo "Return")
- `Sale` (ventas originales referenciadas)
- `Inventory` (stock incrementado)

**User Stories:**
- [HU-EP5-001: Registrar devolución de producto vendido](Historias/HU-EP5-001.md)
- [HU-EP5-002: Buscar ventas elegibles para asociar devolución](Historias/HU-EP5-002.md)
- [HU-EP5-003: Consultar historial de devoluciones](Historias/HU-EP5-003.md)

---

## Épica 6: Gestión de Métodos de Pago

**Descripción:**  
Configuración y gestión de métodos de pago disponibles en el sistema, con asignación específica por punto de venta.

**Alcance:**
- Lista general de métodos de pago predefinidos (Efectivo, Bizum, Transferencia, Tarjetas TPV, PayPal)
- Asignación de métodos de pago a puntos de venta específicos
- Activación/desactivación de métodos por punto de venta
- Validación de métodos disponibles al registrar ventas

**Entidades del modelo de datos relacionadas:**
- `PaymentMethod` (métodos de pago generales con código único)
- `PointOfSalePaymentMethod` (relación muchos a muchos con activación/desactivación)

**User Stories:**
- [HU-EP6-001: Configurar métodos de pago disponibles en el sistema](Historias/HU-EP6-001.md)
- [HU-EP6-002: Asignar métodos de pago a punto de venta](Historias/HU-EP6-002.md)
- [HU-EP6-003: Activar/desactivar método de pago en punto de venta](Historias/HU-EP6-003.md)

---

## Épica 7: Autenticación y Gestión de Usuarios

**Descripción:**  
Sistema de autenticación y gestión de usuarios con roles (Administrador y Operador), incluyendo asignación de operadores a puntos de venta.

**Alcance:**
- Login con usuario y contraseña
- Gestión de roles (Admin con acceso completo, Operador con acceso restringido)
- Asignación de operadores a puntos de venta específicos
- Control de acceso basado en roles y asignaciones
- Gestión de usuarios (crear, editar, desactivar)

**Entidades del modelo de datos relacionadas:**
- `User` (usuarios con roles Admin/Operator, autenticación)
- `UserPointOfSale` (asignación de operadores a puntos de venta)
- `PointOfSale` (puntos de venta asignables)

**User Stories:**
- [HU-EP7-001: Login de usuario con usuario y contraseña](Historias/HU-EP7-001.md)
- [HU-EP7-002: Crear nuevo usuario](Historias/HU-EP7-002.md)
- [HU-EP7-003: Editar usuario existente](Historias/HU-EP7-003.md)
- [HU-EP7-004: Asignar operador a punto de venta](Historias/HU-EP7-004.md)
- [HU-EP7-005: Desasignar operador de punto de venta](Historias/HU-EP7-005.md)
- [HU-EP7-006: Control de acceso según rol y asignaciones](Historias/HU-EP7-006.md)

---

## Épica 8: Gestión de Puntos de Venta

**Descripción:**  
Permite crear, editar y gestionar los puntos de venta del sistema, incluyendo asignación de operadores y métodos de pago.

**Alcance:**
- Creación y edición de puntos de venta (nombre, código, dirección, teléfono, email)
- Asignación de operadores a puntos de venta
- Asignación de métodos de pago a puntos de venta
- Activación/desactivación de puntos de venta
- Visualización de puntos de venta disponibles según rol

**Entidades del modelo de datos relacionadas:**
- `PointOfSale` (información del punto de venta)
- `UserPointOfSale` (asignación de operadores)
- `PointOfSalePaymentMethod` (asignación de métodos de pago)

**User Stories:**
- [HU-EP8-001: Crear punto de venta](Historias/HU-EP8-001.md)
- [HU-EP8-002: Editar punto de venta existente](Historias/HU-EP8-002.md)
- [HU-EP8-003: Activar/desactivar punto de venta](Historias/HU-EP8-003.md)
- [HU-EP8-004: Visualizar puntos de venta disponibles](Historias/HU-EP8-004.md)

---

## Épica 9: Consultas y Reportes

**Descripción:**  
Proporciona funcionalidades de consulta y visualización de datos históricos de ventas e inventario, con filtros y búsquedas.

**Alcance:**
- Consulta de historial de ventas con filtros (punto de venta, fecha, producto, método de pago, operador)
- Visualización de detalles de ventas (foto, SKU, precio, método de pago, fecha, operador)
- Consulta de inventario con búsqueda de productos
- Visualización de movimientos de stock históricos
- Paginación de resultados para optimización

**Entidades del modelo de datos relacionadas:**
- `Sale` (con índices optimizados para consultas por punto de venta, producto, usuario, método de pago)
- `SalePhoto` (fotos asociadas a ventas)
- `Inventory` (stock actual)
- `InventoryMovement` (historial de movimientos)

**Optimizaciones:**
- Índices compuestos en `Sale` para consultas frecuentes
- Paginación obligatoria (máx 50 items por página)
- Filtros eficientes por fecha, punto de venta, producto

**User Stories:**
- [HU-EP9-001: Consultar historial de ventas con filtros](Historias/HU-EP9-001.md)
- [HU-EP9-002: Visualizar detalles de una venta](Historias/HU-EP9-002.md)
- [HU-EP9-003: Consultar inventario con búsqueda](Historias/HU-EP9-003.md)
- [HU-EP9-004: Consultar movimientos de inventario](Historias/HU-EP9-004.md)

---

## Resumen de Épicas

| Épica | Descripción Breve | User Stories Estimadas |
|-------|-------------------|------------------------|
| **EP1** | Gestión de Productos | 7 |
| **EP2** | Gestión de Inventario | 6 |
| **EP3** | Registro de Ventas | 2 |
| **EP4** | Reconocimiento de Imágenes con IA | 1 |
| **EP5** | Gestión de Devoluciones | 3 |
| **EP6** | Gestión de Métodos de Pago | 3 |
| **EP7** | Autenticación y Gestión de Usuarios | 6 |
| **EP8** | Gestión de Puntos de Venta | 4 |
| **EP9** | Consultas y Reportes | 4 |
| **TOTAL** | | **36** |

---

## Orden de Implementación

Este orden de implementación ha sido definido considerando las dependencias entre épicas y la necesidad de construir primero las bases del sistema antes de implementar funcionalidades más complejas. Este orden es **fundamental para generar los tickets de trabajo en el orden correcto de desarrollo**.

**Orden de implementación confirmado:**

1. **EP7**: Autenticación y Gestión de Usuarios (base del sistema)
   - Sin autenticación no se puede acceder al sistema
   - Los roles y permisos son necesarios para todas las demás funcionalidades
   - Base para control de acceso en el resto del sistema

2. **EP8**: Gestión de Puntos de Venta (necesario para el resto)
   - Los puntos de venta son necesarios para inventario, ventas y asignaciones
   - Debe estar disponible antes de asignar operadores o métodos de pago

3. **EP6**: Gestión de Métodos de Pago (necesario para ventas)
   - Los métodos de pago deben estar configurados antes de registrar ventas
   - Necesario para asignar métodos a puntos de venta

4. **EP1**: Gestión de Productos (necesario para ventas e inventario)
   - El catálogo de productos es base para inventario y ventas
   - Las fotos de productos son necesarias para reconocimiento de imágenes

5. **EP2**: Gestión de Inventario (necesario para ventas)
   - El stock debe estar gestionado antes de poder registrar ventas
   - Las validaciones de stock requieren inventario configurado

6. **EP3**: Registro de Ventas (funcionalidad principal)
   - Requiere: productos, inventario, métodos de pago y puntos de venta
   - Funcionalidad core del sistema

7. **EP4**: Reconocimiento de Imágenes con IA (mejora de ventas)
   - Mejora la experiencia de registro de ventas
   - Requiere productos con fotos de referencia (EP1)
   - Puede desarrollarse en paralelo con EP3 pero se integra después

8. **EP5**: Gestión de Devoluciones (complemento)
   - Requiere ventas registradas (EP3)
   - Funcionalidad complementaria que mejora la gestión completa

9. **EP9**: Consultas y Reportes (análisis)
   - Requiere datos existentes (ventas, inventario, devoluciones)
   - Funcionalidad de análisis que se beneficia de tener datos históricos

> **Nota importante:** Este orden debe respetarse al generar los tickets de trabajo para asegurar que las dependencias estén resueltas antes de implementar funcionalidades que las requieren.

---

## Notas

- Las User Stories se crearán siguiendo el formato definido en `Documentos/Procedimientos/Procedimiento-UserStories.md`
- Cada User Story tendrá su propio archivo en `Documentos/Historias/` con el formato `HU-EP[X]-[NNN].md`
- Las épicas están diseñadas para cubrir todos los casos de uso del MVP definidos en el README
- El modelo de datos está optimizado para soportar todas estas épicas de manera eficiente
- Las funcionalidades de Fase 2 (alertas, reportes avanzados, dashboard, etc.) no están incluidas en este MVP
- **Nota:** Algunas historias han sido consolidadas para evitar duplicación. Ver `Documentos/aclaraciones-tecnicas.md` para detalles.

