/**
 * MSW Request Handlers
 * Define mock API responses for testing
 */

import { http, HttpResponse } from 'msw';
import { createMockUser, createMockUserList } from '../utils/test-data';

const API_BASE = '/api';

/**
 * Auth handlers
 */
const authHandlers = [
  // Login
  http.post(`${API_BASE}/auth/login`, async ({ request }) => {
    const body = await request.json() as { email: string; password: string };
    
    if (body.email === 'admin@test.com' && body.password === 'password') {
      return HttpResponse.json({
        accessToken: 'mock-access-token',
        refreshToken: 'mock-refresh-token',
      });
    }
    
    return HttpResponse.json(
      { message: 'Invalid credentials' },
      { status: 401 }
    );
  }),

  // Get current user
  http.get(`${API_BASE}/auth/me`, () => {
    return HttpResponse.json(createMockUser());
  }),

  // Logout
  http.post(`${API_BASE}/auth/logout`, () => {
    return new HttpResponse(null, { status: 204 });
  }),

  // Refresh token
  http.post(`${API_BASE}/auth/refresh`, () => {
    return HttpResponse.json({
      accessToken: 'mock-new-access-token',
    });
  }),
];

/**
 * User management handlers
 */
const userHandlers = [
  // Get users list
  http.get(`${API_BASE}/users`, () => {
    return HttpResponse.json({
      items: createMockUserList(5),
      total: 5,
      page: 1,
      pageSize: 10,
    });
  }),

  // Get user by ID
  http.get(`${API_BASE}/users/:id`, ({ params }) => {
    return HttpResponse.json(
      createMockUser({ id: params.id as string })
    );
  }),
];

/**
 * Product handlers
 */
const productHandlers = [
  // Get products list
  http.get(`${API_BASE}/products`, () => {
    return HttpResponse.json([
      { id: '1', sku: 'JOY-001', name: 'Gold Ring', price: 299.99, isActive: true },
      { id: '2', sku: 'JOY-002', name: 'Silver Necklace', price: 199.99, isActive: true },
    ]);
  }),

  // Import products
  http.post(`${API_BASE}/products/import`, () => {
    return HttpResponse.json({
      success: true,
      totalRows: 2,
      createdCount: 1,
      updatedCount: 1,
      collectionsCreatedCount: 0,
      errors: [],
      warnings: [],
      message: 'Import successful',
    });
  }),

  // Validate import
  http.post(`${API_BASE}/products/import/validate`, () => {
    return HttpResponse.json({
      success: true,
      totalRows: 2,
      createdCount: 0,
      updatedCount: 0,
      collectionsCreatedCount: 0,
      errors: [],
      warnings: [],
      message: 'Validation passed',
    });
  }),
];

/**
 * All handlers combined
 * Export for use in MSW server setup
 */
export const handlers = [
  ...authHandlers,
  ...userHandlers,
  ...productHandlers,
];
