# Guía de Registro de Ventas

## Descripción General

El sistema ofrece dos métodos para registrar ventas:

1. **Registro Manual**: Selección directa de productos por SKU o nombre
2. **Reconocimiento de Imágenes con IA**: Captura de foto del producto para identificación automática

Ambos métodos comparten el mismo flujo de validación y confirmación.

---

## Registro Manual de Ventas

### Acceso
- Navegue a **Ventas** → **Nueva Venta** o directamente a `/sales/new`

### Flujo de Trabajo

1. **Seleccionar Producto**
   - Use el buscador para encontrar el producto por SKU o nombre
   - El sistema muestra el stock disponible en tiempo real

2. **Ingresar Cantidad**
   - Por defecto: 1 unidad
   - El sistema valida que la cantidad no exceda el stock disponible

3. **Seleccionar Método de Pago**
   - Solo se muestran métodos activos y asignados al punto de venta actual

4. **Agregar Notas (opcional)**
   - Campo de texto libre, máximo 500 caracteres

5. **Adjuntar Foto (opcional)**
   - Puede adjuntar una foto del producto incluso en registro manual
   - La foto se comprime automáticamente a JPEG (máx 2MB)

6. **Confirmar Venta**
   - Revise el resumen: producto, cantidad, precio unitario, total
   - Confirme la operación

### Validaciones

| Validación | Mensaje de Error |
|------------|------------------|
| Stock insuficiente | "Stock insuficiente. Disponible: X unidades" |
| Método de pago no disponible | "Método de pago no asignado a este punto de venta" |
| Operador no asignado al POS | "No tiene permisos para operar en este punto de venta" |

### Advertencia de Stock Bajo

Después de una venta exitosa, si el stock restante es bajo (≤5 unidades), se muestra una advertencia:
> "⚠️ Quedan solo X unidades de este producto"

Esta advertencia es informativa y no bloquea la operación.

---

## Registro con Reconocimiento de Imágenes

### Requisitos del Dispositivo

- **Navegador**: Chrome 90+, Edge 90+, Safari 14+, Firefox 88+
- **WebGL 2.0**: Requerido para aceleración GPU
- **Memoria RAM**: 2GB mínimo, 4GB+ recomendado
- **Cámara**: Acceso a cámara del dispositivo

### Acceso
- Navegue a **Ventas** → **Nueva Venta con IA** o directamente a `/sales/new/image`

### Flujo de Trabajo

1. **Verificación de Compatibilidad**
   - El sistema verifica automáticamente WebGL 2.0 y TensorFlow.js
   - Si el dispositivo no es compatible, se redirige a registro manual

2. **Captura de Foto**
   - Otorgue permisos de cámara cuando se solicite
   - Capture una foto clara del producto
   - Puede retomar la foto si no está satisfecho

3. **Validación de Imagen**
   - Tamaño mínimo: 200x200 píxeles
   - Proporción máxima: 5:1
   - La imagen no debe ser completamente negra o blanca

4. **Procesamiento con IA**
   - El modelo descarga automáticamente (primera vez ~14MB)
   - Se muestra progreso de procesamiento
   - Tiempo típico: 2-5 segundos

5. **Selección de Sugerencia**
   - El sistema muestra 3-5 sugerencias ordenadas por confianza
   - Cada sugerencia incluye: foto de referencia, SKU, nombre, % confianza
   - Seleccione el producto correcto

6. **Continuar con Registro**
   - Una vez seleccionado el producto, continúa el flujo normal
   - La foto capturada se adjunta automáticamente a la venta

### Umbral de Confianza

- **Umbral predeterminado**: 40%
- Si ninguna sugerencia alcanza el 40% de confianza:
  - Se muestra mensaje: "No se encontró correspondencia fiable"
  - Opciones: retomar foto o ir a registro manual
  - La foto se preserva para uso en registro manual

### Escenarios de Error

| Escenario | Comportamiento |
|-----------|----------------|
| Sin conexión a internet | "Se requiere conexión a internet" → Redirige a manual |
| Modelo no disponible (404) | "Modelo no disponible aún, entrenar primero" → Manual |
| Dispositivo incompatible | "Tu dispositivo no es compatible. Requisitos: iOS 12+, Android 8+, navegador moderno" |
| Todas las confianzas <40% | "No se encontró correspondencia fiable" → Opción de retomar o manual |

---

## Historial de Ventas

### Acceso
- Navegue a **Ventas** → **Historial** o `/sales/history`

### Filtros Disponibles

| Filtro | Descripción |
|--------|-------------|
| Rango de fechas | Por defecto: últimos 30 días |
| Producto | Filtrar por producto específico |
| Método de pago | Filtrar por método de pago |
| Operador | Solo visible para administradores |

### Información Mostrada

- Fecha y hora de la venta
- Producto (SKU y nombre)
- Cantidad
- Precio unitario y total
- Método de pago
- Operador que realizó la venta
- Indicador de foto adjunta (📷)

### Detalle de Venta

Haga clic en una venta para ver:
- Información completa
- Foto adjunta (si existe) con opción de ampliar
- Movimiento de inventario asociado

### Restricciones por Rol

| Rol | Acceso |
|-----|--------|
| Administrador | Ve todas las ventas de todos los puntos de venta |
| Operador | Solo ve ventas de puntos de venta asignados |

---

## Integración con Inventario

Cada venta crea automáticamente:
- Un movimiento de inventario tipo "Sale"
- Actualización inmediata del stock
- Registro de auditoría completo

La transacción es atómica: si algún paso falla, toda la operación se revierte.

---

## Preguntas Frecuentes

### ¿Puedo registrar ventas sin conexión a internet?
El registro manual funciona sin conexión si ya tiene la sesión iniciada. El reconocimiento de imágenes requiere conexión para descargar/verificar el modelo.

### ¿Qué pasa si dos operadores intentan vender el último producto?
El sistema usa doble validación de stock:
1. Validación al cargar el formulario
2. Validación dentro de la transacción antes de confirmar

Si el stock cambia entre ambas validaciones, se muestra el error:
> "Stock cambió. Disponible: X, Solicitado: Y"

### ¿Cómo mejoro la precisión del reconocimiento de imágenes?
- Capture fotos con buena iluminación
- Centre el producto en la imagen
- Evite fondos muy saturados
- Si el modelo tiene baja precisión, el administrador debe reentrenarlo
