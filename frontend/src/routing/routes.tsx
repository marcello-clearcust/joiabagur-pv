/**
 * Application Route Constants
 * Centralized route definitions for the application
 */

export const ROUTES = {
  // Public routes
  AUTH: {
    LOGIN: '/auth/login',
  },

  // Protected routes
  DASHBOARD: '/dashboard',

  // Module routes - organized by epic
  PRODUCTS: '/products',
  INVENTORY: '/inventory',
  SALES: '/sales',
  RETURNS: '/returns',
  PAYMENT_METHODS: '/payment-methods',
  USERS: '/users',
  POINTS_OF_SALE: '/points-of-sale',
  REPORTS: '/reports',
} as const;

/**
 * Check if a route is public (doesn't require authentication)
 */
export function isPublicRoute(path: string): boolean {
  const publicRoutes = [ROUTES.AUTH.LOGIN];
  return publicRoutes.some((route) => path.startsWith(route));
}
