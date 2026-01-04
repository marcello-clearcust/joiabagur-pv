/**
 * Payment Methods Page Tests
 * Component tests for payment method management
 */

import { describe, it, expect, beforeEach, vi } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { PaymentMethodsPage } from './index';
import { paymentMethodService } from '@/services/payment-method.service';

// Mock the payment method service
vi.mock('@/services/payment-method.service', () => ({
  paymentMethodService: {
    getPaymentMethods: vi.fn(),
    createPaymentMethod: vi.fn(),
    updatePaymentMethod: vi.fn(),
    changePaymentMethodStatus: vi.fn(),
  },
}));

// Mock toast
vi.mock('sonner', () => ({
  toast: {
    success: vi.fn(),
    error: vi.fn(),
  },
}));

describe('PaymentMethodsPage', () => {
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
    {
      id: '2',
      code: 'BIZUM',
      name: 'Bizum',
      description: 'Pago mediante Bizum',
      isActive: true,
      createdAt: '2024-01-01T00:00:00Z',
      updatedAt: '2024-01-01T00:00:00Z',
    },
    {
      id: '3',
      code: 'CARD_POS',
      name: 'Tarjeta TPV punto de venta',
      description: 'Pago con tarjeta en TPV del punto de venta',
      isActive: false,
      createdAt: '2024-01-01T00:00:00Z',
      updatedAt: '2024-01-01T00:00:00Z',
    },
  ];

  beforeEach(() => {
    vi.clearAllMocks();
    vi.mocked(paymentMethodService.getPaymentMethods).mockResolvedValue(
      mockPaymentMethods.filter((pm) => pm.isActive)
    );
  });

  describe('Rendering', () => {
    it('should render payment methods page with title', async () => {
      render(<PaymentMethodsPage />);

      expect(screen.getByText('Métodos de Pago')).toBeInTheDocument();
      expect(
        screen.getByText('Configuración de métodos de pago disponibles en el sistema')
      ).toBeInTheDocument();
    });

    it('should display create button', async () => {
      render(<PaymentMethodsPage />);

      expect(screen.getByRole('button', { name: /nuevo método de pago/i })).toBeInTheDocument();
    });

    it('should display show inactive toggle', async () => {
      render(<PaymentMethodsPage />);

      expect(screen.getByText('Mostrar inactivos')).toBeInTheDocument();
      expect(screen.getByRole('switch', { name: /show-inactive/i })).toBeInTheDocument();
    });
  });

  describe('Data Loading', () => {
    it('should load and display active payment methods', async () => {
      render(<PaymentMethodsPage />);

      await waitFor(() => {
        expect(screen.getByText('CASH')).toBeInTheDocument();
        expect(screen.getByText('Efectivo')).toBeInTheDocument();
        expect(screen.getByText('BIZUM')).toBeInTheDocument();
      });

      // Inactive payment method should not be shown by default
      expect(screen.queryByText('CARD_POS')).not.toBeInTheDocument();
    });

    it('should show inactive payment methods when toggle is enabled', async () => {
      const user = userEvent.setup();

      // Setup mock to return all payment methods when includeInactive is true
      vi.mocked(paymentMethodService.getPaymentMethods)
        .mockResolvedValueOnce(mockPaymentMethods.filter((pm) => pm.isActive))
        .mockResolvedValueOnce(mockPaymentMethods);

      render(<PaymentMethodsPage />);

      // Wait for initial load
      await waitFor(() => {
        expect(screen.getByText('CASH')).toBeInTheDocument();
      });

      // Toggle show inactive
      const toggle = screen.getByRole('switch', { name: /show-inactive/i });
      await user.click(toggle);

      // Should now show inactive payment methods
      await waitFor(() => {
        expect(screen.getByText('CARD_POS')).toBeInTheDocument();
      });
    });

    it('should display error message when loading fails', async () => {
      vi.mocked(paymentMethodService.getPaymentMethods).mockRejectedValueOnce(
        new Error('Network error')
      );

      const { toast } = await import('sonner');

      render(<PaymentMethodsPage />);

      await waitFor(() => {
        expect(toast.error).toHaveBeenCalledWith('Error al cargar los métodos de pago');
      });
    });
  });

  describe('Payment Method Count', () => {
    it('should display correct count of payment methods', async () => {
      render(<PaymentMethodsPage />);

      await waitFor(() => {
        expect(screen.getByText('2 métodos de pago')).toBeInTheDocument();
      });
    });

    it('should use singular form when count is 1', async () => {
      vi.mocked(paymentMethodService.getPaymentMethods).mockResolvedValueOnce([
        mockPaymentMethods[0],
      ]);

      render(<PaymentMethodsPage />);

      await waitFor(() => {
        expect(screen.getByText('1 método de pago')).toBeInTheDocument();
      });
    });
  });

  describe('Info Banner', () => {
    it('should display predefined methods information', async () => {
      render(<PaymentMethodsPage />);

      expect(screen.getByText('Métodos de pago predefinidos')).toBeInTheDocument();
      expect(
        screen.getByText(/El sistema incluye 6 métodos de pago predefinidos/i)
      ).toBeInTheDocument();
      expect(screen.getByText(/CASH.*BIZUM.*TRANSFER/i)).toBeInTheDocument();
    });
  });

  describe('Create Payment Method', () => {
    it('should open create dialog when clicking new button', async () => {
      const user = userEvent.setup();

      render(<PaymentMethodsPage />);

      await waitFor(() => {
        expect(screen.getByText('CASH')).toBeInTheDocument();
      });

      const createButton = screen.getByRole('button', { name: /nuevo método de pago/i });
      await user.click(createButton);

      // Dialog should open (implementation depends on dialog component)
      await waitFor(() => {
        expect(screen.getByText(/crear método de pago/i)).toBeInTheDocument();
      });
    });
  });

  describe('Edit Payment Method', () => {
    it('should open edit dialog when clicking edit action', async () => {
      const user = userEvent.setup();

      render(<PaymentMethodsPage />);

      await waitFor(() => {
        expect(screen.getByText('CASH')).toBeInTheDocument();
      });

      // Click on actions menu (MoreHorizontal icon)
      const actionButtons = screen.getAllByRole('button', { name: /abrir menú/i });
      await user.click(actionButtons[0]);

      // Click edit option
      const editButton = screen.getByRole('menuitem', { name: /editar/i });
      await user.click(editButton);

      // Dialog should open with edit mode
      await waitFor(() => {
        expect(screen.getByText(/editar método de pago/i)).toBeInTheDocument();
      });
    });
  });

  describe('Toggle Payment Method Status', () => {
    it('should deactivate an active payment method', async () => {
      const user = userEvent.setup();
      const updatedMethod = { ...mockPaymentMethods[0], isActive: false };

      vi.mocked(paymentMethodService.changePaymentMethodStatus).mockResolvedValueOnce(
        updatedMethod
      );
      vi.mocked(paymentMethodService.getPaymentMethods).mockResolvedValueOnce(
        mockPaymentMethods.filter((pm) => pm.isActive)
      );

      const { toast } = await import('sonner');

      render(<PaymentMethodsPage />);

      await waitFor(() => {
        expect(screen.getByText('CASH')).toBeInTheDocument();
      });

      // Open actions menu
      const actionButtons = screen.getAllByRole('button', { name: /abrir menú/i });
      await user.click(actionButtons[0]);

      // Click deactivate option
      const deactivateButton = screen.getByRole('menuitem', { name: /desactivar/i });
      await user.click(deactivateButton);

      await waitFor(() => {
        expect(paymentMethodService.changePaymentMethodStatus).toHaveBeenCalledWith('1', false);
        expect(toast.success).toHaveBeenCalledWith('Método de pago desactivado correctamente');
      });
    });

    it('should show error message when status toggle fails', async () => {
      const user = userEvent.setup();

      vi.mocked(paymentMethodService.changePaymentMethodStatus).mockRejectedValueOnce({
        message: 'Error personalizado',
        statusCode: 400,
      });

      const { toast } = await import('sonner');

      render(<PaymentMethodsPage />);

      await waitFor(() => {
        expect(screen.getByText('CASH')).toBeInTheDocument();
      });

      // Open actions menu and click deactivate
      const actionButtons = screen.getAllByRole('button', { name: /abrir menú/i });
      await user.click(actionButtons[0]);

      const deactivateButton = screen.getByRole('menuitem', { name: /desactivar/i });
      await user.click(deactivateButton);

      await waitFor(() => {
        expect(toast.error).toHaveBeenCalledWith('Error personalizado');
      });
    });
  });
});
