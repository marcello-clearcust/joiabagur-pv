/**
 * Tests for ComponentListPage - EP10 HU-EP10-001
 */

import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { BrowserRouter } from 'react-router-dom';
import { ComponentListPage } from './index';
import { componentService } from '@/services/component.service';

vi.mock('@/services/component.service', () => ({
  componentService: {
    getComponents: vi.fn(),
    createComponent: vi.fn(),
    updateComponent: vi.fn(),
    searchComponents: vi.fn(),
  },
}));

vi.mock('sonner', () => ({
  toast: { success: vi.fn(), error: vi.fn(), info: vi.fn() },
}));

const mockComponents = {
  items: [
    { id: '1', description: 'Oro 18k', costPrice: 150, salePrice: 200, isActive: true, createdAt: '2024-01-01', updatedAt: '2024-01-01' },
    { id: '2', description: 'Plata', costPrice: 12, salePrice: 22, isActive: true, createdAt: '2024-01-01', updatedAt: '2024-01-01' },
  ],
  totalCount: 2, totalPages: 1, currentPage: 1, pageSize: 50, hasPreviousPage: false, hasNextPage: false,
};

function renderPage() {
  return render(<BrowserRouter><ComponentListPage /></BrowserRouter>);
}

describe('ComponentListPage', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    vi.mocked(componentService.getComponents).mockResolvedValue(mockComponents);
  });

  describe('Rendering', () => {
    it('should render page title', async () => {
      renderPage();
      expect(screen.getByText('Componentes')).toBeInTheDocument();
    });

    it('should render component list after loading', async () => {
      renderPage();
      await waitFor(() => {
        expect(screen.getByText('Oro 18k')).toBeInTheDocument();
        expect(screen.getByText('Plata')).toBeInTheDocument();
      });
    });

    it('should show total count', async () => {
      renderPage();
      await waitFor(() => {
        expect(screen.getByText(/2 componente/)).toBeInTheDocument();
      });
    });

    it('should show create button', () => {
      renderPage();
      expect(screen.getByText('Nuevo Componente')).toBeInTheDocument();
    });
  });

  describe('Filtering', () => {
    it('should call service with search param when typing', async () => {
      const user = userEvent.setup();
      renderPage();
      await waitFor(() => expect(componentService.getComponents).toHaveBeenCalled());

      const searchInput = screen.getByPlaceholderText('Buscar por descripción...');
      await user.type(searchInput, 'Oro');

      await waitFor(() => {
        expect(componentService.getComponents).toHaveBeenCalledWith(
          expect.objectContaining({ search: 'Oro' })
        );
      });
    });
  });

  describe('Create Dialog', () => {
    it('should open dialog when clicking create button', async () => {
      const user = userEvent.setup();
      renderPage();
      await waitFor(() => expect(componentService.getComponents).toHaveBeenCalled());

      // Click the header button (first one with this text)
      const buttons = screen.getAllByText('Nuevo Componente');
      await user.click(buttons[0]);

      await waitFor(() => {
        expect(screen.getByPlaceholderText('Ej: Oro 18k')).toBeInTheDocument();
      });
    });
  });

  describe('Empty state', () => {
    it('should show empty message when no components', async () => {
      vi.mocked(componentService.getComponents).mockResolvedValue({
        items: [], totalCount: 0, totalPages: 0, currentPage: 1, pageSize: 50, hasPreviousPage: false, hasNextPage: false,
      });
      renderPage();
      await waitFor(() => {
        expect(screen.getByText('No hay componentes registrados')).toBeInTheDocument();
      });
    });
  });
});
