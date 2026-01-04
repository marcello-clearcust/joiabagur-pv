/**
 * Payment Method Service - EP6
 * API service for payment method management operations
 */

import { apiClient } from './api.service';
import type {
  PaymentMethod,
  CreatePaymentMethodRequest,
  UpdatePaymentMethodRequest,
  PaymentMethodStatusRequest,
  PointOfSalePaymentMethod,
} from '@/types/payment-method.types';

/**
 * Get all payment methods
 */
export async function getPaymentMethods(
  includeInactive = false
): Promise<PaymentMethod[]> {
  const response = await apiClient.get<PaymentMethod[]>(
    `/payment-methods?includeInactive=${includeInactive}`
  );
  return response.data;
}

/**
 * Get payment method by ID
 */
export async function getPaymentMethod(id: string): Promise<PaymentMethod> {
  const response = await apiClient.get<PaymentMethod>(`/payment-methods/${id}`);
  return response.data;
}

/**
 * Create a new payment method
 */
export async function createPaymentMethod(
  request: CreatePaymentMethodRequest
): Promise<PaymentMethod> {
  const response = await apiClient.post<PaymentMethod>(
    '/payment-methods',
    request
  );
  return response.data;
}

/**
 * Update an existing payment method
 */
export async function updatePaymentMethod(
  id: string,
  request: UpdatePaymentMethodRequest
): Promise<PaymentMethod> {
  const response = await apiClient.put<PaymentMethod>(
    `/payment-methods/${id}`,
    request
  );
  return response.data;
}

/**
 * Change payment method status (activate/deactivate)
 */
export async function changePaymentMethodStatus(
  id: string,
  isActive: boolean
): Promise<PaymentMethod> {
  const response = await apiClient.patch<PaymentMethod>(
    `/payment-methods/${id}/status`,
    { isActive }
  );
  return response.data;
}

/**
 * Get payment methods assigned to a point of sale
 */
export async function getPointOfSalePaymentMethods(
  pointOfSaleId: string,
  includeInactive = false
): Promise<PointOfSalePaymentMethod[]> {
  const response = await apiClient.get<PointOfSalePaymentMethod[]>(
    `/point-of-sales/${pointOfSaleId}/payment-methods?includeInactive=${includeInactive}`
  );
  return response.data;
}

/**
 * Assign payment method to point of sale
 */
export async function assignPaymentMethodToPointOfSale(
  pointOfSaleId: string,
  paymentMethodId: string
): Promise<PointOfSalePaymentMethod> {
  const response = await apiClient.post<PointOfSalePaymentMethod>(
    `/point-of-sales/${pointOfSaleId}/payment-methods/${paymentMethodId}`
  );
  return response.data;
}

/**
 * Unassign payment method from point of sale
 */
export async function unassignPaymentMethodFromPointOfSale(
  pointOfSaleId: string,
  paymentMethodId: string
): Promise<void> {
  await apiClient.delete(
    `/point-of-sales/${pointOfSaleId}/payment-methods/${paymentMethodId}`
  );
}

/**
 * Change payment method assignment status
 */
export async function changePaymentMethodAssignmentStatus(
  pointOfSaleId: string,
  paymentMethodId: string,
  isActive: boolean
): Promise<PointOfSalePaymentMethod> {
  const response = await apiClient.patch<PointOfSalePaymentMethod>(
    `/point-of-sales/${pointOfSaleId}/payment-methods/${paymentMethodId}/status`,
    { isActive }
  );
  return response.data;
}

export const paymentMethodService = {
  getPaymentMethods,
  getPaymentMethod,
  createPaymentMethod,
  updatePaymentMethod,
  changePaymentMethodStatus,
  getPointOfSalePaymentMethods,
  assignPaymentMethodToPointOfSale,
  unassignPaymentMethodFromPointOfSale,
  changePaymentMethodAssignmentStatus,
};
