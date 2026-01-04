/**
 * User Management type definitions
 */

import { BaseEntity, UserRole, PaginatedResponse } from './common.types';
import { UserPointOfSale } from './auth.types';

/**
 * User entity from API
 */
export interface User extends BaseEntity {
  username: string;
  firstName: string;
  lastName: string;
  email?: string;
  role: UserRole;
  isActive: boolean;
  lastLoginAt?: string;
  assignedPointOfSales?: UserPointOfSale[];
}

/**
 * User list item (compact)
 */
export interface UserListItem {
  id: string;
  username: string;
  firstName: string;
  lastName: string;
  email?: string;
  role: UserRole;
  isActive: boolean;
}

/**
 * Create user request
 */
export interface CreateUserRequest {
  username: string;
  password: string;
  firstName: string;
  lastName: string;
  email?: string;
  role: UserRole;
}

/**
 * Update user request
 */
export interface UpdateUserRequest {
  firstName?: string;
  lastName?: string;
  email?: string;
  role?: UserRole;
  isActive?: boolean;
}

/**
 * Change password request
 */
export interface ChangePasswordRequest {
  newPassword: string;
}

/**
 * User list response
 */
export type UserListResponse = PaginatedResponse<UserListItem>;

/**
 * Assign user to point of sale request
 */
export interface AssignUserToPointOfSaleRequest {
  pointOfSaleId: string;
}
