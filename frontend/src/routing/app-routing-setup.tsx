import { lazy, Suspense } from 'react';
import { Route, Routes, Navigate } from 'react-router';
import { Layout8 } from '@/components/layouts/layout-8';
import { ProtectedRoute, AdminRoute, PublicRoute } from '@/components/auth';
import { ROUTES } from './routes';

// Lazy load pages for optimal bundle size
const LoginPage = lazy(() => import('@/pages/auth/login'));
const DashboardPage = lazy(() => import('@/pages/dashboard/page'));
const ProductsPage = lazy(() => import('@/pages/products'));
const ProductCatalogPage = lazy(() => import('@/pages/products/catalog'));
const ProductCreatePage = lazy(() => import('@/pages/products/create'));
const ProductEditPage = lazy(() => import('@/pages/products/edit'));
const ProductImportPage = lazy(() => import('@/pages/products/import'));
const InventoryPage = lazy(() => import('@/pages/inventory'));
const InventoryAssignPage = lazy(() => import('@/pages/inventory/assign'));
const InventoryImportPage = lazy(() => import('@/pages/inventory/import'));
const InventoryAdjustPage = lazy(() => import('@/pages/inventory/adjust'));
const InventoryMovementsPage = lazy(() => import('@/pages/inventory/movements'));
const InventoryCentralizedPage = lazy(() => import('@/pages/inventory/centralized'));
const SalesPage = lazy(() => import('@/pages/sales'));
const ManualSalesPage = lazy(() => import('@/pages/sales/new'));
const ImageRecognitionSalesPage = lazy(() => import('@/pages/sales/new-image'));
const SalesHistoryPage = lazy(() => import('@/pages/sales/history'));
const ReturnsPage = lazy(() => import('@/pages/returns'));
const NewReturnPage = lazy(() => import('@/pages/returns/new'));
const ReturnsHistoryPage = lazy(() => import('@/pages/returns/history'));
const AIModelPage = lazy(() => import('@/pages/admin/ai-model'));
const PaymentMethodsPage = lazy(() => import('@/pages/payment-methods'));
const UsersPage = lazy(() => import('@/pages/users'));
const PointsOfSalePage = lazy(() => import('@/pages/points-of-sale'));
const ReportsPage = lazy(() => import('@/pages/reports'));

/**
 * Loading fallback component for lazy-loaded pages
 */
function PageLoader() {
  return (
    <div className="flex h-full min-h-[400px] items-center justify-center">
      <div className="text-muted-foreground">Cargando...</div>
    </div>
  );
}

/**
 * Main Application Routing Setup
 * Configures all routes with Layout 8 as the primary layout
 * Uses ProtectedRoute for authentication and AdminRoute for admin-only pages
 */
export function AppRoutingSetup() {
  return (
    <Suspense fallback={<PageLoader />}>
      <Routes>
        {/* Public Routes - Redirect to dashboard if already authenticated */}
        <Route element={<PublicRoute />}>
          <Route path={ROUTES.AUTH.LOGIN} element={<LoginPage />} />
        </Route>

        {/* Protected Routes - Require authentication */}
        <Route element={<ProtectedRoute />}>
          <Route element={<Layout8 />}>
            {/* Routes accessible by all authenticated users */}
            <Route path={ROUTES.DASHBOARD} element={<DashboardPage />} />
            <Route path={ROUTES.PRODUCTS.CATALOG} element={<ProductCatalogPage />} />
            <Route path={ROUTES.INVENTORY.ROOT} element={<InventoryPage />} />
            <Route path={ROUTES.INVENTORY.STOCK} element={<InventoryPage />} />
            <Route path={ROUTES.INVENTORY.MOVEMENTS} element={<InventoryMovementsPage />} />
            <Route path={ROUTES.SALES.ROOT} element={<SalesPage />} />
            <Route path={ROUTES.SALES.NEW} element={<ManualSalesPage />} />
            <Route path={ROUTES.SALES.NEW_IMAGE} element={<ImageRecognitionSalesPage />} />
            <Route path={ROUTES.SALES.HISTORY} element={<SalesHistoryPage />} />
            <Route path={ROUTES.RETURNS.ROOT} element={<ReturnsPage />} />
            <Route path={ROUTES.RETURNS.NEW} element={<NewReturnPage />} />
            <Route path={ROUTES.RETURNS.HISTORY} element={<ReturnsHistoryPage />} />
          </Route>
        </Route>

        {/* Admin-only Routes - Require Administrator role */}
        <Route element={<AdminRoute />}>
          <Route element={<Layout8 />}>
            {/* Inventory admin routes */}
            <Route path={ROUTES.INVENTORY.ASSIGN} element={<InventoryAssignPage />} />
            <Route path={ROUTES.INVENTORY.IMPORT} element={<InventoryImportPage />} />
            <Route path={ROUTES.INVENTORY.ADJUST} element={<InventoryAdjustPage />} />
            <Route path={ROUTES.INVENTORY.CENTRALIZED} element={<InventoryCentralizedPage />} />
            
            <Route path={ROUTES.PRODUCTS.ROOT} element={<ProductsPage />} />
            <Route path={ROUTES.PRODUCTS.CREATE} element={<ProductCreatePage />} />
            <Route path="/products/:productId/edit" element={<ProductEditPage />} />
            <Route path={ROUTES.PRODUCTS.IMPORT} element={<ProductImportPage />} />
            <Route path={ROUTES.PAYMENT_METHODS} element={<PaymentMethodsPage />} />
            <Route path={ROUTES.USERS} element={<UsersPage />} />
            <Route path={ROUTES.POINTS_OF_SALE} element={<PointsOfSalePage />} />
            <Route path={ROUTES.REPORTS} element={<ReportsPage />} />
            <Route path={ROUTES.AI_MODEL} element={<AIModelPage />} />
          </Route>
        </Route>

        {/* Default redirect to dashboard */}
        <Route path="/" element={<Navigate to={ROUTES.DASHBOARD} replace />} />

        {/* Catch-all redirect to dashboard */}
        <Route path="*" element={<Navigate to={ROUTES.DASHBOARD} replace />} />
      </Routes>
    </Suspense>
  );
}
