/**
 * Tests for ProductMarginsPage - EP10 HU-EP10-007
 */

import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { BrowserRouter } from 'react-router-dom';
import { ProductMarginsPage } from './product-margins';
import { componentService } from '@/services/component.service';

vi.mock('@/services/component.service', () => ({
  componentService: {
    getMarginReport: vi.fn(),
    exportMarginReport: vi.fn(),
  },
}));

vi.mock('sonner', () => ({
  toast: { success: vi.fn(), error: vi.fn() },
}));

const mockReport = {
  items: [
    { productId: 'p1', sku: 'JOY-001', productName: 'Gold Ring', collectionName: 'Summer', officialPrice: 500, totalCostPrice: 300, totalSalePrice: 450, marginAmount: 150, marginPercent: 33.33 },
  ],
  totalCount: 1, totalPages: 1, currentPage: 1, pageSize: 50,
  sumCostPrice: 300, sumSalePrice: 450, sumMargin: 150,
};

function renderPage() {
  return render(<BrowserRouter><ProductMarginsPage /></BrowserRouter>);
}

describe('ProductMarginsPage', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    vi.mocked(componentService.getMarginReport).mockResolvedValue(mockReport);
  });

  it('should render page title', () => {
    renderPage();
    expect(screen.getByText('Reporte de Márgenes')).toBeInTheDocument();
  });

  it('should render report data', async () => {
    renderPage();
    await waitFor(() => {
      expect(screen.getByText('JOY-001')).toBeInTheDocument();
      expect(screen.getByText('Gold Ring')).toBeInTheDocument();
    });
  });

  it('should show aggregated totals', async () => {
    renderPage();
    await waitFor(() => {
      // Totals are rendered in summary cards
      const costElements = screen.getAllByText('€300.00');
      expect(costElements.length).toBeGreaterThanOrEqual(1);
      const marginElements = screen.getAllByText('€150.00');
      expect(marginElements.length).toBeGreaterThanOrEqual(1);
    });
  });

  it('should show export button', () => {
    renderPage();
    expect(screen.getByText('Exportar Excel')).toBeInTheDocument();
  });

  it('should show empty message when no data', async () => {
    vi.mocked(componentService.getMarginReport).mockResolvedValue({
      items: [], totalCount: 0, totalPages: 0, currentPage: 1, pageSize: 50,
      sumCostPrice: 0, sumSalePrice: 0, sumMargin: 0,
    });
    renderPage();
    await waitFor(() => {
      expect(screen.getByText('No hay productos con componentes asignados')).toBeInTheDocument();
    });
  });

  it('should search when typing', async () => {
    const user = userEvent.setup();
    renderPage();
    await waitFor(() => expect(componentService.getMarginReport).toHaveBeenCalled());

    const searchInput = screen.getByPlaceholderText('Buscar por producto o SKU...');
    await user.type(searchInput, 'Gold');

    await waitFor(() => {
      expect(componentService.getMarginReport).toHaveBeenCalledWith(
        expect.objectContaining({ search: 'Gold' })
      );
    });
  });
});
