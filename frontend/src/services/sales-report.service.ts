import apiClient from './api.service';
import { toast } from 'sonner';
import type {
  SalesReportFilterRequest,
  SalesReportResponse,
} from '@/types/sales-report.types';

const ENDPOINTS = {
  REPORT: '/reports/sales',
  EXPORT: '/reports/sales/export',
} as const;

function buildParams(filters: SalesReportFilterRequest): Record<string, unknown> {
  const params: Record<string, unknown> = {};
  if (filters.startDate) params.startDate = filters.startDate;
  if (filters.endDate) params.endDate = filters.endDate;
  if (filters.pointOfSaleId) params.pointOfSaleId = filters.pointOfSaleId;
  if (filters.productId) params.productId = filters.productId;
  if (filters.userId) params.userId = filters.userId;
  if (filters.paymentMethodId) params.paymentMethodId = filters.paymentMethodId;
  if (filters.search) params.search = filters.search;
  if (filters.amountMin != null) params.amountMin = filters.amountMin;
  if (filters.amountMax != null) params.amountMax = filters.amountMax;
  if (filters.hasPhoto != null) params.hasPhoto = filters.hasPhoto;
  if (filters.priceWasOverridden != null) params.priceWasOverridden = filters.priceWasOverridden;
  if (filters.page) params.page = filters.page;
  if (filters.pageSize) params.pageSize = filters.pageSize;
  return params;
}

export const salesReportService = {
  getSalesReport: async (filters: SalesReportFilterRequest): Promise<SalesReportResponse> => {
    const response = await apiClient.get<SalesReportResponse>(
      ENDPOINTS.REPORT,
      { params: buildParams(filters) }
    );
    return response.data;
  },

  exportSalesReport: async (filters: SalesReportFilterRequest): Promise<Blob> => {
    try {
      const response = await apiClient.get(ENDPOINTS.EXPORT, {
        params: buildParams(filters),
        responseType: 'blob',
      });
      return response.data as Blob;
    } catch (error: unknown) {
      const err = error as { statusCode?: number; response?: { status?: number; data?: Blob } };
      if (err.response?.status === 409 || err.statusCode === 409) {
        const blob = err.response?.data;
        if (blob) {
          const text = await blob.text();
          try {
            const json = JSON.parse(text);
            toast.warning(`Hay ${json.totalCount?.toLocaleString('es-ES')} ventas. Ajuste los filtros.`);
          } catch {
            toast.warning('Hay más de 10.000 ventas. Ajuste los filtros para exportar.');
          }
        }
        throw error;
      }
      throw error;
    }
  },
};
