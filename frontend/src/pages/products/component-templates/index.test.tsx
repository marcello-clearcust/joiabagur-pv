/**
 * Tests for ComponentTemplatesPage - EP10 HU-EP10-006
 */

import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import { BrowserRouter } from 'react-router-dom';
import { ComponentTemplatesPage } from './index';
import { componentService } from '@/services/component.service';

vi.mock('@/services/component.service', () => ({
  componentService: {
    getTemplates: vi.fn(),
    deleteTemplate: vi.fn(),
    searchComponents: vi.fn(),
    createTemplate: vi.fn(),
    updateTemplate: vi.fn(),
  },
}));

vi.mock('sonner', () => ({
  toast: { success: vi.fn(), error: vi.fn(), info: vi.fn() },
}));

const mockTemplates = [
  { id: 't1', name: 'Anillo oro', description: 'Gold ring template', items: [{ componentId: 'c1', componentDescription: 'Oro', quantity: 3 }], createdAt: '2024-01-01', updatedAt: '2024-01-01' },
  { id: 't2', name: 'Pulsera plata', description: null, items: [], createdAt: '2024-01-01', updatedAt: '2024-01-01' },
];

function renderPage() {
  return render(<BrowserRouter><ComponentTemplatesPage /></BrowserRouter>);
}

describe('ComponentTemplatesPage', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    vi.mocked(componentService.getTemplates).mockResolvedValue(mockTemplates);
  });

  it('should render page title', () => {
    renderPage();
    expect(screen.getByText('Plantillas de Componentes')).toBeInTheDocument();
  });

  it('should render template list after loading', async () => {
    renderPage();
    await waitFor(() => {
      expect(screen.getByText('Anillo oro')).toBeInTheDocument();
      expect(screen.getByText('Pulsera plata')).toBeInTheDocument();
    });
  });

  it('should show item counts', async () => {
    renderPage();
    await waitFor(() => {
      expect(screen.getByText('1 componente')).toBeInTheDocument();
      expect(screen.getByText('0 componentes')).toBeInTheDocument();
    });
  });

  it('should show create button', () => {
    renderPage();
    expect(screen.getByText('Nueva Plantilla')).toBeInTheDocument();
  });

  it('should show empty message when no templates', async () => {
    vi.mocked(componentService.getTemplates).mockResolvedValue([]);
    renderPage();
    await waitFor(() => {
      expect(screen.getByText('No hay plantillas registradas')).toBeInTheDocument();
    });
  });
});
