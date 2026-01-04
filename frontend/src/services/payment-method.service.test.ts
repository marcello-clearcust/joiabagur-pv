/**
 * Payment Method Service Tests
 * Unit tests for payment method API service
 */

import { describe, it, expect, beforeEach, vi } from 'vitest';
import { paymentMethodService } from './payment-method.service';
import { apiClient } from './api.service';

// Mock the API client
vi.mock('./api.service', () => ({
  apiClient: {
    get: vi.fn(),
    post: vi.fn(),
    put: vi.fn(),
    patch: vi.fn(),
    delete: vi.fn(),
  },
}));

describe('PaymentMethodService', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  describe('getPaymentMethods', () => {
    it('should fetch all active payment methods by default', async () => {
      const mockPaymentMethods = [
        {
          id: '1',
          code: 'CASH',
          name: 'Efectivo',
          description: 'Pago en efectivo',
          isActive: true,
          createdAt: '2024-01-01T00:00:00Z',
          updatedAt: '2024-01-01T00:00:00Z',
        },
      ];

      vi.mocked(apiClient.get).mockResolvedValueOnce({ data: mockPaymentMethods });

      const result = await paymentMethodService.getPaymentMethods();

      expect(apiClient.get).toHaveBeenCalledWith('/payment-methods?includeInactive=false');
      expect(result).toEqual(mockPaymentMethods);
    });

    it('should include inactive payment methods when requested', async () => {
      vi.mocked(apiClient.get).mockResolvedValueOnce({ data: [] });

      await paymentMethodService.getPaymentMethods(true);

      expect(apiClient.get).toHaveBeenCalledWith('/api/payment-methods?includeInactive=true');
    });
  });

  describe('getPaymentMethod', () => {
    it('should fetch payment method by ID', async () => {
      const mockPaymentMethod = {
        id: '123',
        code: 'CASH',
        name: 'Efectivo',
        description: 'Pago en efectivo',
        isActive: true,
        createdAt: '2024-01-01T00:00:00Z',
        updatedAt: '2024-01-01T00:00:00Z',
      };

      vi.mocked(apiClient.get).mockResolvedValueOnce({ data: mockPaymentMethod });

      const result = await paymentMethodService.getPaymentMethod('123');

      expect(apiClient.get).toHaveBeenCalledWith('/payment-methods/123');
      expect(result).toEqual(mockPaymentMethod);
    });
  });

  describe('createPaymentMethod', () => {
    it('should create a new payment method', async () => {
      const request = {
        code: 'NEW_METHOD',
        name: 'New Payment',
        description: 'New payment method',
      };

      const mockResponse = {
        id: '456',
        ...request,
        isActive: true,
        createdAt: '2024-01-01T00:00:00Z',
        updatedAt: '2024-01-01T00:00:00Z',
      };

      vi.mocked(apiClient.post).mockResolvedValueOnce({ data: mockResponse });

      const result = await paymentMethodService.createPaymentMethod(request);

      expect(apiClient.post).toHaveBeenCalledWith('/payment-methods', request);
      expect(result).toEqual(mockResponse);
    });
  });

  describe('updatePaymentMethod', () => {
    it('should update an existing payment method', async () => {
      const request = {
        name: 'Updated Name',
        description: 'Updated description',
      };

      const mockResponse = {
        id: '123',
        code: 'CASH',
        ...request,
        isActive: true,
        createdAt: '2024-01-01T00:00:00Z',
        updatedAt: '2024-01-02T00:00:00Z',
      };

      vi.mocked(apiClient.put).mockResolvedValueOnce({ data: mockResponse });

      const result = await paymentMethodService.updatePaymentMethod('123', request);

      expect(apiClient.put).toHaveBeenCalledWith('/payment-methods/123', request);
      expect(result).toEqual(mockResponse);
    });
  });

  describe('changePaymentMethodStatus', () => {
    it('should activate a payment method', async () => {
      const mockResponse = {
        id: '123',
        code: 'CASH',
        name: 'Efectivo',
        description: 'Pago en efectivo',
        isActive: true,
        createdAt: '2024-01-01T00:00:00Z',
        updatedAt: '2024-01-02T00:00:00Z',
      };

      vi.mocked(apiClient.patch).mockResolvedValueOnce({ data: mockResponse });

      const result = await paymentMethodService.changePaymentMethodStatus('123', true);

      expect(apiClient.patch).toHaveBeenCalledWith('/payment-methods/123/status', {
        isActive: true,
      });
      expect(result).toEqual(mockResponse);
    });

    it('should deactivate a payment method', async () => {
      const mockResponse = {
        id: '123',
        code: 'CASH',
        name: 'Efectivo',
        description: 'Pago en efectivo',
        isActive: false,
        createdAt: '2024-01-01T00:00:00Z',
        updatedAt: '2024-01-02T00:00:00Z',
      };

      vi.mocked(apiClient.patch).mockResolvedValueOnce({ data: mockResponse });

      const result = await paymentMethodService.changePaymentMethodStatus('123', false);

      expect(apiClient.patch).toHaveBeenCalledWith('/payment-methods/123/status', {
        isActive: false,
      });
      expect(result.isActive).toBe(false);
    });
  });

  describe('Point of Sale Assignment Operations', () => {
    it('should get payment methods for a point of sale', async () => {
      const mockAssignments = [
        {
          id: '1',
          pointOfSaleId: 'pos-1',
          paymentMethodId: 'pm-1',
          paymentMethod: {
            id: 'pm-1',
            code: 'CASH',
            name: 'Efectivo',
            description: '',
            isActive: true,
            createdAt: '2024-01-01T00:00:00Z',
            updatedAt: '2024-01-01T00:00:00Z',
          },
          isActive: true,
          createdAt: '2024-01-01T00:00:00Z',
        },
      ];

      vi.mocked(apiClient.get).mockResolvedValueOnce({ data: mockAssignments });

      const result = await paymentMethodService.getPointOfSalePaymentMethods('pos-1');

      expect(apiClient.get).toHaveBeenCalledWith(
        '/point-of-sales/pos-1/payment-methods?includeInactive=false'
      );
      expect(result).toEqual(mockAssignments);
    });

    it('should assign payment method to point of sale', async () => {
      const mockAssignment = {
        id: '1',
        pointOfSaleId: 'pos-1',
        paymentMethodId: 'pm-1',
        paymentMethod: {
          id: 'pm-1',
          code: 'CASH',
          name: 'Efectivo',
          description: '',
          isActive: true,
          createdAt: '2024-01-01T00:00:00Z',
          updatedAt: '2024-01-01T00:00:00Z',
        },
        isActive: true,
        createdAt: '2024-01-01T00:00:00Z',
      };

      vi.mocked(apiClient.post).mockResolvedValueOnce({ data: mockAssignment });

      const result = await paymentMethodService.assignPaymentMethodToPointOfSale('pos-1', 'pm-1');

      expect(apiClient.post).toHaveBeenCalledWith('/point-of-sales/pos-1/payment-methods/pm-1');
      expect(result).toEqual(mockAssignment);
    });

    it('should unassign payment method from point of sale', async () => {
      vi.mocked(apiClient.delete).mockResolvedValueOnce({ data: {} });

      await paymentMethodService.unassignPaymentMethodFromPointOfSale('pos-1', 'pm-1');

      expect(apiClient.delete).toHaveBeenCalledWith('/point-of-sales/pos-1/payment-methods/pm-1');
    });

    it('should change payment method assignment status', async () => {
      const mockAssignment = {
        id: '1',
        pointOfSaleId: 'pos-1',
        paymentMethodId: 'pm-1',
        paymentMethod: {
          id: 'pm-1',
          code: 'CASH',
          name: 'Efectivo',
          description: '',
          isActive: true,
          createdAt: '2024-01-01T00:00:00Z',
          updatedAt: '2024-01-01T00:00:00Z',
        },
        isActive: false,
        createdAt: '2024-01-01T00:00:00Z',
        deactivatedAt: '2024-01-02T00:00:00Z',
      };

      vi.mocked(apiClient.patch).mockResolvedValueOnce({ data: mockAssignment });

      const result = await paymentMethodService.changePaymentMethodAssignmentStatus(
        'pos-1',
        'pm-1',
        false
      );

      expect(apiClient.patch).toHaveBeenCalledWith(
        '/point-of-sales/pos-1/payment-methods/pm-1/status',
        { isActive: false }
      );
      expect(result).toEqual(mockAssignment);
    });
  });
});
