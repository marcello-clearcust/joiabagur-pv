/**
 * API type definitions for HTTP client
 */

/**
 * Standard API response wrapper
 */
export interface ApiResponse<T> {
  data: T;
  success: boolean;
  message?: string;
}

/**
 * API error structure
 */
export interface ApiError {
  message: string;
  statusCode: number;
  errors?: Record<string, string[]>;
}

/**
 * Validation error from backend
 */
export interface ValidationError {
  field: string;
  message: string;
}

/**
 * API response with validation errors
 */
export interface ValidationErrorResponse {
  message: string;
  errors: ValidationError[];
}
