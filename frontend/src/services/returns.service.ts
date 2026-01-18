/**
 * Returns Service (EP5)
 * Handles return registration and history operations.
 */

import apiClient from './api.service';
import type {
  CreateReturnRequest,
  CreateReturnResponse,
  Return,
  ReturnsHistoryFilterRequest,
  ReturnsHistoryResponse,
  EligibleSalesResponse,
} from '@/types/returns.types';

const RETURNS_ENDPOINT = '/returns';

/**
 * Returns API operations.
 */
export const returnsService = {
  /**
   * Creates a new return.
   */
  createReturn: async (request: CreateReturnRequest): Promise<CreateReturnResponse> => {
    const response = await apiClient.post<CreateReturnResponse>(RETURNS_ENDPOINT, request);
    return response.data;
  },

  /**
   * Gets a return by ID.
   */
  getReturnById: async (id: string): Promise<Return> => {
    const response = await apiClient.get<Return>(`${RETURNS_ENDPOINT}/${id}`);
    return response.data;
  },

  /**
   * Gets returns history with filters.
   */
  getReturnsHistory: async (filters: ReturnsHistoryFilterRequest): Promise<ReturnsHistoryResponse> => {
    const params = new URLSearchParams();
    
    if (filters.startDate) params.append('startDate', filters.startDate);
    if (filters.endDate) params.append('endDate', filters.endDate);
    if (filters.pointOfSaleId) params.append('pointOfSaleId', filters.pointOfSaleId);
    if (filters.productId) params.append('productId', filters.productId);
    if (filters.page) params.append('page', filters.page.toString());
    if (filters.pageSize) params.append('pageSize', filters.pageSize.toString());

    const response = await apiClient.get<ReturnsHistoryResponse>(`${RETURNS_ENDPOINT}?${params.toString()}`);
    return response.data;
  },

  /**
   * Gets eligible sales for return.
   */
  getEligibleSales: async (productId: string, pointOfSaleId: string): Promise<EligibleSalesResponse> => {
    const params = new URLSearchParams();
    params.append('productId', productId);
    params.append('pointOfSaleId', pointOfSaleId);

    const response = await apiClient.get<EligibleSalesResponse>(`${RETURNS_ENDPOINT}/eligible-sales?${params.toString()}`);
    return response.data;
  },

  /**
   * Gets the photo URL for a return.
   */
  getReturnPhotoUrl: (returnId: string): string => {
    return `${apiClient.defaults.baseURL}${RETURNS_ENDPOINT}/${returnId}/photo/file`;
  },
};

export default returnsService;
