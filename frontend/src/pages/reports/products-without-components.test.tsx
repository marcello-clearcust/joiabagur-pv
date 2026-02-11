/**
 * Tests for ProductsWithoutComponentsPage - EP10 HU-EP10-008
 */

import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { BrowserRouter } from 'react-router-dom';
import { ProductsWithoutComponentsPage } from './products-without-components';
import { componentService } from '@/services/component.service';

vi.mock('@/services/component.service', () => ({
  componentService: {
    getProductsWithoutComponents: vi.fn(),
  },
}));

vi.mock('sonner', () => ({
  toast: { success: vi.fn(), error: vi.fn() },
}));

const mockNavigate = vi.fn();
vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual('react-router-dom');
  return { ...actual, useNavigate: () => mockNavigate };
});

const mockResult = {
  items: [
    { productId: 'p1', sku: 'JOY-001', productName: 'Gold Ring', price: 299.99, collectionName: 'Summer' },
    { productId: 'p2', sku: 'JOY-002', productName: 'Bracelet', price: 150, collectionName: null },
  ],
  totalCount: 2, totalPages: 1, currentPage: 1, pageSize: 50,
};

function renderPage() {
  return render(<BrowserRouter><ProductsWithoutComponentsPage /></BrowserRouter>);
}

describe('ProductsWithoutComponentsPage', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    vi.mocked(componentService.getProductsWithoutComponents).mockResolvedValue(mockResult);
  });

  it('should render page title', () => {
    renderPage();
    expect(screen.getByText('Productos sin Componentes')).toBeInTheDocument();
  });

  it('should render product list', async () => {
    renderPage();
    await waitFor(() => {
      expect(screen.getByText('JOY-001')).toBeInTheDocument();
      expect(screen.getByText('Gold Ring')).toBeInTheDocument();
      expect(screen.getByText('JOY-002')).toBeInTheDocument();
    });
  });

  it('should show edit buttons', async () => {
    renderPage();
    await waitFor(() => {
      const editButtons = screen.getAllByText('Editar');
      expect(editButtons).toHaveLength(2);
    });
  });

  it('should navigate to edit on button click', async () => {
    const user = userEvent.setup();
    renderPage();
    await waitFor(() => expect(screen.getByText('JOY-001')).toBeInTheDocument());

    const editButtons = screen.getAllByText('Editar');
    await user.click(editButtons[0]);

    expect(mockNavigate).toHaveBeenCalledWith('/products/p1/edit');
  });

  it('should show count', async () => {
    renderPage();
    await waitFor(() => {
      expect(screen.getByText('2 productos')).toBeInTheDocument();
    });
  });

  it('should show empty message when no products', async () => {
    vi.mocked(componentService.getProductsWithoutComponents).mockResolvedValue({
      items: [], totalCount: 0, totalPages: 0, currentPage: 1, pageSize: 50,
    });
    renderPage();
    await waitFor(() => {
      expect(screen.getByText('No hay productos sin componentes')).toBeInTheDocument();
    });
  });
});
