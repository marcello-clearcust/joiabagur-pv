# Aclaraciones Técnicas para el MVP

Este documento recopila las decisiones técnicas y aclaraciones acordadas antes de generar las User Stories del MVP.

---

## 1. Roles y Permisos

### Roles Confirmados:
- **Administrador**: Acceso completo al sistema. Puede gestionar productos, inventario, usuarios, puntos de venta, métodos de pago, ventas, devoluciones y consultas.
- **Operador**: Acceso restringido a sus puntos de venta asignados. Puede registrar ventas, consultar inventario de sus puntos de venta asignados. **Nota**: En Fase 2, los operadores también podrán gestionar devoluciones.

---

## 2. Formato de Importación Excel

### Formato para Importación de Productos

**Archivo Excel requerido:** `.xlsx` o `.xls`

**Columnas obligatorias (según modelo de datos `Product`):**
| Columna | Tipo | Descripción | Validación |
|---------|------|-------------|------------|
| SKU | Texto | Código único del producto | Obligatorio, único, no puede estar vacío |
| Name | Texto | Nombre del producto | Obligatorio, no puede estar vacío |
| Description | Texto | Descripción del producto | Opcional |
| Price | Decimal | Precio del producto | Obligatorio, debe ser > 0 |
| Collection | Texto | Nombre de la colección | Opcional (si no existe, se puede crear o dejar null) |

**Ejemplo de estructura:**
```
| SKU      | Name              | Description           | Price  | Collection |
|----------|-------------------|-----------------------|--------|------------|
| JOY-001  | Anillo Plata      | Anillo de plata 925   | 150.00 | Clásicos   |
| JOY-002  | Collar Oro        | Collar de oro 18k    | 500.00 | Premium    |
```

**Comportamiento:**
- Si el SKU existe: se actualiza el producto (Name, Description, Price, Collection)
- Si el SKU no existe: se crea un nuevo producto
- Validaciones antes de confirmar importación:
  - SKU único en el archivo
  - SKU no vacío
  - Name no vacío
  - Price numérico y > 0
- Errores: mostrar lista de filas con errores y permitir corrección antes de importar

---

### Formato para Importación de Stock (Inventario)

**Archivo Excel requerido:** `.xlsx` o `.xls`

**Columnas obligatorias (según modelo de datos `Inventory`):**
| Columna | Tipo | Descripción | Validación |
|---------|------|-------------|------------|
| SKU | Texto | Código del producto | Obligatorio, debe existir en catálogo |
| Quantity | Entero | Cantidad a sumar al stock | Obligatorio, debe ser >= 0 |

**Ejemplo de estructura:**
```
| SKU      | Quantity |
|----------|----------|
| JOY-001  | 5        |
| JOY-002  | 10       |
```

**Comportamiento:**
- El punto de venta se selecciona en la interfaz antes de subir el archivo
- Si el producto ya tiene stock en ese punto de venta: se suma la cantidad (Quantity actual + Quantity del Excel)
- Si el producto no tiene stock en ese punto de venta: se crea registro con Quantity del Excel
- Validaciones:
  - SKU debe existir en catálogo de productos
  - Quantity debe ser numérico y >= 0
- Errores: mostrar lista de filas con errores (SKU no encontrado, Quantity inválido)

---

## 3. Reconocimiento de Imágenes con IA

### Estrategia: Procesamiento en Cliente (Móvil)

**Decisión técnica:** El procesamiento de imágenes se realizará en el dispositivo cliente (móvil) para:
- Reducir costos de servicios cloud (free-tier)
- Mejorar velocidad de respuesta (sin latencia de red)
- Funcionar offline (después de descargar el modelo)

### Tecnología Propuesta:

**Opción Recomendada: TensorFlow.js / ONNX.js**
- **TensorFlow.js**: Framework JavaScript que permite ejecutar modelos ML en navegadores y dispositivos móviles
- **ONNX.js**: Alternativa más ligera, compatible con modelos ONNX
- **Ventajas:**
  - No requiere servicios cloud para inferencia
  - Modelo se descarga una vez y se ejecuta localmente
  - Compatible con React Native / Flutter mediante wrappers
  - Optimizado para móviles

**Modelo de IA:**
- Modelo de clasificación de imágenes entrenado con las fotos de referencia de productos (`ProductPhoto`)
- El modelo se entrena en backend y se exporta para uso en cliente
- Se actualiza periódicamente cuando se añaden nuevas fotos de productos

### Flujo de Reconocimiento:

1. **Captura de foto** en dispositivo móvil
2. **Preprocesamiento** en cliente (redimensionar, normalizar)
3. **Inferencia local** con modelo TensorFlow.js/ONNX.js
4. **Generación de 3 sugerencias** ordenadas por confianza/precisión
5. **Visualización** de sugerencias con fotos de referencia
6. **Selección manual** del operador
7. **Envío al backend** solo de la venta confirmada (no la imagen procesada)

### Manejo de Errores:

**Cuando no hay correspondencia fiable (< umbral de confianza):**
- Mostrar mensaje: "No se encontró una correspondencia fiable para esta imagen"
- Ofrecer 2 opciones:
  1. **Tomar otra foto**: Volver a capturar imagen
  2. **Registrar venta manual**: Redirigir a funcionalidad de venta manual (HU-EP3-002)

**Umbral de confianza sugerido:** 0.6 (60%) - configurable

---

## 4. Gestión de Fotos

### Especificaciones Técnicas:

**Formatos permitidos:**
- JPG / JPEG
- PNG
- **Excluidos:** RAW, Bitmap (BMP)

**Límites:**
- **Tamaño máximo:** Sin límite inicial o límite elevado (ej: 50MB por foto)
- **Nota:** En producción, considerar compresión automática antes de subir

### Estrategia de Almacenamiento: Desarrollo Local vs Producción

**Problema:** En desarrollo local no tenemos acceso a S3/Blob Storage.

**Solución Propuesta - Estrategia Dual:**

#### Desarrollo Local:
- **Almacenamiento en sistema de archivos local** (`/uploads` o similar)
- Configuración mediante variable de entorno `STORAGE_TYPE=local`
- Ruta base configurable: `STORAGE_PATH=./uploads`
- Estructura de carpetas:
  ```
  uploads/
    products/
      {productId}/
        {photoId}.jpg
    sales/
      {saleId}/
        {photoId}.jpg
  ```

#### Producción:
- **Almacenamiento en S3/Blob Storage** (AWS S3, Azure Blob Storage)
- Configuración mediante variable de entorno `STORAGE_TYPE=s3` o `STORAGE_TYPE=azure`
- Credenciales y configuración mediante variables de entorno
- Implementar abstracción (patrón Strategy/Repository) para cambiar entre almacenamientos sin modificar código

**Implementación:**
```csharp
// Interfaz común
public interface IFileStorageService
{
    Task<string> UploadFileAsync(Stream fileStream, string fileName, string folder);
    Task<Stream> DownloadFileAsync(string filePath);
    Task DeleteFileAsync(string filePath);
    string GetFileUrl(string filePath);
}

// Implementaciones: LocalFileStorageService, S3FileStorageService, AzureBlobStorageService
```

**Ventajas:**
- Desarrollo sin dependencias externas
- Fácil migración a producción
- Testing simplificado
- Mismo código para ambos entornos

---

## 5. Consolidación de User Stories

### Historias Consolidadas:

#### Épica 3 - Registro de Ventas:
- ✅ **HU-EP3-001**: Registrar venta con reconocimiento de imagen
  - **Incluye:** Validación de stock disponible + Selección de método de pago
- ✅ **HU-EP3-002**: Registrar venta manual (sin foto)
  - **Incluye:** Validación de stock disponible + Selección de método de pago
- ❌ **HU-EP3-003**: ~~Validar stock disponible antes de venta~~ (consolidada)
- ❌ **HU-EP3-004**: ~~Seleccionar método de pago en venta~~ (consolidada)

**Nota de implementación:** Crear servicios compartidos para validación de stock y selección de método de pago, evitando duplicación de código.

#### Épica 4 - Reconocimiento de Imágenes:
- ✅ **HU-EP4-001**: Reconocimiento de productos mediante imagen
  - **Consolida:** HU-EP4-001, HU-EP4-002, HU-EP4-003, HU-EP4-004
  - **Incluye:** Captura, procesamiento, visualización de sugerencias y selección

---

## 6. Gestión de Colecciones

**Decisión:** Las colecciones son **opcionales** en el MVP.

**Alcance MVP:**
- Los productos pueden tener una colección asignada (campo opcional)
- Las colecciones se gestionan **manualmente** fuera del sistema o mediante scripts SQL
- No hay interfaz de usuario para crear/editar colecciones en el MVP
- En importación de productos, si se especifica una colección que no existe, se puede:
  - Opción A: Crear automáticamente la colección
  - Opción B: Dejar el campo CollectionId como null y mostrar advertencia

**Recomendación:** Opción A (crear automáticamente) para facilitar la importación inicial.

---

## 7. Priorización de Épicas

**Orden de implementación confirmado:**

1. **EP7**: Autenticación y Gestión de Usuarios (base del sistema)
2. **EP8**: Gestión de Puntos de Venta (necesario para el resto)
3. **EP6**: Gestión de Métodos de Pago (necesario para ventas)
4. **EP1**: Gestión de Productos (necesario para ventas e inventario)
5. **EP2**: Gestión de Inventario (necesario para ventas)
6. **EP3**: Registro de Ventas (funcionalidad principal)
7. **EP4**: Reconocimiento de Imágenes con IA (mejora de ventas)
8. **EP5**: Gestión de Devoluciones (complemento)
9. **EP9**: Consultas y Reportes (análisis)

---

## Resumen de Cambios en Épicas

### Épica 3 - Registro de Ventas:
- **Antes:** 4 User Stories
- **Después:** 2 User Stories (consolidadas)
- **Total:** 2 User Stories

### Épica 4 - Reconocimiento de Imágenes:
- **Antes:** 4 User Stories
- **Después:** 1 User Story (consolidada)
- **Total:** 1 User Story

### Total General:
- **Antes:** 40 User Stories
- **Después:** 35 User Stories (5 consolidadas)

---

## Próximos Pasos

Con estas aclaraciones, procederemos a generar las **35 User Stories** siguiendo el formato definido en `Procedimiento-UserStories.md`, respetando:
- Formato estándar "Como... quiero... para..."
- Descripción detallada
- Criterios de aceptación (Given/When/Then)
- Tareas de alto nivel
- Notas técnicas cuando corresponda

