/**
 * Authentication Service
 * Handles login, logout, token refresh, and user info retrieval.
 * Supports both cookie-based auth (same-origin) and token-based auth (cross-origin).
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

// Token storage keys
const ACCESS_TOKEN_KEY = 'jpv_access_token';
const REFRESH_TOKEN_KEY = 'jpv_refresh_token';

/**
 * Token management utilities
 */
export const tokenStorage = {
  getAccessToken: (): string | null => localStorage.getItem(ACCESS_TOKEN_KEY),
  getRefreshToken: (): string | null => localStorage.getItem(REFRESH_TOKEN_KEY),
  setTokens: (accessToken: string, refreshToken: string) => {
    localStorage.setItem(ACCESS_TOKEN_KEY, accessToken);
    localStorage.setItem(REFRESH_TOKEN_KEY, refreshToken);
  },
  clearTokens: () => {
    localStorage.removeItem(ACCESS_TOKEN_KEY);
    localStorage.removeItem(REFRESH_TOKEN_KEY);
  },
};

/**
 * Auth Service - Authentication operations
 */
export const authService = {
  /**
   * Login with username and password
   * Stores tokens in localStorage for cross-origin scenarios
   */
  login: async (credentials: LoginRequest): Promise<LoginResponse> => {
    const response = await apiClient.post<LoginResponse>(
      AUTH_ENDPOINTS.LOGIN,
      credentials,
      { withCredentials: true }
    );
    
    // Store tokens in localStorage for cross-origin auth
    if (response.data.accessToken && response.data.refreshToken) {
      tokenStorage.setTokens(response.data.accessToken, response.data.refreshToken);
    }
    
    return response.data;
  },

  /**
   * Logout and clear session
   * Clears both cookies and localStorage tokens
   */
  logout: async (): Promise<void> => {
    try {
      await apiClient.post(AUTH_ENDPOINTS.LOGOUT, null, {
        withCredentials: true,
      });
    } finally {
      tokenStorage.clearTokens();
    }
  },

  /**
   * Refresh access token
   * Returns true if refresh was successful
   */
  refresh: async (): Promise<boolean> => {
    try {
      const refreshToken = tokenStorage.getRefreshToken();
      const response = await apiClient.post<LoginResponse>(
        AUTH_ENDPOINTS.REFRESH, 
        { refreshToken },
        { withCredentials: true }
      );
      
      // Update stored tokens if new ones are returned
      if (response.data.accessToken && response.data.refreshToken) {
        tokenStorage.setTokens(response.data.accessToken, response.data.refreshToken);
      }
      
      return true;
    } catch {
      tokenStorage.clearTokens();
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
