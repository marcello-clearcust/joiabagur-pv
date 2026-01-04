import axios, {
  AxiosError,
  AxiosInstance,
  AxiosResponse,
  InternalAxiosRequestConfig,
} from 'axios';
import { ApiError, ApiResponse } from '@/types/api.types';

/**
 * Base API service with Axios client configured for backend communication.
 * Uses HTTP-only cookies for JWT authentication (set by backend).
 * Provides automatic token refresh on 401 responses.
 */

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000/api';

// Create Axios instance with default configuration
const apiClient: AxiosInstance = axios.create({
  baseURL: API_BASE_URL,
  timeout: 30000,
  headers: {
    'Content-Type': 'application/json',
  },
  withCredentials: true, // Always send cookies for auth
});

// Track if we're currently refreshing to prevent multiple refresh attempts
let isRefreshing = false;
let failedQueue: Array<{
  resolve: (value: unknown) => void;
  reject: (reason?: unknown) => void;
}> = [];

const processQueue = (error: AxiosError | null) => {
  failedQueue.forEach((promise) => {
    if (error) {
      promise.reject(error);
    } else {
      promise.resolve(undefined);
    }
  });
  failedQueue = [];
};

// Request interceptor - minimal, cookies are sent automatically
apiClient.interceptors.request.use(
  (config: InternalAxiosRequestConfig) => {
    return config;
  },
  (error: AxiosError) => {
    return Promise.reject(error);
  }
);

// Response interceptor - handles errors and automatic token refresh
apiClient.interceptors.response.use(
  (response: AxiosResponse) => {
    return response;
  },
  async (error: AxiosError<ApiError>) => {
    const originalRequest = error.config as InternalAxiosRequestConfig & {
      _retry?: boolean;
    };

    // Handle 401 Unauthorized - attempt token refresh
    if (error.response?.status === 401 && !originalRequest._retry) {
      // Don't try to refresh for auth endpoints
      if (originalRequest.url?.includes('/auth/')) {
        return Promise.reject(error);
      }

      if (isRefreshing) {
        // If already refreshing, queue this request
        return new Promise((resolve, reject) => {
          failedQueue.push({ resolve, reject });
        })
          .then(() => apiClient(originalRequest))
          .catch((err) => Promise.reject(err));
      }

      originalRequest._retry = true;
      isRefreshing = true;

      try {
        // Attempt to refresh the token
        await apiClient.post('/auth/refresh', null, {
          withCredentials: true,
        });

        processQueue(null);
        return apiClient(originalRequest);
      } catch (refreshError) {
        processQueue(refreshError as AxiosError);
        // Refresh failed - redirect to login
        window.location.href = '/auth/login';
        return Promise.reject(refreshError);
      } finally {
        isRefreshing = false;
      }
    }

    // Handle 403 Forbidden - user doesn't have permission
    if (error.response?.status === 403) {
      // Could redirect to a forbidden page or show a message
      console.warn('Access forbidden:', originalRequest.url);
    }

    // Handle other errors
    console.log('API Error Response:', error.response?.data);
    console.log('Error status:', error.response?.status);
    console.log('Error message:', error.message);

    let message = error.message || 'An error occurred';

    // Try to extract message from various response formats
    if (error.response?.data) {
      const data = error.response.data;

      // Format 1: { error: { message: "..." } } (camelCase)
      if (data.error?.message) {
        message = data.error.message;
      }
      // Format 2: { Error: { Message: "..." } } (PascalCase)
      else if (data.Error?.Message) {
        message = data.Error.Message;
      }
      // Format 3: { message: "..." } (camelCase)
      else if (data.message) {
        message = data.message;
      }
      // Format 4: { Message: "..." } (PascalCase)
      else if (data.Message) {
        message = data.Message;
      }
      // Format 5: { error: "..." } (string, camelCase)
      else if (typeof data.error === 'string') {
        message = data.error;
      }
      // Format 6: { Error: "..." } (string, PascalCase)
      else if (typeof data.Error === 'string') {
        message = data.Error;
      }
      // Format 7: response is a string
      else if (typeof data === 'string') {
        message = data;
      }
    }

    const apiError: ApiError = {
      message,
      statusCode: error.response?.status || 500,
      errors: error.response?.data?.errors,
    };

    return Promise.reject(apiError);
  }
);

/**
 * API Service - Generic methods for HTTP requests
 */
export const apiService = {
  /**
   * GET request
   */
  get: async <T>(url: string, params?: Record<string, unknown>): Promise<ApiResponse<T>> => {
    const response = await apiClient.get<ApiResponse<T>>(url, { params });
    return response.data;
  },

  /**
   * POST request
   */
  post: async <T>(url: string, data?: unknown): Promise<ApiResponse<T>> => {
    const response = await apiClient.post<ApiResponse<T>>(url, data);
    return response.data;
  },

  /**
   * PUT request
   */
  put: async <T>(url: string, data?: unknown): Promise<ApiResponse<T>> => {
    const response = await apiClient.put<ApiResponse<T>>(url, data);
    return response.data;
  },

  /**
   * PATCH request
   */
  patch: async <T>(url: string, data?: unknown): Promise<ApiResponse<T>> => {
    const response = await apiClient.patch<ApiResponse<T>>(url, data);
    return response.data;
  },

  /**
   * DELETE request
   */
  delete: async <T>(url: string): Promise<ApiResponse<T>> => {
    const response = await apiClient.delete<ApiResponse<T>>(url);
    return response.data;
  },

  /**
   * Upload file with multipart/form-data
   */
  upload: async <T>(url: string, formData: FormData): Promise<ApiResponse<T>> => {
    const response = await apiClient.post<ApiResponse<T>>(url, formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
    return response.data;
  },
};

// Export apiClient as both default and named export for flexibility
export { apiClient };
export default apiClient;
