/**
 * Unit tests for UserService
 * Tests user management operations: CRUD, password change, point of sale assignments
 */

import { describe, it, expect, vi, beforeEach } from 'vitest';
import { userService } from './user.service';
import apiClient from './api.service';

// Mock the API client
vi.mock('./api.service', () => ({
  default: {
    get: vi.fn(),
    post: vi.fn(),
    put: vi.fn(),
    delete: vi.fn(),
  },
}));

describe('UserService', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  describe('getUsers', () => {
    it('should fetch all users', async () => {
      const mockUsers = [
        { id: '1', username: 'user1', firstName: 'User', lastName: 'One', role: 'Operator', isActive: true },
        { id: '2', username: 'user2', firstName: 'User', lastName: 'Two', role: 'Administrator', isActive: true },
      ];

      vi.mocked(apiClient.get).mockResolvedValueOnce({ data: mockUsers });

      const result = await userService.getUsers();

      expect(apiClient.get).toHaveBeenCalledWith('/users', { params: undefined });
      expect(result).toEqual(mockUsers);
    });

    it('should pass pagination params', async () => {
      vi.mocked(apiClient.get).mockResolvedValueOnce({ data: [] });

      await userService.getUsers({ page: 2, pageSize: 20 });

      expect(apiClient.get).toHaveBeenCalledWith('/users', { params: { page: 2, pageSize: 20 } });
    });
  });

  describe('getUser', () => {
    it('should fetch user by ID', async () => {
      const mockUser = {
        id: '123',
        username: 'testuser',
        firstName: 'Test',
        lastName: 'User',
        role: 'Operator',
        isActive: true,
      };

      vi.mocked(apiClient.get).mockResolvedValueOnce({ data: mockUser });

      const result = await userService.getUser('123');

      expect(apiClient.get).toHaveBeenCalledWith('/users/123');
      expect(result).toEqual(mockUser);
    });
  });

  describe('createUser', () => {
    it('should create a new user', async () => {
      const newUser = {
        username: 'newuser',
        password: 'password123',
        firstName: 'New',
        lastName: 'User',
        role: 'Operator' as const,
      };

      const createdUser = { id: '456', ...newUser, isActive: true };
      vi.mocked(apiClient.post).mockResolvedValueOnce({ data: createdUser });

      const result = await userService.createUser(newUser);

      expect(apiClient.post).toHaveBeenCalledWith('/users', newUser);
      expect(result.id).toBe('456');
    });

    it('should handle duplicate username error', async () => {
      const newUser = {
        username: 'existinguser',
        password: 'password123',
        firstName: 'Test',
        lastName: 'User',
        role: 'Operator' as const,
      };

      vi.mocked(apiClient.post).mockRejectedValueOnce(new Error('El nombre de usuario ya está en uso'));

      await expect(userService.createUser(newUser)).rejects.toThrow('El nombre de usuario ya está en uso');
    });
  });

  describe('updateUser', () => {
    it('should update user data', async () => {
      const updateData = {
        firstName: 'Updated',
        lastName: 'Name',
        isActive: false,
      };

      const updatedUser = {
        id: '123',
        username: 'testuser',
        ...updateData,
        role: 'Operator',
      };

      vi.mocked(apiClient.put).mockResolvedValueOnce({ data: updatedUser });

      const result = await userService.updateUser('123', updateData);

      expect(apiClient.put).toHaveBeenCalledWith('/users/123', updateData);
      expect(result.firstName).toBe('Updated');
      expect(result.isActive).toBe(false);
    });

    it('should update user role', async () => {
      const updateData = { role: 'Administrator' as const };

      vi.mocked(apiClient.put).mockResolvedValueOnce({
        data: { id: '123', role: 'Administrator' },
      });

      const result = await userService.updateUser('123', updateData);

      expect(result.role).toBe('Administrator');
    });
  });

  describe('changePassword', () => {
    it('should change user password', async () => {
      vi.mocked(apiClient.put).mockResolvedValueOnce({ data: null });

      await userService.changePassword('123', { newPassword: 'newPassword123' });

      expect(apiClient.put).toHaveBeenCalledWith('/users/123/password', { newPassword: 'newPassword123' });
    });
  });

  describe('getUserPointOfSales', () => {
    it('should fetch user point of sale assignments', async () => {
      const mockAssignments = [
        { pointOfSaleId: 'pos1', name: 'Store 1', code: 'S1', assignedAt: '2024-01-01', isActive: true },
        { pointOfSaleId: 'pos2', name: 'Store 2', code: 'S2', assignedAt: '2024-01-01', isActive: false },
      ];

      vi.mocked(apiClient.get).mockResolvedValueOnce({ data: mockAssignments });

      const result = await userService.getUserPointOfSales('123');

      expect(apiClient.get).toHaveBeenCalledWith('/users/123/point-of-sales');
      expect(result).toHaveLength(2);
      expect(result[0].isActive).toBe(true);
    });
  });

  describe('assignUserToPointOfSale', () => {
    it('should assign user to point of sale', async () => {
      const mockAssignment = {
        pointOfSaleId: 'pos1',
        name: 'Store 1',
        code: 'S1',
        assignedAt: '2024-01-01',
        isActive: true,
      };

      vi.mocked(apiClient.post).mockResolvedValueOnce({ data: mockAssignment });

      const result = await userService.assignUserToPointOfSale('123', 'pos1');

      expect(apiClient.post).toHaveBeenCalledWith('/users/123/point-of-sales/pos1');
      expect(result.isActive).toBe(true);
    });

    it('should handle already assigned error', async () => {
      vi.mocked(apiClient.post).mockRejectedValueOnce(
        new Error('El operador ya está asignado a este punto de venta')
      );

      await expect(userService.assignUserToPointOfSale('123', 'pos1')).rejects.toThrow(
        'El operador ya está asignado a este punto de venta'
      );
    });
  });

  describe('unassignUserFromPointOfSale', () => {
    it('should unassign user from point of sale', async () => {
      vi.mocked(apiClient.delete).mockResolvedValueOnce({ data: null });

      await userService.unassignUserFromPointOfSale('123', 'pos1');

      expect(apiClient.delete).toHaveBeenCalledWith('/users/123/point-of-sales/pos1');
    });

    it('should handle last assignment error', async () => {
      vi.mocked(apiClient.delete).mockRejectedValueOnce(
        new Error('Un operador debe tener al menos un punto de venta asignado')
      );

      await expect(userService.unassignUserFromPointOfSale('123', 'pos1')).rejects.toThrow(
        'Un operador debe tener al menos un punto de venta asignado'
      );
    });
  });
});
