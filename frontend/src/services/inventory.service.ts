/**
 * Inventory Management Service
 * Handles inventory operations including assignment, stock import, adjustments, and movement history.
 */

import apiClient from './api.service';
import {
  Inventory,
  CentralizedStock,
  PaginatedInventoryResult,
  PaginatedCentralizedStockResult,
  AssignProductRequest,
  BulkAssignProductsRequest,
  UnassignProductRequest,
  AssignmentResult,
  BulkAssignmentResult,
  StockAdjustmentRequest,
  StockAdjustmentResult,
  StockImportResult,
  PaginatedMovementResult,
  MovementHistoryFilter,
} from '@/types/inventory.types';

const INVENTORY_ENDPOINTS = {
  // Assignment
  ASSIGN: '/inventory/assign',
  ASSIGN_BULK: '/inventory/assign/bulk',
  UNASSIGN: '/inventory/unassign',
  ASSIGNED: '/inventory/assigned',
  
  // Stock view
  STOCK: '/inventory',
  CENTRALIZED: '/inventory/centralized',
  PRODUCT_BREAKDOWN: (productId: string) => `/inventory/product/${productId}`,
  
  // Import
  IMPORT: '/inventory/import',
  IMPORT_VALIDATE: '/inventory/import/validate',
  IMPORT_TEMPLATE: '/inventory/import-template',
  
  // Adjustment
  ADJUSTMENT: '/inventory/adjustment',
  
  // Movement history
  MOVEMENTS: '/inventory/movements',
} as const;

/**
 * Inventory Service - Inventory management operations
 */
export const inventoryService = {
  // ==================== Assignment Operations ====================

  /**
   * Assign a product to a point of sale
   */
  assignProduct: async (request: AssignProductRequest): Promise<AssignmentResult> => {
    const response = await apiClient.post<AssignmentResult>(
      INVENTORY_ENDPOINTS.ASSIGN,
      request
    );
    return response.data;
  },

  /**
   * Assign multiple products to a point of sale
   */
  assignProducts: async (request: BulkAssignProductsRequest): Promise<BulkAssignmentResult> => {
    const response = await apiClient.post<BulkAssignmentResult>(
      INVENTORY_ENDPOINTS.ASSIGN_BULK,
      request
    );
    return response.data;
  },

  /**
   * Unassign a product from a point of sale
   */
  unassignProduct: async (request: UnassignProductRequest): Promise<AssignmentResult> => {
    const response = await apiClient.post<AssignmentResult>(
      INVENTORY_ENDPOINTS.UNASSIGN,
      request
    );
    return response.data;
  },

  /**
   * Get products assigned to a point of sale
   */
  getAssignedProducts: async (
    pointOfSaleId: string,
    page = 1,
    pageSize = 50
  ): Promise<PaginatedInventoryResult> => {
    const response = await apiClient.get<PaginatedInventoryResult>(
      INVENTORY_ENDPOINTS.ASSIGNED,
      {
        params: { pointOfSaleId, page, pageSize },
      }
    );
    return response.data;
  },

  // ==================== Stock View Operations ====================

  /**
   * Get stock for a point of sale
   */
  getStock: async (
    pointOfSaleId: string,
    page = 1,
    pageSize = 50
  ): Promise<PaginatedInventoryResult> => {
    const response = await apiClient.get<PaginatedInventoryResult>(
      INVENTORY_ENDPOINTS.STOCK,
      {
        params: { pointOfSaleId, page, pageSize },
      }
    );
    return response.data;
  },

  /**
   * Get centralized stock view (aggregated by product)
   */
  getCentralizedStock: async (
    page = 1,
    pageSize = 50
  ): Promise<PaginatedCentralizedStockResult> => {
    const response = await apiClient.get<PaginatedCentralizedStockResult>(
      INVENTORY_ENDPOINTS.CENTRALIZED,
      {
        params: { page, pageSize },
      }
    );
    return response.data;
  },

  /**
   * Get stock breakdown for a product across all points of sale
   */
  getProductStockBreakdown: async (productId: string): Promise<CentralizedStock | null> => {
    try {
      const response = await apiClient.get<CentralizedStock>(
        INVENTORY_ENDPOINTS.PRODUCT_BREAKDOWN(productId)
      );
      return response.data;
    } catch {
      return null;
    }
  },

  /**
   * Search inventory by product name or SKU for a specific point of sale
   */
  searchInventory: async (pointOfSaleId: string, query: string): Promise<Inventory[]> => {
    if (!query || query.length < 2) {
      return [];
    }

    const response = await apiClient.get<Inventory[]>(
      `${INVENTORY_ENDPOINTS.STOCK}/search`,
      {
        params: { pointOfSaleId, query },
      }
    );
    return response.data;
  },

  // ==================== Stock Import Operations ====================

  /**
   * Download stock import template
   */
  downloadImportTemplate: async (): Promise<Blob> => {
    const response = await apiClient.get(INVENTORY_ENDPOINTS.IMPORT_TEMPLATE, {
      responseType: 'blob',
    });
    return response.data;
  },

  /**
   * Validate stock import file
   */
  validateImport: async (
    file: File,
    pointOfSaleId: string
  ): Promise<StockImportResult> => {
    const formData = new FormData();
    formData.append('file', file);

    const response = await apiClient.post<StockImportResult>(
      `${INVENTORY_ENDPOINTS.IMPORT_VALIDATE}?pointOfSaleId=${pointOfSaleId}`,
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
   * Import stock from Excel file
   */
  importStock: async (
    file: File,
    pointOfSaleId: string
  ): Promise<StockImportResult> => {
    const formData = new FormData();
    formData.append('file', file);

    const response = await apiClient.post<StockImportResult>(
      `${INVENTORY_ENDPOINTS.IMPORT}?pointOfSaleId=${pointOfSaleId}`,
      formData,
      {
        headers: {
          'Content-Type': 'multipart/form-data',
        },
      }
    );
    return response.data;
  },

  // ==================== Stock Adjustment Operations ====================

  /**
   * Adjust stock for a product at a point of sale
   */
  adjustStock: async (request: StockAdjustmentRequest): Promise<StockAdjustmentResult> => {
    const response = await apiClient.post<StockAdjustmentResult>(
      INVENTORY_ENDPOINTS.ADJUSTMENT,
      request
    );
    return response.data;
  },

  // ==================== Movement History Operations ====================

  /**
   * Get movement history with filters
   */
  getMovementHistory: async (
    filter: MovementHistoryFilter
  ): Promise<PaginatedMovementResult> => {
    const response = await apiClient.get<PaginatedMovementResult>(
      INVENTORY_ENDPOINTS.MOVEMENTS,
      {
        params: {
          productId: filter.productId,
          pointOfSaleId: filter.pointOfSaleId,
          startDate: filter.startDate,
          endDate: filter.endDate,
          page: filter.page ?? 1,
          pageSize: filter.pageSize ?? 50,
        },
      }
    );
    return response.data;
  },
};

export default inventoryService;

