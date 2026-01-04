import { Navigate, useLocation } from 'react-router';
import { ROUTES } from '@/routing/routes';

interface ProtectedRouteProps {
  children: React.ReactNode;
}

/**
 * Protected Route Component - Placeholder
 *
 * Will be fully implemented with add-authentication-user-management change.
 * Currently allows all access for development purposes.
 *
 * When implemented, this component will:
 * - Check if user is authenticated
 * - Redirect to login if not authenticated
 * - Optionally check for required roles
 */
export function ProtectedRoute({ children }: ProtectedRouteProps) {
  const location = useLocation();

  // TODO: Implement actual authentication check when auth is available
  // const { isAuthenticated, isLoading } = useAuth();
  //
  // if (isLoading) {
  //   return <LoadingSpinner />;
  // }
  //
  // if (!isAuthenticated) {
  //   return <Navigate to={ROUTES.AUTH.LOGIN} state={{ from: location }} replace />;
  // }

  // For now, allow all access
  const isAuthenticated = true;

  if (!isAuthenticated) {
    return <Navigate to={ROUTES.AUTH.LOGIN} state={{ from: location }} replace />;
  }

  return <>{children}</>;
}

export default ProtectedRoute;
