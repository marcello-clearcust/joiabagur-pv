-- Script para verificar que la limpieza de datos se realizó correctamente

-- Verificar tablas que deberían estar vacías
SELECT 'Products' as table_name, COUNT(*) as count FROM "Products"
UNION ALL
SELECT 'Inventories', COUNT(*) FROM "Inventories"
UNION ALL
SELECT 'Sales', COUNT(*) FROM "Sales"
UNION ALL
SELECT 'SalePhotos', COUNT(*) FROM "SalePhotos"
UNION ALL
SELECT 'ProductPhotos', COUNT(*) FROM "ProductPhotos"
UNION ALL
SELECT 'InventoryMovements', COUNT(*) FROM "InventoryMovements"
UNION ALL
SELECT 'Collections', COUNT(*) FROM "Collections"
UNION ALL
SELECT 'ModelMetadata', COUNT(*) FROM "ModelMetadata"
UNION ALL
SELECT 'ModelTrainingJobs', COUNT(*) FROM "ModelTrainingJobs"
ORDER BY table_name;

-- Verificar tablas que NO deberían estar vacías (datos preservados)
SELECT 'Users' as table_name, COUNT(*) as count FROM "Users"
UNION ALL
SELECT 'PointOfSales', COUNT(*) FROM "PointOfSales"
UNION ALL
SELECT 'PaymentMethods', COUNT(*) FROM "PaymentMethods"
UNION ALL
SELECT 'PointOfSalePaymentMethods', COUNT(*) FROM "PointOfSalePaymentMethods"
UNION ALL
SELECT 'UserPointOfSales', COUNT(*) FROM "UserPointOfSales"
ORDER BY table_name;