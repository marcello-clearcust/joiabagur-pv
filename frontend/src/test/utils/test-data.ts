/**
 * Test Data Factories
 * Helper functions to create consistent test data
 */

import type { AuthUser } from '@/types/auth.types';
import type { UserListItem } from '@/types/user.types';

/**
 * Create a mock authenticated user
 */
export function createMockUser(overrides: Partial<AuthUser> = {}): AuthUser {
  return {
    id: '1',
    email: 'admin@test.com',
    name: 'Test Admin',
    role: 'Admin',
    ...overrides,
  };
}

/**
 * Create a mock user list item for user management
 */
export function createMockUserListItem(overrides: Partial<UserListItem> = {}): UserListItem {
  return {
    id: '1',
    email: 'user@test.com',
    name: 'Test User',
    role: 'Operator',
    isActive: true,
    createdAt: new Date().toISOString(),
    ...overrides,
  };
}

/**
 * Create multiple mock users
 */
export function createMockUserList(count: number = 3): UserListItem[] {
  return Array.from({ length: count }, (_, i) => 
    createMockUserListItem({
      id: String(i + 1),
      email: `user${i + 1}@test.com`,
      name: `Test User ${i + 1}`,
      role: i === 0 ? 'Admin' : 'Operator',
    })
  );
}
