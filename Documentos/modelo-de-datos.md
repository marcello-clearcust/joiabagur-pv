# Modelo de Datos - Sistema de Gestión de Puntos de Venta para Joyería

## Visión General

Modelo de datos diseñado para soportar el MVP del sistema de gestión de puntos de venta, con consideraciones para facilitar la escalabilidad hacia la Fase 2. El modelo está optimizado para PostgreSQL 15+ y utiliza Entity Framework Core como ORM.

---

## Diagrama del Modelo de Datos

```mermaid
erDiagram
    User ||--o{ UserPointOfSale : "asignado a"
    User ||--o{ Sale : "realiza"
    User ||--o{ InventoryMovement : "registra"
    User ||--o{ Return : "registra"
    
    PointOfSale ||--o{ UserPointOfSale : "tiene asignados"
    PointOfSale ||--o{ Sale : "registra ventas"
    PointOfSale ||--o{ Inventory : "tiene stock"
    PointOfSale ||--o{ PointOfSalePaymentMethod : "tiene métodos"
    PointOfSale ||--o{ Return : "recibe devoluciones"
    
    Product ||--o{ ProductPhoto : "tiene fotos"
    Product ||--o{ Sale : "se vende"
    Product ||--o{ Inventory : "en stock"
    Product ||--o{ InventoryMovement : "movimiento"
    Product ||--o{ Return : "devuelto"
    Product }o--|| Collection : "pertenece a"
    
    Collection ||--o{ Product : "contiene"
    
    PaymentMethod ||--o{ PointOfSalePaymentMethod : "disponible en"
    PaymentMethod ||--o{ Sale : "usado en"
    
    Sale ||--o{ SalePhoto : "tiene foto"
    Sale ||--o{ Return : "puede tener devolución"
    Sale ||--o{ InventoryMovement : "genera movimiento"
    
    Inventory ||--o{ InventoryMovement : "tiene movimientos"
    
    User {
        uuid Id PK
        string Username UK "unique"
        string Email UK "unique, nullable"
        string PasswordHash
        string FirstName
        string LastName
        enum Role "Admin, Operator"
        bool IsActive
        datetime CreatedAt
        datetime UpdatedAt
        datetime? LastLoginAt
    }
    
    PointOfSale {
        uuid Id PK
        string Name
        string Code UK "unique"
        string? Address
        string? Phone
        string? Email
        bool IsActive
        datetime CreatedAt
        datetime UpdatedAt
    }
    
    UserPointOfSale {
        uuid Id PK
        uuid UserId FK
        uuid PointOfSaleId FK
        datetime AssignedAt
        datetime? UnassignedAt
        bool IsActive
        unique(UserId, PointOfSaleId)
    }
    
    Collection {
        uuid Id PK
        string Name
        string? Description
        datetime CreatedAt
        datetime UpdatedAt
    }
    
    Product {
        uuid Id PK
        string SKU UK "unique, indexed"
        string Name
        string? Description
        decimal Price
        uuid? CollectionId FK "nullable"
        bool IsActive
        datetime CreatedAt
        datetime UpdatedAt
    }
    
    ProductPhoto {
        uuid Id PK
        uuid ProductId FK
        string FilePath "S3/blob path"
        string FileName
        int FileSize "bytes"
        string MimeType
        int DisplayOrder "para ordenar múltiples fotos"
        bool IsPrimary "foto principal"
        datetime CreatedAt
        indexed(ProductId, DisplayOrder)
    }
    
    PaymentMethod {
        uuid Id PK
        string Code UK "unique"
        string Name
        string? Description
        bool IsActive
        datetime CreatedAt
        datetime UpdatedAt
    }
    
    PointOfSalePaymentMethod {
        uuid Id PK
        uuid PointOfSaleId FK
        uuid PaymentMethodId FK
        bool IsActive
        datetime CreatedAt
        datetime? DeactivatedAt
        unique(PointOfSaleId, PaymentMethodId)
    }
    
    Sale {
        uuid Id PK
        uuid ProductId FK
        uuid PointOfSaleId FK
        uuid UserId FK "operador que realizó la venta"
        uuid PaymentMethodId FK
        decimal Price "precio al momento de la venta"
        int Quantity "default 1"
        string? Notes "notas adicionales"
        datetime SaleDate
        datetime CreatedAt
        indexed(PointOfSaleId, SaleDate)
        indexed(ProductId, SaleDate)
        indexed(UserId, SaleDate)
        indexed(PaymentMethodId, SaleDate)
    }
    
    SalePhoto {
        uuid Id PK
        uuid SaleId FK
        string FilePath "S3/blob path"
        string FileName
        int FileSize "bytes"
        string MimeType
        datetime CreatedAt
    }
    
    Return {
        uuid Id PK
        uuid SaleId FK "venta original"
        uuid ProductId FK
        uuid PointOfSaleId FK
        uuid UserId FK "usuario que registra la devolución"
        string? Reason
        datetime ReturnDate
        datetime CreatedAt
        indexed(SaleId)
        indexed(PointOfSaleId, ReturnDate)
    }
    
    Inventory {
        uuid Id PK
        uuid ProductId FK
        uuid PointOfSaleId FK
        int Quantity "stock actual"
        int? MinimumThreshold "para alertas futuras (Fase 2)"
        datetime LastUpdatedAt
        datetime CreatedAt
        unique(ProductId, PointOfSaleId)
        indexed(PointOfSaleId, Quantity)
        indexed(ProductId)
    }
    
    InventoryMovement {
        uuid Id PK
        uuid InventoryId FK
        uuid? SaleId FK "nullable, si es movimiento por venta"
        uuid? ReturnId FK "nullable, si es movimiento por devolución"
        uuid UserId FK "usuario que registra el movimiento"
        enum MovementType "Sale, Return, Adjustment, Import"
        int QuantityChange "positivo o negativo"
        int QuantityBefore "stock antes del movimiento"
        int QuantityAfter "stock después del movimiento"
        string? Reason "motivo del ajuste"
        datetime MovementDate
        datetime CreatedAt
        indexed(InventoryId, MovementDate)
        indexed(SaleId)
        indexed(ReturnId)
        indexed(UserId, MovementDate)
    }
```

---

## Descripción de Entidades Principales

### User (Usuarios)

Representa a los usuarios del sistema con dos roles principales: **Administrador** y **Operador**.

**Campos Clave:**
- `Id`: Identificador único (UUID)
- `Username`: Nombre de usuario único para login
- `Email`: Email único (opcional, para notificaciones futuras)
- `PasswordHash`: Hash de la contraseña (BCrypt)
- `Role`: Enum (Admin, Operator) - define permisos del usuario
- `IsActive`: Flag para habilitar/deshabilitar usuarios sin eliminarlos

**Consideraciones Fase 2:**
- Campo `Email` preparado para notificaciones de alertas de stock bajo
- `LastLoginAt` para auditoría y seguridad

---

### PointOfSale (Puntos de Venta)

Representa los diferentes puntos de venta donde se realizan las transacciones (tiendas propias, hoteles, terceros).

**Campos Clave:**
- `Id`: Identificador único (UUID)
- `Name`: Nombre del punto de venta
- `Code`: Código único para identificación rápida
- `IsActive`: Flag para habilitar/deshabilitar puntos de venta

**Consideraciones Fase 2:**
- Campos `Address`, `Phone`, `Email` preparados para reportes y contactos

---

### UserPointOfSale (Asignación Usuario-Punto de Venta)

Tabla de relación muchos a muchos entre usuarios y puntos de venta. Permite que los operadores estén asignados a múltiples puntos de venta.

**Campos Clave:**
- `UserId`: Referencia al usuario (operador)
- `PointOfSaleId`: Referencia al punto de venta
- `IsActive`: Controla si la asignación está activa
- `AssignedAt` / `UnassignedAt`: Historial de asignaciones

**Nota:** Los administradores no necesitan estar en esta tabla ya que tienen acceso a todos los puntos de venta por defecto.

---

### Collection (Colecciones)

Agrupa productos por colección. Aunque está en el MVP, es opcional inicialmente.

**Campos Clave:**
- `Id`: Identificador único (UUID)
- `Name`: Nombre de la colección
- `Description`: Descripción opcional

**Consideraciones Fase 2:**
- Preparado para reportes por colección y filtros avanzados

---

### Product (Productos)

Catálogo centralizado de productos de la joyería.

**Campos Clave:**
- `Id`: Identificador único (UUID)
- `SKU`: Código único del producto (índice único, usado para matching en importaciones)
- `Name`: Nombre del producto
- `Description`: Descripción detallada (opcional)
- `Price`: Precio actual del producto
- `CollectionId`: Referencia opcional a la colección
- `IsActive`: Flag para productos activos/inactivos

**Consideraciones Fase 2:**
- Campo `Price` puede evolucionar a tabla `ProductPriceHistory` para historial de precios
- Preparado para precios diferentes por punto de venta (nueva tabla `ProductPointOfSalePrice`)

---

### ProductPhoto (Fotos de Productos)

Fotos de referencia de productos para el reconocimiento de imágenes. Múltiples fotos por producto para mejorar la precisión del modelo de IA.

**Campos Clave:**
- `ProductId`: Referencia al producto
- `FilePath`: Ruta en S3/Blob Storage
- `FileName`: Nombre original del archivo
- `FileSize`: Tamaño en bytes
- `MimeType`: Tipo MIME de la imagen
- `DisplayOrder`: Orden de visualización
- `IsPrimary`: Indica si es la foto principal

**Optimizaciones:**
- Índice compuesto en `(ProductId, DisplayOrder)` para ordenamiento eficiente
- Las fotos se almacenan en object storage (S3/Blob), no en la base de datos

---

### PaymentMethod (Métodos de Pago)

Lista general de métodos de pago disponibles en el sistema.

**Campos Clave:**
- `Id`: Identificador único (UUID)
- `Code`: Código único (ej: "CASH", "BIZUM", "CARD_OWN", "CARD_POS", "PAYPAL", "TRANSFER")
- `Name`: Nombre descriptivo
- `Description`: Descripción opcional
- `IsActive`: Flag para habilitar/deshabilitar métodos

**Valores Predefinidos (MVP):**
- Efectivo (CASH)
- Bizum (BIZUM)
- Transferencia bancaria (TRANSFER)
- Tarjeta TPV propio (CARD_OWN)
- Tarjeta TPV punto de venta (CARD_POS)
- PayPal (PAYPAL)

---

### PointOfSalePaymentMethod (Métodos de Pago por Punto de Venta)

Tabla de relación muchos a muchos que define qué métodos de pago están disponibles en cada punto de venta.

**Campos Clave:**
- `PointOfSaleId`: Referencia al punto de venta
- `PaymentMethodId`: Referencia al método de pago
- `IsActive`: Controla si el método está activo para ese punto de venta
- `DeactivatedAt`: Timestamp de desactivación (auditoría)

**Restricción:** Constraint único en `(PointOfSaleId, PaymentMethodId)` para evitar duplicados.

---

### Sale (Ventas)

Registro de todas las ventas realizadas en el sistema.

**Campos Clave:**
- `Id`: Identificador único (UUID)
- `ProductId`: Producto vendido
- `PointOfSaleId`: Punto de venta donde se realizó la venta
- `UserId`: Operador que realizó la venta
- `PaymentMethodId`: Método de pago utilizado
- `Price`: Precio al momento de la venta (snapshot, no referencia al precio actual)
- `Quantity`: Cantidad vendida (default 1)
- `SaleDate`: Fecha y hora de la venta
- `Notes`: Notas adicionales opcionales

**Consideraciones:**
- `Price` es un snapshot para mantener integridad histórica
- Múltiples índices compuestos para consultas frecuentes por punto de venta, producto, usuario y método de pago

**Consideraciones Fase 2:**
- Campo `Notes` puede evolucionar para incluir información de promociones/descuentos
- Preparado para tabla `SaleDiscount` si se implementan descuentos

---

### SalePhoto (Fotos de Ventas)

Fotos asociadas a ventas cuando se registran mediante reconocimiento de imágenes.

**Campos Clave:**
- `SaleId`: Referencia a la venta
- `FilePath`: Ruta en S3/Blob Storage
- `FileName`: Nombre original del archivo
- `FileSize`: Tamaño en bytes
- `MimeType`: Tipo MIME de la imagen

**Nota:** Una venta puede tener 0 o 1 foto (cuando se registra con reconocimiento de imagen). Si se registra manualmente, no tendrá foto.

---

### Return (Devoluciones)

Registro de devoluciones de productos vendidos.

**Campos Clave:**
- `Id`: Identificador único (UUID)
- `SaleId`: Referencia a la venta original
- `ProductId`: Producto devuelto (redundante pero útil para consultas)
- `PointOfSaleId`: Punto de venta donde se recibe la devolución
- `UserId`: Usuario que registra la devolución
- `Reason`: Motivo de la devolución (opcional)
- `ReturnDate`: Fecha y hora de la devolución

**Consideraciones:**
- Una venta puede tener múltiples devoluciones (parciales) en el futuro
- El stock se incrementa automáticamente mediante `InventoryMovement`

---

### Inventory (Inventario)

Stock actual de cada producto en cada punto de venta.

**Campos Clave:**
- `Id`: Identificador único (UUID)
- `ProductId`: Referencia al producto
- `PointOfSaleId`: Referencia al punto de venta
- `Quantity`: Cantidad actual en stock
- `MinimumThreshold`: Umbral mínimo para alertas (preparado para Fase 2)
- `LastUpdatedAt`: Última actualización del stock

**Restricción:** Constraint único en `(ProductId, PointOfSaleId)` para garantizar un solo registro por combinación producto-punto de venta.

**Optimizaciones:**
- Índice compuesto en `(PointOfSaleId, Quantity)` para consultas de stock bajo
- Índice en `ProductId` para consultas de stock total por producto

---

### InventoryMovement (Movimientos de Inventario)

Historial completo y trazable de todos los movimientos de inventario (ventas, devoluciones, ajustes manuales, importaciones).

**Campos Clave:**
- `Id`: Identificador único (UUID)
- `InventoryId`: Referencia al inventario afectado
- `SaleId`: Referencia opcional a la venta (si el movimiento es por venta)
- `ReturnId`: Referencia opcional a la devolución (si el movimiento es por devolución)
- `UserId`: Usuario que registra el movimiento
- `MovementType`: Tipo de movimiento (Sale, Return, Adjustment, Import)
- `QuantityChange`: Cambio en la cantidad (positivo o negativo)
- `QuantityBefore`: Stock antes del movimiento
- `QuantityAfter`: Stock después del movimiento
- `Reason`: Motivo del movimiento (especialmente para ajustes manuales)
- `MovementDate`: Fecha y hora del movimiento

**Consideraciones:**
- Tabla de auditoría completa para trazabilidad total
- Permite reconstruir el estado del inventario en cualquier momento
- Los campos `QuantityBefore` y `QuantityAfter` permiten validar integridad

**Optimizaciones:**
- Múltiples índices para consultas por inventario, venta, devolución y usuario
- Índice compuesto en `(InventoryId, MovementDate)` para historial ordenado

---

## Relaciones y Cardinalidades

### Relaciones Principales

| Entidad Origen | Relación | Entidad Destino | Cardinalidad | Descripción |
|----------------|----------|-----------------|--------------|-------------|
| **User** | asignado a | **PointOfSale** | N:M | Operadores asignados a puntos de venta (tabla intermedia: `UserPointOfSale`) |
| **User** | realiza | **Sale** | 1:N | Un usuario puede realizar múltiples ventas |
| **User** | registra | **InventoryMovement** | 1:N | Un usuario registra múltiples movimientos |
| **User** | registra | **Return** | 1:N | Un usuario registra múltiples devoluciones |
| **PointOfSale** | tiene métodos | **PaymentMethod** | N:M | Puntos de venta tienen múltiples métodos de pago (tabla intermedia: `PointOfSalePaymentMethod`) |
| **PointOfSale** | registra ventas | **Sale** | 1:N | Un punto de venta registra múltiples ventas |
| **PointOfSale** | tiene stock | **Inventory** | 1:N | Un punto de venta tiene múltiples productos en stock |
| **Product** | tiene fotos | **ProductPhoto** | 1:N | Un producto tiene múltiples fotos de referencia |
| **Product** | pertenece a | **Collection** | N:1 | Un producto pertenece a una colección (opcional, nullable) |
| **Product** | se vende | **Sale** | 1:N | Un producto puede venderse múltiples veces |
| **Product** | en stock | **Inventory** | 1:N | Un producto puede estar en stock en múltiples puntos de venta |
| **Sale** | tiene foto | **SalePhoto** | 1:0..1 | Una venta puede tener una foto (opcional) |
| **Sale** | puede tener devolución | **Return** | 1:0..N | Una venta puede tener múltiples devoluciones |
| **Sale** | genera movimiento | **InventoryMovement** | 1:1 | Cada venta genera un movimiento de inventario |
| **Return** | genera movimiento | **InventoryMovement** | 1:1 | Cada devolución genera un movimiento de inventario |
| **Inventory** | tiene movimientos | **InventoryMovement** | 1:N | Un inventario tiene múltiples movimientos históricos |

### Reglas de Negocio Implícitas

1. **Usuarios Administradores:**
   - No requieren asignación en `UserPointOfSale`
   - Tienen acceso a todos los puntos de venta por lógica de aplicación

2. **Usuarios Operadores:**
   - Deben estar asignados al menos a un punto de venta en `UserPointOfSale`
   - Solo pueden registrar ventas en puntos de venta asignados

3. **Métodos de Pago:**
   - Un punto de venta debe tener al menos un método de pago asignado
   - Solo se pueden usar métodos de pago activos y asignados al punto de venta

4. **Fotos de Productos:**
   - Un producto debe tener al menos una foto para usar reconocimiento de imágenes
   - Solo una foto puede ser marcada como `IsPrimary = true` por producto

5. **Stock:**
   - El stock no puede ser negativo (validación a nivel de aplicación)
   - Cada movimiento actualiza `Inventory.Quantity` y crea registro en `InventoryMovement`

6. **Ventas:**
   - El precio en `Sale` es un snapshot (no referencia al precio actual del producto)
   - Una venta debe tener un método de pago válido para el punto de venta

7. **Devoluciones:**
   - Una devolución debe referenciar una venta existente
   - El producto devuelto debe coincidir con el producto de la venta original

---

## Índices y Optimizaciones

### Índices Primarios

Todas las entidades utilizan `Id` (UUID) como clave primaria con índice automático.

### Índices Únicos

| Tabla | Campo(s) | Propósito |
|-------|----------|-----------|
| `User` | `Username` | Login único |
| `User` | `Email` | Email único (si se proporciona) |
| `PointOfSale` | `Code` | Código único de identificación |
| `Product` | `SKU` | Código único para matching en importaciones |
| `PaymentMethod` | `Code` | Código único del método de pago |
| `Inventory` | `(ProductId, PointOfSaleId)` | Un solo registro por combinación |
| `UserPointOfSale` | `(UserId, PointOfSaleId)` | Evitar asignaciones duplicadas |
| `PointOfSalePaymentMethod` | `(PointOfSaleId, PaymentMethodId)` | Evitar métodos duplicados |

### Índices Compuestos para Consultas Frecuentes

#### Tabla: Sale

| Índice | Campos | Propósito | Casos de Uso |
|--------|--------|-----------|--------------|
| `IX_Sale_PointOfSale_SaleDate` | `(PointOfSaleId, SaleDate DESC)` | Consultas de ventas por punto de venta y fecha | Historial de ventas por punto de venta |
| `IX_Sale_Product_SaleDate` | `(ProductId, SaleDate DESC)` | Consultas de ventas por producto | Productos más vendidos, historial por producto |
| `IX_Sale_User_SaleDate` | `(UserId, SaleDate DESC)` | Consultas de ventas por operador | Rendimiento por operador |
| `IX_Sale_PaymentMethod_SaleDate` | `(PaymentMethodId, SaleDate DESC)` | Consultas de ventas por método de pago | Reportes por método de pago |

**Justificación:** Las consultas de historial de ventas (caso de uso #10) requieren filtrado por múltiples criterios y ordenamiento por fecha. Estos índices optimizan las consultas más comunes.

#### Tabla: InventoryMovement

| Índice | Campos | Propósito | Casos de Uso |
|--------|--------|-----------|--------------|
| `IX_InventoryMovement_Inventory_MovementDate` | `(InventoryId, MovementDate DESC)` | Historial de movimientos por inventario | Trazabilidad de stock |
| `IX_InventoryMovement_Sale` | `(SaleId)` | Búsqueda rápida de movimientos por venta | Validación y auditoría |
| `IX_InventoryMovement_Return` | `(ReturnId)` | Búsqueda rápida de movimientos por devolución | Validación y auditoría |
| `IX_InventoryMovement_User_MovementDate` | `(UserId, MovementDate DESC)` | Movimientos por usuario | Auditoría de operaciones |

**Justificación:** La tabla `InventoryMovement` crecerá rápidamente y necesita índices para consultas de historial y auditoría.

#### Tabla: Inventory

| Índice | Campos | Propósito | Casos de Uso |
|--------|--------|-----------|--------------|
| `IX_Inventory_PointOfSale_Quantity` | `(PointOfSaleId, Quantity)` | Consultas de stock bajo por punto de venta | Alertas de stock bajo (Fase 2) |
| `IX_Inventory_Product` | `(ProductId)` | Consultas de stock total por producto | Vista centralizada de stock |

**Justificación:** Optimiza las consultas de inventario por punto de venta y producto, esenciales para el caso de uso #11.

#### Tabla: ProductPhoto

| Índice | Campos | Propósito | Casos de Uso |
|--------|--------|-----------|--------------|
| `IX_ProductPhoto_Product_DisplayOrder` | `(ProductId, DisplayOrder)` | Ordenamiento de fotos por producto | Visualización de catálogo |

**Justificación:** Permite cargar fotos ordenadas eficientemente para el reconocimiento de imágenes.

#### Tabla: Return

| Índice | Campos | Propósito | Casos de Uso |
|--------|--------|-----------|--------------|
| `IX_Return_Sale` | `(SaleId)` | Búsqueda de devoluciones por venta | Gestión de devoluciones |
| `IX_Return_PointOfSale_ReturnDate` | `(PointOfSaleId, ReturnDate DESC)` | Historial de devoluciones por punto de venta | Reportes de devoluciones |

**Justificación:** Optimiza las consultas de devoluciones asociadas a ventas y por punto de venta.

### Índices Adicionales para Búsquedas

| Tabla | Campo | Tipo | Propósito |
|-------|-------|------|-----------|
| `Product` | `Name` | B-tree | Búsqueda por nombre de producto |
| `Product` | `SKU` | B-tree (único) | Búsqueda rápida por SKU (caso de uso #6) |
| `PointOfSale` | `Name` | B-tree | Búsqueda por nombre de punto de venta |
| `User` | `Username` | B-tree (único) | Login rápido |

### Optimizaciones Específicas para PostgreSQL

#### 1. Particionamiento (Fase 2 - Preparado)

Las tablas `Sale` e `InventoryMovement` pueden particionarse por rango de fechas cuando crezcan significativamente:

```sql
-- Ejemplo para Fase 2
CREATE TABLE Sale_2024 PARTITION OF Sale
FOR VALUES FROM ('2024-01-01') TO ('2025-01-01');
```

**Beneficio:** Mejora el rendimiento de consultas históricas y facilita el archivado.

#### 2. Índices Parciales

Para consultas frecuentes de datos activos:

```sql
-- Solo indexar productos activos
CREATE INDEX IX_Product_Active_SKU ON Product(SKU) 
WHERE IsActive = true;

-- Solo indexar ventas recientes (últimos 6 meses)
CREATE INDEX IX_Sale_Recent ON Sale(SaleDate DESC) 
WHERE SaleDate >= CURRENT_DATE - INTERVAL '6 months';
```

**Beneficio:** Reduce el tamaño de los índices y mejora el rendimiento de consultas comunes.

#### 3. Full-Text Search (Fase 2 - Preparado)

Para búsquedas avanzadas en descripciones de productos:

```sql
-- Preparado para Fase 2
ALTER TABLE Product ADD COLUMN SearchVector tsvector;
CREATE INDEX IX_Product_SearchVector ON Product USING GIN(SearchVector);
```

**Beneficio:** Búsquedas de texto completo más eficientes que `LIKE`.

#### 4. Connection Pooling

Configurar pool de conexiones en Entity Framework Core:

```csharp
// En Program.cs
services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.MaxBatchSize(50);
        npgsqlOptions.CommandTimeout(30);
    }));
```

**Recomendación:** Máximo 5-10 conexiones simultáneas para free-tier.

#### 5. VACUUM y Mantenimiento

Configurar VACUUM automático para mantener el rendimiento:

```sql
-- Configuración recomendada para free-tier
ALTER TABLE Sale SET (autovacuum_vacuum_scale_factor = 0.1);
ALTER TABLE InventoryMovement SET (autovacuum_vacuum_scale_factor = 0.1);
```

**Beneficio:** Mantiene las tablas optimizadas sin intervención manual.

### Estrategias de Optimización por Caso de Uso

#### Caso de Uso #1: Importar productos desde Excel
- **Índice crítico:** `Product.SKU` (único) para matching rápido
- **Optimización:** Usar `UPSERT` (INSERT ... ON CONFLICT) para actualizaciones eficientes

#### Caso de Uso #5 y #6: Registrar ventas
- **Índices críticos:** 
  - `Sale(PointOfSaleId, SaleDate)` para validación rápida
  - `Inventory(ProductId, PointOfSaleId)` para verificar stock
- **Optimización:** Transacciones atómicas para venta + movimiento de inventario

#### Caso de Uso #7: Gestionar devoluciones
- **Índice crítico:** `Return(SaleId)` para validar venta original
- **Optimización:** Transacciones atómicas para devolución + movimiento de inventario

#### Caso de Uso #10: Consultar historial de ventas
- **Índices críticos:** Todos los índices compuestos de `Sale`
- **Optimización:** Paginación obligatoria (máx 50 items/página)

#### Caso de Uso #11: Consultar inventario
- **Índices críticos:** `Inventory(PointOfSaleId, Quantity)` y `Inventory(ProductId)`
- **Optimización:** Cache en memoria para productos frecuentes (backend)

### Consideraciones de Escalabilidad (Fase 2)

1. **Read Replicas:** Para reportes pesados, usar réplicas de lectura
2. **Caching:** Redis para cache de productos y stock frecuente
3. **Archivado:** Mover datos antiguos (>2 años) a tablas de archivado
4. **Materialized Views:** Para agregaciones complejas de reportes
5. **Índices Adicionales:** Basados en patrones de consulta reales (monitoreo)

---

## Consideraciones de Implementación

### Campos de Auditoría

Todas las entidades principales incluyen:
- `CreatedAt`: Timestamp de creación (automático)
- `UpdatedAt`: Timestamp de última actualización (automático, trigger)

### Soft Delete

Las entidades críticas (`User`, `Product`, `PointOfSale`) utilizan `IsActive` en lugar de eliminación física para mantener integridad referencial histórica.

### UUID vs Integer

Se utiliza UUID como clave primaria para:
- **Ventajas:**
  - Evitar problemas de colisión en sistemas distribuidos
  - Mayor seguridad (no expone información sobre cantidad de registros)
  - Facilita sincronización offline (Fase 2)
- **Desventajas:**
  - Mayor tamaño (16 bytes vs 4-8 bytes)
  - Rendimiento ligeramente inferior en joins
- **Mitigación:** Los índices compensan el impacto en rendimiento

### Normalización

El modelo sigue 3NF (Tercera Forma Normal) con las siguientes excepciones intencionales:
- `Sale.Price`: Snapshot del precio (redundante pero necesario para integridad histórica)
- `Return.ProductId`: Redundante pero útil para consultas sin join

### Constraints y Validaciones

#### A Nivel de Base de Datos

```sql
-- Stock no negativo (validación en aplicación, constraint opcional)
ALTER TABLE Inventory ADD CONSTRAINT CHK_Quantity_NonNegative 
CHECK (Quantity >= 0);

-- Precio positivo
ALTER TABLE Product ADD CONSTRAINT CHK_Price_Positive 
CHECK (Price > 0);

-- Cantidad de venta positiva
ALTER TABLE Sale ADD CONSTRAINT CHK_Sale_Quantity_Positive 
CHECK (Quantity > 0);
```

#### A Nivel de Aplicación

- Validación de métodos de pago asignados al punto de venta
- Validación de stock disponible antes de venta
- Validación de usuario asignado al punto de venta (para operadores)
- Validación de SKU único en importaciones

---

## Migración y Evolución del Modelo

### Fase 1 (MVP) - Implementación Inicial

1. Crear todas las tablas base
2. Implementar índices principales
3. Seed de datos iniciales (métodos de pago predefinidos)
4. Migraciones de Entity Framework Core

### Fase 2 - Extensiones Preparadas

El modelo está preparado para agregar sin cambios estructurales mayores:

1. **Historial de Precios:**
   ```sql
   CREATE TABLE ProductPriceHistory (
       Id UUID PRIMARY KEY,
       ProductId UUID REFERENCES Product(Id),
       Price DECIMAL NOT NULL,
       EffectiveDate TIMESTAMP NOT NULL,
       CreatedAt TIMESTAMP DEFAULT NOW()
   );
   ```

2. **Alertas de Stock Bajo:**
   - Campo `MinimumThreshold` ya existe en `Inventory`
   - Nueva tabla `StockAlert` para registro de alertas

3. **Promociones y Descuentos:**
   ```sql
   CREATE TABLE SaleDiscount (
       Id UUID PRIMARY KEY,
       SaleId UUID REFERENCES Sale(Id),
       DiscountType VARCHAR(50), -- Percentage, Fixed
       DiscountValue DECIMAL NOT NULL,
       CreatedAt TIMESTAMP DEFAULT NOW()
   );
   ```

4. **Feedback de Reconocimiento de Imágenes:**
   ```sql
   CREATE TABLE ImageRecognitionFeedback (
       Id UUID PRIMARY KEY,
       SaleId UUID REFERENCES Sale(Id),
       ProductId UUID REFERENCES Product(Id),
       ConfidenceScore DECIMAL,
       WasCorrect BOOLEAN,
       CreatedAt TIMESTAMP DEFAULT NOW()
   );
   ```

---

## Resumen de Optimizaciones para Free-tier

### Reducción de Carga en Base de Datos

1. **Índices Selectivos:** Solo índices necesarios para consultas frecuentes
2. **Paginación Obligatoria:** Todas las listas paginadas (máx 50 items)
3. **Cache en Backend:** Productos y métodos de pago en memoria
4. **Lazy Loading:** Carga diferida de relaciones no críticas
5. **Connection Pooling:** Máximo 5-10 conexiones simultáneas

### Optimización de Almacenamiento

1. **Fotos en Object Storage:** No en base de datos (reduce tamaño de DB)
2. **UUID Eficiente:** Aunque más grande, evita problemas de escalabilidad
3. **Soft Delete:** Mantiene historial sin crecimiento excesivo de tablas

### Monitoreo Recomendado

1. **Tamaño de Tablas:** Monitorear crecimiento de `Sale` e `InventoryMovement`
2. **Uso de Índices:** Analizar índices no utilizados con `pg_stat_user_indexes`
3. **Tiempo de Consulta:** Monitorear queries lentas con `pg_stat_statements`
4. **Conexiones:** Alertar cuando se acerque al límite del pool

---

## Conclusión

Este modelo de datos está diseñado para:

✅ **Soportar todos los casos de uso del MVP** de manera eficiente  
✅ **Escalar hacia la Fase 2** sin cambios estructurales mayores  
✅ **Optimizar para free-tier** con índices selectivos y paginación  
✅ **Mantener integridad** mediante constraints y validaciones  
✅ **Facilitar auditoría** con campos de trazabilidad completos  
✅ **Preparar para crecimiento** con campos y estructuras extensibles  

El modelo balancea normalización con rendimiento, priorizando las consultas más frecuentes mientras mantiene la flexibilidad para evolucionar según las necesidades del negocio.

