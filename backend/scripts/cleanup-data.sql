-- Script para limpiar datos de Productos, Inventario, Ventas y Modelos de reconocimiento de imagen
-- Este script mantiene intactos: Usuarios, Puntos de venta, Métodos de pago

-- Iniciar transacción para asegurar atomicidad
BEGIN;

-- Paso 1: Eliminar movimientos de inventario (depende de Inventories, Sales)
DELETE FROM "InventoryMovements";

-- Paso 2: Eliminar fotos de ventas (depende de Sales)
DELETE FROM "SalePhotos";

-- Paso 3: Eliminar ventas (depende de Products, PointOfSales, Users, PaymentMethods)
DELETE FROM "Sales";

-- Paso 4: Eliminar inventario (depende de Products, PointOfSales)
DELETE FROM "Inventories";

-- Paso 5: Eliminar fotos de productos (depende de Products)
DELETE FROM "ProductPhotos";

-- Paso 6: Eliminar productos (depende de Collections)
DELETE FROM "Products";

-- Paso 7: Eliminar colecciones (no depende de nadie más)
DELETE FROM "Collections";

-- Paso 8: Eliminar trabajos de entrenamiento de modelos (depende de Users)
DELETE FROM "ModelTrainingJobs";

-- Paso 9: Eliminar metadatos de modelos (no depende de nadie más)
DELETE FROM "ModelMetadata";

-- Confirmar la transacción
COMMIT;

-- Mensaje de confirmación
SELECT 'Limpieza de datos completada exitosamente. Se han eliminado todos los datos de: Productos, Inventario, Ventas y Modelos de reconocimiento de imagen.' as mensaje;