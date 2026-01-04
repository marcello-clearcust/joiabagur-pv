/**
 * Payment Method Types - EP6
 * Type definitions for payment method management
 */

export interface PaymentMethod {
  id: string;
  code: string;
  name: string;
  description: string;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface CreatePaymentMethodRequest {
  code: string;
  name: string;
  description: string;
}

export interface UpdatePaymentMethodRequest {
  name: string;
  description: string;
}

export interface PaymentMethodStatusRequest {
  isActive: boolean;
}

export interface PointOfSalePaymentMethod {
  id: string;
  pointOfSaleId: string;
  paymentMethodId: string;
  paymentMethod: PaymentMethod;
  isActive: boolean;
  createdAt: string;
  deactivatedAt?: string;
}
