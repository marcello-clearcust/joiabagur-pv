/**
 * Points of Sale Page Tests
 * Component tests for point of sale management
 */

import { describe, it, expect, beforeEach, vi } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { PointsOfSalePage } from './index';
import { pointOfSaleService } from '@/services/point-of-sale.service';

// Mock the point of sale service
vi.mock('@/services/point-of-sale.service', () => ({
  pointOfSaleService: {
    getPointsOfSale: vi.fn(),
    createPointOfSale: vi.fn(),
    updatePointOfSale: vi.fn(),
    changePointOfSaleStatus: vi.fn(),
  },
}));

// Mock toast
vi.mock('sonner', () => ({
  toast: {
    success: vi.fn(),
    error: vi.fn(),
  },
}));

describe('PointsOfSalePage', () => {
  const mockPointsOfSale = [
    {
      id: '1',
      name: 'Tienda Centro',
      code: 'CENTRO-001',
      address: 'Calle Principal 123, Madrid',
      phone: '+34 600 123 456',
      email: 'centro@test.com',
      isActive: true,
      createdAt: '2024-01-01T00:00:00Z',
      updatedAt: '2024-01-01T00:00:00Z',
    },
    {
      id: '2',
      name: 'Hotel Plaza',
      code: 'HOTEL-PLZ',
      address: 'Plaza Mayor 1, Barcelona',
      phone: '+34 600 789 012',
      email: 'plaza@test.com',
      isActive: true,
      createdAt: '2024-01-01T00:00:00Z',
      updatedAt: '2024-01-01T00:00:00Z',
    },
    {
      id: '3',
      name: 'Aeropuerto',
      code: 'AIRPORT-MAD',
      address: 'Terminal 4, Madrid',
      phone: null,
      email: null,
      isActive: false,
      createdAt: '2024-01-01T00:00:00Z',
      updatedAt: '2024-01-01T00:00:00Z',
    },
  ];

  beforeEach(() => {
    vi.clearAllMocks();
    vi.mocked(pointOfSaleService.getPointsOfSale).mockResolvedValue(mockPointsOfSale);
  });

  describe('Rendering', () => {
    it('should render points of sale page with title', async () => {
      render(<PointsOfSalePage />);

      expect(screen.getByText('Puntos de Venta')).toBeInTheDocument();
      expect(
        screen.getByText('Gestión de puntos de venta y ubicaciones del negocio')
      ).toBeInTheDocument();
    });

    it('should display create button', async () => {
      render(<PointsOfSalePage />);

      expect(screen.getByRole('button', { name: /nuevo punto de venta/i })).toBeInTheDocument();
    });

    it('should display info banner', async () => {
      render(<PointsOfSalePage />);

      expect(screen.getByText('Gestión de ubicaciones')).toBeInTheDocument();
      expect(screen.getByText(/Los puntos de venta representan/i)).toBeInTheDocument();
    });
  });

  describe('Statistics', () => {
    it('should display total count', async () => {
      render(<PointsOfSalePage />);

      await waitFor(() => {
        expect(screen.getByText('3')).toBeInTheDocument();
        expect(screen.getByText('Puntos de venta registrados')).toBeInTheDocument();
      });
    });

    it('should display active count', async () => {
      render(<PointsOfSalePage />);

      await waitFor(() => {
        expect(screen.getByText('2')).toBeInTheDocument();
        expect(screen.getByText('En operación')).toBeInTheDocument();
      });
    });

    it('should display inactive count', async () => {
      render(<PointsOfSalePage />);

      await waitFor(() => {
        expect(screen.getByText('1')).toBeInTheDocument();
        expect(screen.getByText('Fuera de servicio')).toBeInTheDocument();
      });
    });
  });

  describe('Data Loading', () => {
    it('should load and display points of sale', async () => {
      render(<PointsOfSalePage />);

      await waitFor(() => {
        expect(screen.getByText('Tienda Centro')).toBeInTheDocument();
        expect(screen.getByText('CENTRO-001')).toBeInTheDocument();
        expect(screen.getByText('Hotel Plaza')).toBeInTheDocument();
        expect(screen.getByText('HOTEL-PLZ')).toBeInTheDocument();
        expect(screen.getByText('Aeropuerto')).toBeInTheDocument();
      });
    });

    it('should display addresses when available', async () => {
      render(<PointsOfSalePage />);

      await waitFor(() => {
        expect(screen.getByText('Calle Principal 123, Madrid')).toBeInTheDocument();
        expect(screen.getByText('Plaza Mayor 1, Barcelona')).toBeInTheDocument();
      });
    });

    it('should display error message when loading fails', async () => {
      vi.mocked(pointOfSaleService.getPointsOfSale).mockRejectedValueOnce(
        new Error('Network error')
      );

      const { toast } = await import('sonner');

      render(<PointsOfSalePage />);

      await waitFor(() => {
        expect(toast.error).toHaveBeenCalledWith('Error al cargar los puntos de venta');
      });
    });
  });

  describe('Status Display', () => {
    it('should display active status with green indicator', async () => {
      render(<PointsOfSalePage />);

      await waitFor(() => {
        const activeStatuses = screen.getAllByText('Activo');
        expect(activeStatuses).toHaveLength(2);
      });
    });

    it('should display inactive status with gray indicator', async () => {
      render(<PointsOfSalePage />);

      await waitFor(() => {
        expect(screen.getByText('Inactivo')).toBeInTheDocument();
      });
    });
  });

  describe('Create Point of Sale', () => {
    it('should open create dialog when clicking new button', async () => {
      const user = userEvent.setup();

      render(<PointsOfSalePage />);

      await waitFor(() => {
        expect(screen.getByText('Tienda Centro')).toBeInTheDocument();
      });

      const createButton = screen.getByRole('button', { name: /nuevo punto de venta/i });
      await user.click(createButton);

      // Dialog should open
      await waitFor(() => {
        expect(screen.getByText(/crear punto de venta/i)).toBeInTheDocument();
      });
    });
  });

  describe('Edit Point of Sale', () => {
    it('should open edit dialog when clicking edit action', async () => {
      const user = userEvent.setup();

      render(<PointsOfSalePage />);

      await waitFor(() => {
        expect(screen.getByText('Tienda Centro')).toBeInTheDocument();
      });

      // Click on actions menu
      const actionButtons = screen.getAllByRole('button', { name: /abrir menú/i });
      await user.click(actionButtons[0]);

      // Click edit option
      const editButton = screen.getByRole('menuitem', { name: /editar/i });
      await user.click(editButton);

      // Dialog should open with edit mode
      await waitFor(() => {
        expect(screen.getByText(/editar punto de venta/i)).toBeInTheDocument();
      });
    });
  });

  describe('Operator Assignments', () => {
    it('should open operator assignments dialog when clicking manage operators', async () => {
      const user = userEvent.setup();

      render(<PointsOfSalePage />);

      await waitFor(() => {
        expect(screen.getByText('Tienda Centro')).toBeInTheDocument();
      });

      // Click on actions menu
      const actionButtons = screen.getAllByRole('button', { name: /abrir menú/i });
      await user.click(actionButtons[0]);

      // Click assign operators option
      const assignButton = screen.getByRole('menuitem', { name: /asignar operadores/i });
      await user.click(assignButton);

      // Dialog should open
      await waitFor(() => {
        expect(screen.getByText(/asignar operadores/i)).toBeInTheDocument();
      });
    });
  });

  describe('Payment Method Assignments', () => {
    it('should open payment methods dialog when clicking manage payment methods', async () => {
      const user = userEvent.setup();

      render(<PointsOfSalePage />);

      await waitFor(() => {
        expect(screen.getByText('Tienda Centro')).toBeInTheDocument();
      });

      // Click on actions menu
      const actionButtons = screen.getAllByRole('button', { name: /abrir menú/i });
      await user.click(actionButtons[0]);

      // Click payment methods option
      const paymentMethodsButton = screen.getByRole('menuitem', { name: /métodos de pago/i });
      await user.click(paymentMethodsButton);

      // Dialog should open
      await waitFor(() => {
        expect(screen.getByText(/asignar métodos de pago/i)).toBeInTheDocument();
      });
    });
  });

  describe('Toggle Status', () => {
    it('should deactivate an active point of sale', async () => {
      const user = userEvent.setup();
      const updatedPointOfSale = { ...mockPointsOfSale[0], isActive: false };

      vi.mocked(pointOfSaleService.changePointOfSaleStatus).mockResolvedValueOnce(
        updatedPointOfSale
      );
      vi.mocked(pointOfSaleService.getPointsOfSale).mockResolvedValueOnce(mockPointsOfSale);

      const { toast } = await import('sonner');

      render(<PointsOfSalePage />);

      await waitFor(() => {
        expect(screen.getByText('Tienda Centro')).toBeInTheDocument();
      });

      // Open actions menu
      const actionButtons = screen.getAllByRole('button', { name: /abrir menú/i });
      await user.click(actionButtons[0]);

      // Click deactivate option
      const deactivateButton = screen.getByRole('menuitem', { name: /desactivar/i });
      await user.click(deactivateButton);

      await waitFor(() => {
        expect(pointOfSaleService.changePointOfSaleStatus).toHaveBeenCalledWith('1', false);
        expect(toast.success).toHaveBeenCalledWith('Punto de venta desactivado correctamente');
      });
    });

    it('should show error message when status toggle fails', async () => {
      const user = userEvent.setup();

      vi.mocked(pointOfSaleService.changePointOfSaleStatus).mockRejectedValueOnce({
        message: 'No se puede desactivar punto de venta con operadores asignados activos',
        statusCode: 400,
      });

      const { toast } = await import('sonner');

      render(<PointsOfSalePage />);

      await waitFor(() => {
        expect(screen.getByText('Tienda Centro')).toBeInTheDocument();
      });

      // Open actions menu and click deactivate
      const actionButtons = screen.getAllByRole('button', { name: /abrir menú/i });
      await user.click(actionButtons[0]);

      const deactivateButton = screen.getByRole('menuitem', { name: /desactivar/i });
      await user.click(deactivateButton);

      await waitFor(() => {
        expect(toast.error).toHaveBeenCalledWith(
          'No se puede desactivar punto de venta con operadores asignados activos'
        );
      });
    });
  });
});
