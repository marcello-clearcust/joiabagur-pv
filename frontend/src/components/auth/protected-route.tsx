/**
 * Protected Route Component
 * Provides route guards for authenticated and role-based access.
 */

import { Navigate, Outlet, useLocation } from 'react-router-dom';
import { useAuth } from '@/providers';
import { ROUTES } from '@/routing/routes';
import { UserRole } from '@/types/common.types';

interface ProtectedRouteProps {
  /**
   * Required roles to access this route.
   * If not specified, any authenticated user can access.
   */
  allowedRoles?: UserRole[];
  /**
   * Redirect path when access is denied
   */
  redirectTo?: string;
}

/**
 * Loading spinner for auth check
 */
function AuthLoadingSpinner() {
  return (
    <div className="flex h-screen items-center justify-center">
      <div className="text-muted-foreground">Verificando autenticación...</div>
    </div>
  );
}

/**
 * Access denied component
 */
function AccessDenied() {
  return (
    <div className="flex h-screen flex-col items-center justify-center gap-4">
      <h1 className="text-2xl font-bold text-destructive">Acceso Denegado</h1>
      <p className="text-muted-foreground">
        No tienes permisos para acceder a esta página.
      </p>
    </div>
  );
}

/**
 * Protected Route - Requires authentication
 * Redirects to login if not authenticated
 */
export function ProtectedRoute({
  allowedRoles,
  redirectTo = ROUTES.AUTH.LOGIN,
}: ProtectedRouteProps) {
  const { isAuthenticated, isLoading, user } = useAuth();
  const location = useLocation();

  // Show loading while checking auth status
  if (isLoading) {
    return <AuthLoadingSpinner />;
  }

  // Redirect to login if not authenticated
  if (!isAuthenticated) {
    return <Navigate to={redirectTo} state={{ from: location }} replace />;
  }

  // Check role-based access if roles are specified
  if (allowedRoles && user) {
    const hasRequiredRole = allowedRoles.includes(user.role);
    if (!hasRequiredRole) {
      return <AccessDenied />;
    }
  }

  return <Outlet />;
}

/**
 * Admin Route - Requires Administrator role
 */
export function AdminRoute() {
  return <ProtectedRoute allowedRoles={['Administrator']} />;
}

/**
 * Public Route - Redirects to dashboard if already authenticated
 * Used for login page
 */
export function PublicRoute() {
  const { isAuthenticated, isLoading } = useAuth();
  const location = useLocation();

  if (isLoading) {
    return <AuthLoadingSpinner />;
  }

  // Redirect to dashboard if already authenticated
  if (isAuthenticated) {
    // Check if there's a "from" location to redirect back to
    const from = (location.state as { from?: Location })?.from?.pathname || ROUTES.DASHBOARD;
    return <Navigate to={from} replace />;
  }

  return <Outlet />;
}
