/**
 * Unit tests for AuthService
 * Tests authentication operations: login, logout, refresh, getCurrentUser
 */

import { describe, it, expect, vi, beforeEach } from 'vitest';
import { authService } from './auth.service';
import apiClient from './api.service';

// Mock the API client
vi.mock('./api.service', () => ({
  default: {
    post: vi.fn(),
    get: vi.fn(),
  },
}));

describe('AuthService', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  describe('login', () => {
    it('should call login endpoint with credentials', async () => {
      const mockResponse = {
        data: {
          userId: '123',
          username: 'admin',
          firstName: 'Admin',
          lastName: 'User',
          role: 'Administrator',
        },
      };

      vi.mocked(apiClient.post).mockResolvedValueOnce(mockResponse);

      const credentials = { username: 'admin', password: 'password123' };
      const result = await authService.login(credentials);

      expect(apiClient.post).toHaveBeenCalledWith(
        '/auth/login',
        credentials,
        { withCredentials: true }
      );
      expect(result).toEqual(mockResponse.data);
    });

    it('should throw error on invalid credentials', async () => {
      const error = new Error('Unauthorized');
      vi.mocked(apiClient.post).mockRejectedValueOnce(error);

      const credentials = { username: 'admin', password: 'wrong' };

      await expect(authService.login(credentials)).rejects.toThrow('Unauthorized');
    });
  });

  describe('logout', () => {
    it('should call logout endpoint', async () => {
      vi.mocked(apiClient.post).mockResolvedValueOnce({ data: null });

      await authService.logout();

      expect(apiClient.post).toHaveBeenCalledWith(
        '/auth/logout',
        null,
        { withCredentials: true }
      );
    });
  });

  describe('refresh', () => {
    it('should return true on successful refresh', async () => {
      vi.mocked(apiClient.post).mockResolvedValueOnce({ data: { message: 'Token refreshed' } });

      const result = await authService.refresh();

      expect(apiClient.post).toHaveBeenCalledWith(
        '/auth/refresh',
        null,
        { withCredentials: true }
      );
      expect(result).toBe(true);
    });

    it('should return false on refresh failure', async () => {
      vi.mocked(apiClient.post).mockRejectedValueOnce(new Error('Token expired'));

      const result = await authService.refresh();

      expect(result).toBe(false);
    });
  });

  describe('getCurrentUser', () => {
    it('should return user data when authenticated', async () => {
      const mockUser = {
        userId: '123',
        username: 'admin',
        firstName: 'Admin',
        lastName: 'User',
        role: 'Administrator',
        assignedPointOfSales: [],
      };

      vi.mocked(apiClient.get).mockResolvedValueOnce({ data: mockUser });

      const result = await authService.getCurrentUser();

      expect(apiClient.get).toHaveBeenCalledWith(
        '/auth/me',
        { withCredentials: true }
      );
      expect(result).toEqual(mockUser);
    });

    it('should return null when not authenticated', async () => {
      vi.mocked(apiClient.get).mockRejectedValueOnce(new Error('Unauthorized'));

      const result = await authService.getCurrentUser();

      expect(result).toBeNull();
    });

    it('should include assigned point of sales for operators', async () => {
      const mockUser = {
        userId: '456',
        username: 'operator1',
        firstName: 'Operator',
        lastName: 'User',
        role: 'Operator',
        assignedPointOfSales: [
          { pointOfSaleId: 'pos1', name: 'Store 1', code: 'S1', assignedAt: '2024-01-01', isActive: true },
          { pointOfSaleId: 'pos2', name: 'Store 2', code: 'S2', assignedAt: '2024-01-01', isActive: true },
        ],
      };

      vi.mocked(apiClient.get).mockResolvedValueOnce({ data: mockUser });

      const result = await authService.getCurrentUser();

      expect(result?.assignedPointOfSales).toHaveLength(2);
      expect(result?.role).toBe('Operator');
    });
  });
});
