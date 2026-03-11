import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { DashboardPage } from './page';

const mockUseAuth = vi.fn();

vi.mock('@/providers/auth-provider', () => ({
  useAuth: () => mockUseAuth(),
}));

vi.mock('./AdminDashboard', () => ({
  AdminDashboard: () => <div data-testid="admin-dashboard">Admin Dashboard</div>,
}));

vi.mock('./OperatorDashboard', () => ({
  OperatorDashboard: () => <div data-testid="operator-dashboard">Operator Dashboard</div>,
}));

function renderDashboard() {
  return render(
    <MemoryRouter>
      <DashboardPage />
    </MemoryRouter>,
  );
}

describe('DashboardPage', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('should render AdminDashboard for Administrator role', () => {
    mockUseAuth.mockReturnValue({
      user: { userId: '1', username: 'admin', firstName: 'Admin', lastName: 'User', role: 'Administrator' },
      isAuthenticated: true,
      isLoading: false,
    });

    renderDashboard();

    expect(screen.getByTestId('admin-dashboard')).toBeInTheDocument();
    expect(screen.queryByTestId('operator-dashboard')).not.toBeInTheDocument();
  });

  it('should render OperatorDashboard for Operator role', () => {
    mockUseAuth.mockReturnValue({
      user: {
        userId: '2',
        username: 'operator',
        firstName: 'Op',
        lastName: 'User',
        role: 'Operator',
        assignedPointOfSales: [{ pointOfSaleId: 'pos-1', name: 'Tienda 1', code: 'T1', assignedAt: '', isActive: true }],
      },
      isAuthenticated: true,
      isLoading: false,
    });

    renderDashboard();

    expect(screen.getByTestId('operator-dashboard')).toBeInTheDocument();
    expect(screen.queryByTestId('admin-dashboard')).not.toBeInTheDocument();
  });

  it('should render nothing when user is null', () => {
    mockUseAuth.mockReturnValue({
      user: null,
      isAuthenticated: false,
      isLoading: false,
    });

    const { container } = renderDashboard();

    expect(container.innerHTML).toBe('');
  });
});
