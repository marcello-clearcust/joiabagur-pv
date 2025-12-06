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

**Registro de Ventas:**
- Captura de ventas por punto de venta
- Asociación automática de fotos a cada transacción
- Registro de método de pago por cada venta
- Historial completo de ventas con trazabilidad

**Reconocimiento de Productos con IA:**
- Identificación de productos a partir de fotografías tomadas en el punto de venta
- Generación de 3-5 sugerencias ordenadas por precisión
- Validación manual del operador antes de confirmar la venta
- Reducción de errores en la clasificación de productos vendidos

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

- **Backend y Frontend separados** para facilitar el mantenimiento y escalabilidad
- **Despliegue en capa free-tier** de AWS, Azure u otro servicio similar
- **Optimizado para móviles** para operadores en puntos de venta
- **Responsive** para administradores en cualquier dispositivo

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

