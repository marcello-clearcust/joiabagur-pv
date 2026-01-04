/**
 * User Management Service
 * Handles user CRUD operations and point-of-sale assignments.
 */

import apiClient from './api.service';
import {
  User,
  UserListItem,
  CreateUserRequest,
  UpdateUserRequest,
  ChangePasswordRequest,
} from '@/types/user.types';
import { UserPointOfSale } from '@/types/auth.types';
import { PaginationParams } from '@/types/common.types';

const USER_ENDPOINTS = {
  USERS: '/users',
  USER_BY_ID: (id: string) => `/users/${id}`,
  USER_PASSWORD: (id: string) => `/users/${id}/password`,
  USER_POINT_OF_SALES: (userId: string) => `/users/${userId}/point-of-sales`,
  USER_POINT_OF_SALE: (userId: string, posId: string) =>
    `/users/${userId}/point-of-sales/${posId}`,
} as const;

/**
 * User Service - User management operations
 */
export const userService = {
  /**
   * Get all users with optional pagination
   */
  getUsers: async (params?: PaginationParams): Promise<UserListItem[]> => {
    const response = await apiClient.get<UserListItem[]>(USER_ENDPOINTS.USERS, {
      params,
    });
    return response.data;
  },

  /**
   * Get a single user by ID
   */
  getUser: async (userId: string): Promise<User> => {
    const response = await apiClient.get<User>(
      USER_ENDPOINTS.USER_BY_ID(userId)
    );
    return response.data;
  },

  /**
   * Create a new user
   */
  createUser: async (data: CreateUserRequest): Promise<User> => {
    const response = await apiClient.post<User>(USER_ENDPOINTS.USERS, data);
    return response.data;
  },

  /**
   * Update an existing user
   */
  updateUser: async (userId: string, data: UpdateUserRequest): Promise<User> => {
    const response = await apiClient.put<User>(
      USER_ENDPOINTS.USER_BY_ID(userId),
      data
    );
    return response.data;
  },

  /**
   * Change user's password (admin only)
   */
  changePassword: async (
    userId: string,
    data: ChangePasswordRequest
  ): Promise<void> => {
    await apiClient.put(USER_ENDPOINTS.USER_PASSWORD(userId), data);
  },

  /**
   * Get point-of-sale assignments for a user
   */
  getUserPointOfSales: async (userId: string): Promise<UserPointOfSale[]> => {
    const response = await apiClient.get<UserPointOfSale[]>(
      USER_ENDPOINTS.USER_POINT_OF_SALES(userId)
    );
    return response.data;
  },

  /**
   * Assign user to a point of sale
   */
  assignUserToPointOfSale: async (
    userId: string,
    pointOfSaleId: string
  ): Promise<UserPointOfSale> => {
    const response = await apiClient.post<UserPointOfSale>(
      USER_ENDPOINTS.USER_POINT_OF_SALE(userId, pointOfSaleId)
    );
    return response.data;
  },

  /**
   * Unassign user from a point of sale
   */
  unassignUserFromPointOfSale: async (
    userId: string,
    pointOfSaleId: string
  ): Promise<void> => {
    await apiClient.delete(
      USER_ENDPOINTS.USER_POINT_OF_SALE(userId, pointOfSaleId)
    );
  },
};

export default userService;
