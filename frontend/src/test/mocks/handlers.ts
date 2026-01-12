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

  // Get product by ID - T-EP1-003-004 (Task 12.11)
  http.get(`${API_BASE}/products/:id`, ({ params }) => {
    const mockProduct = {
      id: params.id,
      sku: 'JOY-001',
      name: 'Gold Ring 18K',
      description: 'Beautiful gold ring',
      price: 299.99,
      collectionId: 'col-1',
      collectionName: 'Summer 2024',
      isActive: true,
      createdAt: '2024-01-01T00:00:00Z',
      updatedAt: '2024-01-01T00:00:00Z',
      photos: [],
    };

    return HttpResponse.json(mockProduct);
  }),

  // Update product - T-EP1-003-004 (Task 12.11)
  http.put(`${API_BASE}/products/:id`, async ({ params, request }) => {
    const body = await request.json() as {
      name: string;
      description?: string;
      price: number;
      collectionId?: string;
      isActive: boolean;
    };

    // Validation: Check for invalid price
    if (body.price <= 0) {
      return HttpResponse.json(
        { message: 'Price must be greater than zero' },
        { status: 400 }
      );
    }

    // Validation: Check for empty name
    if (!body.name || body.name.trim() === '') {
      return HttpResponse.json(
        { message: 'Name is required' },
        { status: 400 }
      );
    }

    // Return updated product
    const updatedProduct = {
      id: params.id,
      sku: 'JOY-001', // SKU is immutable
      name: body.name,
      description: body.description || '',
      price: body.price,
      collectionId: body.collectionId || null,
      collectionName: body.collectionId ? 'Summer 2024' : null,
      isActive: body.isActive,
      createdAt: '2024-01-01T00:00:00Z',
      updatedAt: new Date().toISOString(),
      photos: [],
    };

    return HttpResponse.json(updatedProduct);
  }),

  // Get collections - T-EP1-003-004 (Task 12.11)
  http.get(`${API_BASE}/products/collections`, () => {
    return HttpResponse.json([
      {
        id: 'col-1',
        name: 'Summer 2024',
        description: 'Summer collection',
        createdAt: '2024-01-01T00:00:00Z',
        updatedAt: '2024-01-01T00:00:00Z',
        productCount: 5,
      },
      {
        id: 'col-2',
        name: 'Winter 2024',
        description: 'Winter collection',
        createdAt: '2024-01-01T00:00:00Z',
        updatedAt: '2024-01-01T00:00:00Z',
        productCount: 3,
      },
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

  // Upload product photo - T-EP1-004 (Task 16.10)
  http.post(`${API_BASE}/products/:productId/photos`, async ({ params, request }) => {
    const formData = await request.formData();
    const file = formData.get('file') as File;

    // Validation: Check file format
    if (file && !['image/jpeg', 'image/jpg', 'image/png'].includes(file.type)) {
      return HttpResponse.json(
        { error: 'Only JPG and PNG formats are allowed' },
        { status: 400 }
      );
    }

    // Validation: Check file size (5MB limit)
    if (file && file.size > 5 * 1024 * 1024) {
      return HttpResponse.json(
        { error: 'File size exceeds 5MB limit' },
        { status: 413 }
      );
    }

    // Return created photo
    const newPhoto = {
      id: `photo-${Date.now()}`,
      productId: params.productId,
      fileName: file?.name || 'uploaded.jpg',
      url: `http://localhost/uploads/${file?.name}`,
      displayOrder: 0,
      isPrimary: true,
      createdAt: new Date().toISOString(),
      updatedAt: new Date().toISOString(),
    };

    return HttpResponse.json(newPhoto, { status: 201 });
  }),

  // Set primary photo - T-EP1-004 (Task 16.10)
  http.put(`${API_BASE}/products/:productId/photos/:photoId/primary`, ({ params }) => {
    return new HttpResponse(null, { status: 204 });
  }),

  // Delete product photo - T-EP1-004 (Task 16.10)
  http.delete(`${API_BASE}/products/:productId/photos/:photoId`, ({ params }) => {
    return new HttpResponse(null, { status: 204 });
  }),

  // Update photo display order - T-EP1-004 (Task 16.10)
  http.put(`${API_BASE}/products/:productId/photos/order`, async ({ request }) => {
    const body = await request.json();
    
    if (!body || Object.keys(body).length === 0) {
      return HttpResponse.json(
        { error: 'Photo order mapping is required' },
        { status: 400 }
      );
    }

    return new HttpResponse(null, { status: 204 });
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
