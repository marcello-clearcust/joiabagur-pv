/**
 * Unit tests for Product Service
 * Tests product CRUD operations and Excel import
 */

import { describe, it, expect, vi, beforeEach } from 'vitest';
import { productService } from './product.service';
import apiClient from './api.service';
import { Product, ImportResult } from '@/types/product.types';

// Mock the API client
vi.mock('./api.service', () => ({
  default: {
    get: vi.fn(),
    post: vi.fn(),
    put: vi.fn(),
    delete: vi.fn(),
  },
}));

const mockProduct: Product = {
  id: '1',
  sku: 'JOY-001',
  name: 'Gold Ring',
  description: 'Beautiful gold ring',
  price: 299.99,
  collectionId: 'col-1',
  collectionName: 'Summer',
  isActive: true,
  createdAt: '2024-01-01T00:00:00Z',
  updatedAt: '2024-01-01T00:00:00Z',
  photos: [],
};

const mockProducts: Product[] = [
  mockProduct,
  {
    id: '2',
    sku: 'JOY-002',
    name: 'Silver Necklace',
    price: 199.99,
    isActive: true,
    createdAt: '2024-01-01T00:00:00Z',
    updatedAt: '2024-01-01T00:00:00Z',
    photos: [],
  },
];

const mockImportResult: ImportResult = {
  success: true,
  totalRows: 5,
  createdCount: 3,
  updatedCount: 2,
  collectionsCreatedCount: 1,
  errors: [],
  warnings: [],
  message: 'Import successful',
};

describe('productService', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  describe('getProducts', () => {
    it('should fetch all products', async () => {
      vi.mocked(apiClient.get).mockResolvedValue({ data: mockProducts });

      const result = await productService.getProducts();

      expect(apiClient.get).toHaveBeenCalledWith('/products', {
        params: { includeInactive: true },
      });
      expect(result).toEqual(mockProducts);
    });

    it('should fetch only active products when includeInactive is false', async () => {
      vi.mocked(apiClient.get).mockResolvedValue({ data: mockProducts });

      await productService.getProducts(false);

      expect(apiClient.get).toHaveBeenCalledWith('/products', {
        params: { includeInactive: false },
      });
    });
  });

  describe('getProduct', () => {
    it('should fetch a single product by ID', async () => {
      vi.mocked(apiClient.get).mockResolvedValue({ data: mockProduct });

      const result = await productService.getProduct('1');

      expect(apiClient.get).toHaveBeenCalledWith('/products/1');
      expect(result).toEqual(mockProduct);
    });
  });

  describe('getProductBySku', () => {
    it('should fetch a product by SKU', async () => {
      vi.mocked(apiClient.get).mockResolvedValue({ data: mockProduct });

      const result = await productService.getProductBySku('JOY-001');

      expect(apiClient.get).toHaveBeenCalledWith('/products/by-sku/JOY-001');
      expect(result).toEqual(mockProduct);
    });
  });

  describe('createProduct', () => {
    it('should create a new product', async () => {
      const createData = {
        sku: 'JOY-003',
        name: 'Diamond Bracelet',
        price: 599.99,
      };
      vi.mocked(apiClient.post).mockResolvedValue({ data: { ...mockProduct, ...createData } });

      const result = await productService.createProduct(createData);

      expect(apiClient.post).toHaveBeenCalledWith('/products', createData);
      expect(result.sku).toBe(createData.sku);
    });
  });

  describe('updateProduct', () => {
    it('should update an existing product', async () => {
      const updateData = {
        name: 'Updated Gold Ring',
        price: 399.99,
        isActive: true,
      };
      vi.mocked(apiClient.put).mockResolvedValue({ data: { ...mockProduct, ...updateData } });

      const result = await productService.updateProduct('1', updateData);

      expect(apiClient.put).toHaveBeenCalledWith('/products/1', updateData);
      expect(result.name).toBe(updateData.name);
    });
  });

  describe('deactivateProduct', () => {
    it('should deactivate a product', async () => {
      vi.mocked(apiClient.delete).mockResolvedValue({});

      await productService.deactivateProduct('1');

      expect(apiClient.delete).toHaveBeenCalledWith('/products/1');
    });
  });

  describe('activateProduct', () => {
    it('should activate a product', async () => {
      vi.mocked(apiClient.post).mockResolvedValue({});

      await productService.activateProduct('1');

      expect(apiClient.post).toHaveBeenCalledWith('/products/1/activate');
    });
  });

  describe('validateImport', () => {
    it('should validate an Excel file', async () => {
      const file = new File(['test'], 'products.xlsx', {
        type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
      });

      vi.mocked(apiClient.post).mockResolvedValue({ data: mockImportResult });

      const result = await productService.validateImport(file);

      expect(apiClient.post).toHaveBeenCalledWith(
        '/products/import/validate',
        expect.any(FormData),
        expect.objectContaining({
          headers: { 'Content-Type': 'multipart/form-data' },
        })
      );
      expect(result).toEqual(mockImportResult);
    });
  });

  describe('importProducts', () => {
    it('should import products from an Excel file', async () => {
      const file = new File(['test'], 'products.xlsx', {
        type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
      });

      vi.mocked(apiClient.post).mockResolvedValue({ data: mockImportResult });

      const result = await productService.importProducts(file);

      expect(apiClient.post).toHaveBeenCalledWith(
        '/products/import',
        expect.any(FormData),
        expect.objectContaining({
          headers: { 'Content-Type': 'multipart/form-data' },
        })
      );
      expect(result.success).toBe(true);
      expect(result.createdCount).toBe(3);
      expect(result.updatedCount).toBe(2);
    });
  });
});




