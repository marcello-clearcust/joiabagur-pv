/**
 * Point of Sale Service Tests
 * Unit tests for point of sale API service
 */

import { describe, it, expect, beforeEach, vi } from 'vitest';
import { pointOfSaleService } from './point-of-sale.service';
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

describe('PointOfSaleService', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  describe('getPointsOfSale', () => {
    it('should fetch all points of sale', async () => {
      const mockPointsOfSale = [
        {
          id: '1',
          name: 'Tienda Centro',
          code: 'CENTRO-001',
          address: 'Calle Principal 123',
          phone: '+34 600 123 456',
          email: 'centro@test.com',
          isActive: true,
          createdAt: '2024-01-01T00:00:00Z',
          updatedAt: '2024-01-01T00:00:00Z',
        },
      ];

      vi.mocked(apiClient.get).mockResolvedValueOnce({ data: mockPointsOfSale });

      const result = await pointOfSaleService.getPointsOfSale();

      expect(apiClient.get).toHaveBeenCalledWith('/point-of-sales');
      expect(result).toEqual(mockPointsOfSale);
    });
  });

  describe('getPointOfSale', () => {
    it('should fetch point of sale by ID', async () => {
      const mockPointOfSale = {
        id: '123',
        name: 'Tienda Centro',
        code: 'CENTRO-001',
        address: 'Calle Principal 123',
        isActive: true,
        createdAt: '2024-01-01T00:00:00Z',
        updatedAt: '2024-01-01T00:00:00Z',
      };

      vi.mocked(apiClient.get).mockResolvedValueOnce({ data: mockPointOfSale });

      const result = await pointOfSaleService.getPointOfSale('123');

      expect(apiClient.get).toHaveBeenCalledWith('/point-of-sales/123');
      expect(result).toEqual(mockPointOfSale);
    });
  });

  describe('createPointOfSale', () => {
    it('should create a new point of sale', async () => {
      const request = {
        name: 'Nueva Tienda',
        code: 'NUEVA-001',
        address: 'Avenida Test 456',
        phone: '+34 600 999 888',
        email: 'nueva@test.com',
      };

      const mockResponse = {
        id: '456',
        ...request,
        isActive: true,
        createdAt: '2024-01-01T00:00:00Z',
        updatedAt: '2024-01-01T00:00:00Z',
      };

      vi.mocked(apiClient.post).mockResolvedValueOnce({ data: mockResponse });

      const result = await pointOfSaleService.createPointOfSale(request);

      expect(apiClient.post).toHaveBeenCalledWith('/point-of-sales', request);
      expect(result).toEqual(mockResponse);
    });
  });

  describe('updatePointOfSale', () => {
    it('should update an existing point of sale', async () => {
      const request = {
        name: 'Tienda Centro Actualizada',
        code: 'CENTRO-001',
        address: 'Nueva Dirección 789',
      };

      const mockResponse = {
        id: '123',
        ...request,
        isActive: true,
        createdAt: '2024-01-01T00:00:00Z',
        updatedAt: '2024-01-02T00:00:00Z',
      };

      vi.mocked(apiClient.put).mockResolvedValueOnce({ data: mockResponse });

      const result = await pointOfSaleService.updatePointOfSale('123', request);

      expect(apiClient.put).toHaveBeenCalledWith('/point-of-sales/123', request);
      expect(result).toEqual(mockResponse);
    });
  });

  describe('changePointOfSaleStatus', () => {
    it('should activate a point of sale', async () => {
      const mockResponse = {
        id: '123',
        name: 'Tienda Centro',
        code: 'CENTRO-001',
        isActive: true,
        createdAt: '2024-01-01T00:00:00Z',
        updatedAt: '2024-01-02T00:00:00Z',
      };

      vi.mocked(apiClient.patch).mockResolvedValueOnce({ data: mockResponse });

      const result = await pointOfSaleService.changePointOfSaleStatus('123', true);

      expect(apiClient.patch).toHaveBeenCalledWith('/point-of-sales/123/status', {
        isActive: true,
      });
      expect(result.isActive).toBe(true);
    });

    it('should deactivate a point of sale', async () => {
      const mockResponse = {
        id: '123',
        name: 'Tienda Centro',
        code: 'CENTRO-001',
        isActive: false,
        createdAt: '2024-01-01T00:00:00Z',
        updatedAt: '2024-01-02T00:00:00Z',
      };

      vi.mocked(apiClient.patch).mockResolvedValueOnce({ data: mockResponse });

      const result = await pointOfSaleService.changePointOfSaleStatus('123', false);

      expect(result.isActive).toBe(false);
    });
  });

  describe('Operator Assignment Operations', () => {
    it('should get operators for a point of sale', async () => {
      const mockAssignments = [
        {
          id: '1',
          userId: 'user-1',
          pointOfSaleId: 'pos-1',
          isActive: true,
          assignedAt: '2024-01-01T00:00:00Z',
        },
      ];

      vi.mocked(apiClient.get).mockResolvedValueOnce({ data: mockAssignments });

      const result = await pointOfSaleService.getPointOfSaleOperators('pos-1');

      expect(apiClient.get).toHaveBeenCalledWith('/point-of-sales/pos-1/operators');
      expect(result).toEqual(mockAssignments);
    });

    it('should assign operator to point of sale', async () => {
      const mockAssignment = {
        id: '1',
        userId: 'user-1',
        pointOfSaleId: 'pos-1',
        isActive: true,
        assignedAt: '2024-01-01T00:00:00Z',
      };

      vi.mocked(apiClient.post).mockResolvedValueOnce({ data: mockAssignment });

      const result = await pointOfSaleService.assignOperatorToPointOfSale('pos-1', 'user-1');

      expect(apiClient.post).toHaveBeenCalledWith('/point-of-sales/pos-1/operators/user-1');
      expect(result).toEqual(mockAssignment);
    });

    it('should unassign operator from point of sale', async () => {
      vi.mocked(apiClient.delete).mockResolvedValueOnce({ data: {} });

      await pointOfSaleService.unassignOperatorFromPointOfSale('pos-1', 'user-1');

      expect(apiClient.delete).toHaveBeenCalledWith('/point-of-sales/pos-1/operators/user-1');
    });
  });

  describe('Payment Method Assignment Operations', () => {
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

      const result = await pointOfSaleService.getPointOfSalePaymentMethods('pos-1');

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
        isActive: true,
        createdAt: '2024-01-01T00:00:00Z',
      };

      vi.mocked(apiClient.post).mockResolvedValueOnce({ data: mockAssignment });

      const result = await pointOfSaleService.assignPaymentMethodToPointOfSale('pos-1', 'pm-1');

      expect(apiClient.post).toHaveBeenCalledWith('/point-of-sales/pos-1/payment-methods/pm-1');
      expect(result).toEqual(mockAssignment);
    });

    it('should unassign payment method from point of sale', async () => {
      vi.mocked(apiClient.delete).mockResolvedValueOnce({ data: {} });

      await pointOfSaleService.unassignPaymentMethodFromPointOfSale('pos-1', 'pm-1');

      expect(apiClient.delete).toHaveBeenCalledWith('/point-of-sales/pos-1/payment-methods/pm-1');
    });

    it('should change payment method assignment status', async () => {
      const mockAssignment = {
        id: '1',
        pointOfSaleId: 'pos-1',
        paymentMethodId: 'pm-1',
        isActive: false,
        createdAt: '2024-01-01T00:00:00Z',
        deactivatedAt: '2024-01-02T00:00:00Z',
      };

      vi.mocked(apiClient.patch).mockResolvedValueOnce({ data: mockAssignment });

      const result = await pointOfSaleService.changePaymentMethodAssignmentStatus(
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
