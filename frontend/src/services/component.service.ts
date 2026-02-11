/**
 * Component Management Service - EP10
 * API service for product component operations
 */

import apiClient from './api.service';
import type {
  ProductComponent,
  PaginatedComponentResult,
  CreateComponentRequest,
  UpdateComponentRequest,
  ComponentQueryParams,
  ComponentAssignment,
  SaveComponentAssignmentsRequest,
  PriceSyncPreview,
  ComponentTemplate,
  SaveComponentTemplateRequest,
  ApplyTemplateRequest,
  ApplyTemplateResult,
  MarginReport,
  MarginReportQueryParams,
  ProductWithoutComponents,
  ProductsWithoutComponentsQueryParams,
} from '@/types/component.types';

const COMPONENT_ENDPOINTS = {
  COMPONENTS: '/product-components',
  COMPONENT_BY_ID: (id: string) => `/product-components/${id}`,
  SEARCH: '/product-components/search',
  PRODUCT_COMPONENTS: (productId: string) => `/products/${productId}/components`,
  SYNC_PREVIEW: (productId: string) => `/products/${productId}/components/sync-preview`,
  SYNC_FROM_MASTER: (productId: string) => `/products/${productId}/components/sync-from-master`,
  APPLY_TEMPLATE: (productId: string) => `/products/${productId}/components/apply-template`,
  TEMPLATES: '/component-templates',
  TEMPLATE_BY_ID: (id: string) => `/component-templates/${id}`,
  MARGIN_REPORT: '/reports/product-margins',
  MARGIN_REPORT_EXPORT: '/reports/product-margins/export',
  PRODUCTS_WITHOUT_COMPONENTS: '/reports/products-without-components',
} as const;

export const componentService = {
  // ─── Master Table ──────────────────────────────────────────────────

  getComponents: async (params: ComponentQueryParams = {}): Promise<PaginatedComponentResult> => {
    const response = await apiClient.get<PaginatedComponentResult>(
      COMPONENT_ENDPOINTS.COMPONENTS,
      { params }
    );
    return response.data;
  },

  getComponentById: async (id: string): Promise<ProductComponent> => {
    const response = await apiClient.get<ProductComponent>(
      COMPONENT_ENDPOINTS.COMPONENT_BY_ID(id)
    );
    return response.data;
  },

  createComponent: async (data: CreateComponentRequest): Promise<ProductComponent> => {
    const response = await apiClient.post<ProductComponent>(
      COMPONENT_ENDPOINTS.COMPONENTS,
      data
    );
    return response.data;
  },

  updateComponent: async (id: string, data: UpdateComponentRequest): Promise<ProductComponent> => {
    const response = await apiClient.put<ProductComponent>(
      COMPONENT_ENDPOINTS.COMPONENT_BY_ID(id),
      data
    );
    return response.data;
  },

  searchComponents: async (query: string): Promise<ProductComponent[]> => {
    const response = await apiClient.get<ProductComponent[]>(
      COMPONENT_ENDPOINTS.SEARCH,
      { params: { query } }
    );
    return response.data;
  },

  // ─── Component Assignments ─────────────────────────────────────────

  getProductComponents: async (productId: string): Promise<ComponentAssignment[]> => {
    const response = await apiClient.get<ComponentAssignment[]>(
      COMPONENT_ENDPOINTS.PRODUCT_COMPONENTS(productId)
    );
    return response.data;
  },

  saveProductComponents: async (
    productId: string,
    data: SaveComponentAssignmentsRequest
  ): Promise<ComponentAssignment[]> => {
    const response = await apiClient.put<ComponentAssignment[]>(
      COMPONENT_ENDPOINTS.PRODUCT_COMPONENTS(productId),
      data
    );
    return response.data;
  },

  getSyncPreview: async (productId: string): Promise<PriceSyncPreview> => {
    const response = await apiClient.get<PriceSyncPreview>(
      COMPONENT_ENDPOINTS.SYNC_PREVIEW(productId)
    );
    return response.data;
  },

  syncFromMaster: async (productId: string): Promise<ComponentAssignment[]> => {
    const response = await apiClient.post<ComponentAssignment[]>(
      COMPONENT_ENDPOINTS.SYNC_FROM_MASTER(productId)
    );
    return response.data;
  },

  applyTemplate: async (
    productId: string,
    data: ApplyTemplateRequest
  ): Promise<ApplyTemplateResult> => {
    const response = await apiClient.post<ApplyTemplateResult>(
      COMPONENT_ENDPOINTS.APPLY_TEMPLATE(productId),
      data
    );
    return response.data;
  },

  // ─── Templates ─────────────────────────────────────────────────────

  getTemplates: async (): Promise<ComponentTemplate[]> => {
    const response = await apiClient.get<ComponentTemplate[]>(
      COMPONENT_ENDPOINTS.TEMPLATES
    );
    return response.data;
  },

  getTemplateById: async (id: string): Promise<ComponentTemplate> => {
    const response = await apiClient.get<ComponentTemplate>(
      COMPONENT_ENDPOINTS.TEMPLATE_BY_ID(id)
    );
    return response.data;
  },

  createTemplate: async (data: SaveComponentTemplateRequest): Promise<ComponentTemplate> => {
    const response = await apiClient.post<ComponentTemplate>(
      COMPONENT_ENDPOINTS.TEMPLATES,
      data
    );
    return response.data;
  },

  updateTemplate: async (
    id: string,
    data: SaveComponentTemplateRequest
  ): Promise<ComponentTemplate> => {
    const response = await apiClient.put<ComponentTemplate>(
      COMPONENT_ENDPOINTS.TEMPLATE_BY_ID(id),
      data
    );
    return response.data;
  },

  deleteTemplate: async (id: string): Promise<void> => {
    await apiClient.delete(COMPONENT_ENDPOINTS.TEMPLATE_BY_ID(id));
  },

  // ─── Reports ───────────────────────────────────────────────────────

  getMarginReport: async (params: MarginReportQueryParams = {}): Promise<MarginReport> => {
    const response = await apiClient.get<MarginReport>(
      COMPONENT_ENDPOINTS.MARGIN_REPORT,
      { params }
    );
    return response.data;
  },

  exportMarginReport: async (params: MarginReportQueryParams = {}): Promise<Blob> => {
    const response = await apiClient.get(
      COMPONENT_ENDPOINTS.MARGIN_REPORT_EXPORT,
      {
        params,
        responseType: 'blob',
      }
    );
    return response.data as Blob;
  },

  getProductsWithoutComponents: async (
    params: ProductsWithoutComponentsQueryParams = {}
  ): Promise<{
    items: ProductWithoutComponents[];
    totalCount: number;
    totalPages: number;
    currentPage: number;
    pageSize: number;
  }> => {
    const response = await apiClient.get(
      COMPONENT_ENDPOINTS.PRODUCTS_WITHOUT_COMPONENTS,
      { params }
    );
    return response.data;
  },
};
