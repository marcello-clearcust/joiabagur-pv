/**
 * Authentication type definitions
 */

import { UserRole } from './common.types';

/**
 * Point of Sale assignment for user
 */
export interface UserPointOfSale {
  pointOfSaleId: string;
  name: string;
  code: string;
  assignedAt: string;
  isActive: boolean;
}

/**
 * Authenticated user information returned from API
 */
export interface AuthUser {
  userId: string;
  username: string;
  firstName: string;
  lastName: string;
  email?: string;
  role: UserRole;
  assignedPointOfSales?: UserPointOfSale[];
}

/**
 * Login request payload
 */
export interface LoginRequest {
  username: string;
  password: string;
}

/**
 * Login response from API
 */
export interface LoginResponse {
  userId: string;
  username: string;
  firstName: string;
  lastName: string;
  role: UserRole;
  accessToken?: string;
  refreshToken?: string;
}

/**
 * Token refresh response
 */
export interface RefreshResponse {
  success: boolean;
}

/**
 * Auth state for context
 */
export interface AuthState {
  user: AuthUser | null;
  isAuthenticated: boolean;
  isLoading: boolean;
}

/**
 * Auth context value
 */
export interface AuthContextValue extends AuthState {
  login: (credentials: LoginRequest) => Promise<void>;
  logout: () => Promise<void>;
  refreshAuth: () => Promise<boolean>;
}
