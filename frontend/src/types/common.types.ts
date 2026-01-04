/**
 * Common type definitions shared across the application
 */

/**
 * Pagination metadata for list responses
 * Max page size is 50 per project constraints
 */
export interface PaginationMeta {
  currentPage: number;
  pageSize: number;
  totalPages: number;
  totalCount: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

/**
 * Paginated response wrapper
 */
export interface PaginatedResponse<T> {
  data: T[];
  pagination: PaginationMeta;
}

/**
 * Pagination request parameters
 */
export interface PaginationParams {
  page?: number;
  pageSize?: number;
}

/**
 * Sort direction
 */
export type SortDirection = 'asc' | 'desc';

/**
 * Sort parameters
 */
export interface SortParams {
  sortBy?: string;
  sortDirection?: SortDirection;
}

/**
 * User roles in the system
 */
export type UserRole = 'Administrator' | 'Operator';

/**
 * Base entity with common fields
 */
export interface BaseEntity {
  id: string;
  createdAt: string;
  updatedAt: string;
}

/**
 * Select option for dropdowns
 */
export interface SelectOption<T = string> {
  label: string;
  value: T;
}
