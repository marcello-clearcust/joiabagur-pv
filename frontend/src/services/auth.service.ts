/**
 * Authentication Service
 * Handles login, logout, token refresh, and user info retrieval.
 * Uses HTTP-only cookies for token storage (set by backend).
 */

import apiClient from './api.service';
import {
  AuthUser,
  LoginRequest,
  LoginResponse,
} from '@/types/auth.types';

const AUTH_ENDPOINTS = {
  LOGIN: '/auth/login',
  LOGOUT: '/auth/logout',
  REFRESH: '/auth/refresh',
  ME: '/auth/me',
} as const;

/**
 * Auth Service - Authentication operations
 */
export const authService = {
  /**
   * Login with username and password
   * Tokens are stored in HTTP-only cookies by the backend
   */
  login: async (credentials: LoginRequest): Promise<LoginResponse> => {
    const response = await apiClient.post<LoginResponse>(
      AUTH_ENDPOINTS.LOGIN,
      credentials,
      { withCredentials: true }
    );
    return response.data;
  },

  /**
   * Logout and clear session
   * Clears HTTP-only cookies on the backend
   */
  logout: async (): Promise<void> => {
    await apiClient.post(AUTH_ENDPOINTS.LOGOUT, null, {
      withCredentials: true,
    });
  },

  /**
   * Refresh access token using refresh token cookie
   * Returns true if refresh was successful
   */
  refresh: async (): Promise<boolean> => {
    try {
      await apiClient.post(AUTH_ENDPOINTS.REFRESH, null, {
        withCredentials: true,
      });
      return true;
    } catch {
      return false;
    }
  },

  /**
   * Get current authenticated user information
   * Returns null if not authenticated
   */
  getCurrentUser: async (): Promise<AuthUser | null> => {
    try {
      const response = await apiClient.get<AuthUser>(AUTH_ENDPOINTS.ME, {
        withCredentials: true,
      });
      return response.data;
    } catch {
      return null;
    }
  },
};

export default authService;
