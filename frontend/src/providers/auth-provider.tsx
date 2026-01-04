/**
 * Authentication Provider
 * Provides global authentication state and methods to the application.
 */

import {
  createContext,
  useContext,
  useState,
  useEffect,
  useCallback,
  ReactNode,
} from 'react';
import { useNavigate } from 'react-router-dom';
import {
  AuthUser,
  AuthContextValue,
  LoginRequest,
} from '@/types/auth.types';
import { authService } from '@/services/auth.service';
import { ROUTES } from '@/routing/routes';

const AuthContext = createContext<AuthContextValue | undefined>(undefined);

interface AuthProviderProps {
  children: ReactNode;
}

export function AuthProvider({ children }: AuthProviderProps) {
  const [user, setUser] = useState<AuthUser | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const navigate = useNavigate();

  const isAuthenticated = user !== null;

  /**
   * Initialize auth state on app load
   * Attempts to get current user from session
   */
  useEffect(() => {
    const initializeAuth = async () => {
      try {
        const currentUser = await authService.getCurrentUser();
        setUser(currentUser);
      } catch {
        setUser(null);
      } finally {
        setIsLoading(false);
      }
    };

    initializeAuth();
  }, []);

  /**
   * Login with credentials
   */
  const login = useCallback(
    async (credentials: LoginRequest) => {
      const response = await authService.login(credentials);
      
      // Fetch full user info after login
      const fullUser = await authService.getCurrentUser();
      setUser(fullUser);

      // Navigate to dashboard after successful login
      navigate(ROUTES.DASHBOARD, { replace: true });
    },
    [navigate]
  );

  /**
   * Logout and clear session
   */
  const logout = useCallback(async () => {
    try {
      await authService.logout();
    } finally {
      setUser(null);
      navigate(ROUTES.AUTH.LOGIN, { replace: true });
    }
  }, [navigate]);

  /**
   * Refresh authentication token
   * Returns true if refresh was successful
   */
  const refreshAuth = useCallback(async (): Promise<boolean> => {
    const success = await authService.refresh();
    if (!success) {
      setUser(null);
    }
    return success;
  }, []);

  const value: AuthContextValue = {
    user,
    isAuthenticated,
    isLoading,
    login,
    logout,
    refreshAuth,
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

/**
 * Hook to access auth context
 * Must be used within AuthProvider
 */
export function useAuth(): AuthContextValue {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
}

export { AuthContext };
