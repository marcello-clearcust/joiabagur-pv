/**
 * Tests for ComponentAssignmentSection - EP10 HU-EP10-002/003/004/005
 */

import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { ComponentAssignmentSection } from './component-assignment-section';
import { componentService } from '@/services/component.service';

vi.mock('@/services/component.service', () => ({
  componentService: {
    getProductComponents: vi.fn(),
    saveProductComponents: vi.fn(),
    searchComponents: vi.fn(),
    getSyncPreview: vi.fn(),
    syncFromMaster: vi.fn(),
    getTemplates: vi.fn(),
    getComponentById: vi.fn(),
    applyTemplate: vi.fn(),
  },
}));

vi.mock('sonner', () => ({
  toast: { success: vi.fn(), error: vi.fn(), info: vi.fn() },
}));

describe('ComponentAssignmentSection', () => {
  const defaultProps = {
    productId: 'prod-1',
    productPrice: 500,
    onTotalsChange: vi.fn(),
    onAssignmentsChange: vi.fn(),
    onAdjustPrice: vi.fn(),
  };

  beforeEach(() => {
    vi.clearAllMocks();
    vi.mocked(componentService.getProductComponents).mockResolvedValue([]);
    vi.mocked(componentService.getTemplates).mockResolvedValue([]);
  });

  it('should render section title', async () => {
    render(<ComponentAssignmentSection {...defaultProps} />);
    expect(screen.getByText('Componentes')).toBeInTheDocument();
  });

  it('should show empty state when no assignments', async () => {
    render(<ComponentAssignmentSection {...defaultProps} />);
    await waitFor(() => {
      expect(screen.getByText(/No hay componentes asignados/)).toBeInTheDocument();
    });
  });

  it('should render search input', () => {
    render(<ComponentAssignmentSection {...defaultProps} />);
    expect(screen.getByPlaceholderText('Buscar componente para añadir...')).toBeInTheDocument();
  });

  it('should load assignments for existing product', async () => {
    vi.mocked(componentService.getProductComponents).mockResolvedValue([
      { id: '1', componentId: 'c1', componentDescription: 'Oro', quantity: 3, costPrice: 150, salePrice: 200, displayOrder: 0, masterCostPrice: 150, masterSalePrice: 200 },
    ]);

    render(<ComponentAssignmentSection {...defaultProps} />);

    await waitFor(() => {
      expect(screen.getByText('Oro')).toBeInTheDocument();
    });
  });

  it('should calculate totals in real-time', async () => {
    vi.mocked(componentService.getProductComponents).mockResolvedValue([
      { id: '1', componentId: 'c1', componentDescription: 'Oro', quantity: 3, costPrice: 150, salePrice: 200, displayOrder: 0, masterCostPrice: 150, masterSalePrice: 200 },
    ]);

    render(<ComponentAssignmentSection {...defaultProps} />);

    await waitFor(() => {
      expect(defaultProps.onTotalsChange).toHaveBeenCalledWith(450, 600);
    });
  });

  it('should show deviation warning when price differs > 10%', async () => {
    vi.mocked(componentService.getProductComponents).mockResolvedValue([
      { id: '1', componentId: 'c1', componentDescription: 'Oro', quantity: 1, costPrice: 80, salePrice: 100, displayOrder: 0, masterCostPrice: 80, masterSalePrice: 100 },
    ]);

    render(<ComponentAssignmentSection {...{ ...defaultProps, productPrice: 120 }} />);

    await waitFor(() => {
      expect(screen.getByText(/difiere más del 10%/)).toBeInTheDocument();
      expect(screen.getByText('Ajustar a precio sugerido')).toBeInTheDocument();
    });
  });

  it('should NOT show deviation warning when within 10%', async () => {
    vi.mocked(componentService.getProductComponents).mockResolvedValue([
      { id: '1', componentId: 'c1', componentDescription: 'Oro', quantity: 1, costPrice: 80, salePrice: 100, displayOrder: 0, masterCostPrice: 80, masterSalePrice: 100 },
    ]);

    render(<ComponentAssignmentSection {...{ ...defaultProps, productPrice: 108 }} />);

    await waitFor(() => {
      expect(screen.queryByText(/difiere más del 10%/)).not.toBeInTheDocument();
    });
  });

  it('should search components with debounce', async () => {
    const user = userEvent.setup();
    vi.mocked(componentService.searchComponents).mockResolvedValue([
      { id: 'c2', description: 'Plata', costPrice: 12, salePrice: 22, isActive: true, createdAt: '', updatedAt: '' },
    ]);

    render(<ComponentAssignmentSection {...defaultProps} />);
    const searchInput = screen.getByPlaceholderText('Buscar componente para añadir...');
    await user.type(searchInput, 'Pl');

    await waitFor(() => {
      expect(componentService.searchComponents).toHaveBeenCalledWith('Pl');
    });
  });

  it('should show sync button when product has components', async () => {
    vi.mocked(componentService.getProductComponents).mockResolvedValue([
      { id: '1', componentId: 'c1', componentDescription: 'Oro', quantity: 1, costPrice: 150, salePrice: 200, displayOrder: 0, masterCostPrice: 150, masterSalePrice: 200 },
    ]);

    render(<ComponentAssignmentSection {...defaultProps} />);

    await waitFor(() => {
      expect(screen.getByText('Aplicar precios del maestro')).toBeInTheDocument();
    });
  });
});
