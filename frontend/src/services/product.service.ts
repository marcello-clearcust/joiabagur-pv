/**
 * Product Management Service
 * Handles product CRUD operations and Excel import.
 */

import apiClient from './api.service';
import {
  Product,
  Collection,
  CreateProductRequest,
  UpdateProductRequest,
  ImportResult,
} from '@/types/product.types';

const PRODUCT_ENDPOINTS = {
  PRODUCTS: '/products',
  PRODUCT_BY_ID: (id: string) => `/products/${id}`,
  PRODUCT_BY_SKU: (sku: string) => `/products/by-sku/${sku}`,
  PRODUCT_ACTIVATE: (id: string) => `/products/${id}/activate`,
  PRODUCT_PHOTOS: (id: string) => `/products/${id}/photos`,
  PRODUCT_PHOTO: (productId: string, photoId: string) => `/products/${productId}/photos/${photoId}`,
  SET_PRIMARY_PHOTO: (productId: string, photoId: string) => `/products/${productId}/photos/${photoId}/set-primary`,
  IMPORT: '/products/import',
  IMPORT_VALIDATE: '/products/import/validate',
} as const;

/**
 * Product Service - Product management operations
 */
export const productService = {
  /**
   * Get all products
   */
  getProducts: async (includeInactive = true): Promise<Product[]> => {
    const response = await apiClient.get<Product[]>(PRODUCT_ENDPOINTS.PRODUCTS, {
      params: { includeInactive },
    });
    return response.data;
  },

  /**
   * Get a single product by ID
   */
  getProduct: async (productId: string): Promise<Product> => {
    const response = await apiClient.get<Product>(
      PRODUCT_ENDPOINTS.PRODUCT_BY_ID(productId)
    );
    return response.data;
  },

  /**
   * Get a product by SKU
   */
  getProductBySku: async (sku: string): Promise<Product> => {
    const response = await apiClient.get<Product>(
      PRODUCT_ENDPOINTS.PRODUCT_BY_SKU(sku)
    );
    return response.data;
  },

  /**
   * Check if a SKU already exists
   * Returns true if SKU exists, false otherwise
   */
  checkSkuExists: async (sku: string): Promise<boolean> => {
    try {
      await apiClient.get<Product>(PRODUCT_ENDPOINTS.PRODUCT_BY_SKU(sku));
      return true; // SKU exists
    } catch {
      return false; // SKU doesn't exist or error
    }
  },

  /**
   * Get all unique collections from existing products
   * Note: This extracts collections from products since there's no dedicated collections endpoint
   */
  getCollections: async (): Promise<Collection[]> => {
    const products = await productService.getProducts(true);
    const collectionsMap = new Map<string, Collection>();
    
    products.forEach((product) => {
      if (product.collectionId && product.collectionName) {
        if (!collectionsMap.has(product.collectionId)) {
          collectionsMap.set(product.collectionId, {
            id: product.collectionId,
            name: product.collectionName,
            description: undefined,
            createdAt: product.createdAt,
            updatedAt: product.updatedAt,
            productCount: 1,
          });
        } else {
          const existing = collectionsMap.get(product.collectionId)!;
          existing.productCount++;
        }
      }
    });
    
    return Array.from(collectionsMap.values()).sort((a, b) => 
      a.name.localeCompare(b.name)
    );
  },

  /**
   * Create a new product
   */
  createProduct: async (data: CreateProductRequest): Promise<Product> => {
    const response = await apiClient.post<Product>(PRODUCT_ENDPOINTS.PRODUCTS, data);
    return response.data;
  },

  /**
   * Update an existing product
   */
  updateProduct: async (productId: string, data: UpdateProductRequest): Promise<Product> => {
    const response = await apiClient.put<Product>(
      PRODUCT_ENDPOINTS.PRODUCT_BY_ID(productId),
      data
    );
    return response.data;
  },

  /**
   * Deactivate (soft delete) a product
   */
  deactivateProduct: async (productId: string): Promise<void> => {
    await apiClient.delete(PRODUCT_ENDPOINTS.PRODUCT_BY_ID(productId));
  },

  /**
   * Reactivate a deactivated product
   */
  activateProduct: async (productId: string): Promise<void> => {
    await apiClient.post(PRODUCT_ENDPOINTS.PRODUCT_ACTIVATE(productId));
  },

  /**
   * Validate Excel file for product import
   */
  validateImport: async (file: File): Promise<ImportResult> => {
    const formData = new FormData();
    formData.append('file', file);

    const response = await apiClient.post<ImportResult>(
      PRODUCT_ENDPOINTS.IMPORT_VALIDATE,
      formData,
      {
        headers: {
          'Content-Type': 'multipart/form-data',
        },
      }
    );
    return response.data;
  },

  /**
   * Import products from Excel file
   */
  importProducts: async (file: File): Promise<ImportResult> => {
    const formData = new FormData();
    formData.append('file', file);

    const response = await apiClient.post<ImportResult>(
      PRODUCT_ENDPOINTS.IMPORT,
      formData,
      {
        headers: {
          'Content-Type': 'multipart/form-data',
        },
      }
    );
    return response.data;
  },

  /**
   * Upload a photo for a product
   */
  uploadPhoto: async (productId: string, file: File): Promise<Product> => {
    const formData = new FormData();
    formData.append('file', file);

    const response = await apiClient.post<Product>(
      PRODUCT_ENDPOINTS.PRODUCT_PHOTOS(productId),
      formData,
      {
        headers: {
          'Content-Type': 'multipart/form-data',
        },
      }
    );
    return response.data;
  },

  /**
   * Get all photos for a product
   */
  getProductPhotos: async (productId: string): Promise<Product[]> => {
    const response = await apiClient.get<Product[]>(
      PRODUCT_ENDPOINTS.PRODUCT_PHOTOS(productId)
    );
    return response.data;
  },

  /**
   * Set a photo as primary
   */
  setPrimaryPhoto: async (productId: string, photoId: string): Promise<void> => {
    await apiClient.post(
      PRODUCT_ENDPOINTS.SET_PRIMARY_PHOTO(productId, photoId)
    );
  },

  /**
   * Delete a photo
   */
  deletePhoto: async (productId: string, photoId: string): Promise<void> => {
    await apiClient.delete(
      PRODUCT_ENDPOINTS.PRODUCT_PHOTO(productId, photoId)
    );
  },
};

export default productService;

