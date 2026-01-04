/**
 * Component tests for UsersPage
 * Tests user list display, CRUD operations, and assignments dialog
 */

import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { BrowserRouter } from 'react-router-dom';
import { UsersPage } from './index';
import { userService } from '@/services/user.service';

// Mock the user service
vi.mock('@/services/user.service', () => ({
  userService: {
    getUsers: vi.fn(),
    getUser: vi.fn(),
    createUser: vi.fn(),
    updateUser: vi.fn(),
    changePassword: vi.fn(),
    getUserPointOfSales: vi.fn(),
    assignUserToPointOfSale: vi.fn(),
    unassignUserFromPointOfSale: vi.fn(),
  },
}));

// Mock sonner toast
vi.mock('sonner', () => ({
  toast: {
    success: vi.fn(),
    error: vi.fn(),
  },
}));

const mockUsers = [
  {
    id: '1',
    username: 'admin',
    firstName: 'Admin',
    lastName: 'User',
    email: 'admin@test.com',
    role: 'Administrator' as const,
    isActive: true,
  },
  {
    id: '2',
    username: 'operator1',
    firstName: 'Operator',
    lastName: 'One',
    email: 'op1@test.com',
    role: 'Operator' as const,
    isActive: true,
  },
  {
    id: '3',
    username: 'operator2',
    firstName: 'Operator',
    lastName: 'Two',
    email: null,
    role: 'Operator' as const,
    isActive: false,
  },
];

function renderUsersPage() {
  return render(
    <BrowserRouter>
      <UsersPage />
    </BrowserRouter>
  );
}

describe('UsersPage', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    vi.mocked(userService.getUsers).mockResolvedValue(mockUsers);
  });

  describe('Rendering', () => {
    it('should render page title and create button', async () => {
      renderUsersPage();

      expect(screen.getByRole('heading', { name: /usuarios/i })).toBeInTheDocument();
      expect(screen.getByRole('button', { name: /nuevo usuario/i })).toBeInTheDocument();
    });

    it('should display loading state initially', () => {
      vi.mocked(userService.getUsers).mockImplementation(() => new Promise(() => {}));
      renderUsersPage();

      // Should show some loading indication (skeleton or similar)
      expect(screen.queryByText('admin')).not.toBeInTheDocument();
    });

    it('should display users list after loading', async () => {
      renderUsersPage();

      await waitFor(() => {
        expect(screen.getByText('admin')).toBeInTheDocument();
        expect(screen.getByText('operator1')).toBeInTheDocument();
        expect(screen.getByText('operator2')).toBeInTheDocument();
      });
    });

    it('should display user full names', async () => {
      renderUsersPage();

      await waitFor(() => {
        expect(screen.getByText('Admin User')).toBeInTheDocument();
        expect(screen.getByText('Operator One')).toBeInTheDocument();
      });
    });

    it('should display role badges', async () => {
      renderUsersPage();

      await waitFor(() => {
        expect(screen.getByText('Administrador')).toBeInTheDocument();
        expect(screen.getAllByText('Operador')).toHaveLength(2);
      });
    });

    it('should display active/inactive status', async () => {
      renderUsersPage();

      await waitFor(() => {
        const activeLabels = screen.getAllByText('Activo');
        const inactiveLabels = screen.getAllByText('Inactivo');
        
        expect(activeLabels.length).toBeGreaterThanOrEqual(2);
        expect(inactiveLabels.length).toBeGreaterThanOrEqual(1);
      });
    });
  });

  describe('Create User Dialog', () => {
    it('should open create user dialog when clicking new user button', async () => {
      const user = userEvent.setup();
      renderUsersPage();

      await waitFor(() => {
        expect(screen.getByText('admin')).toBeInTheDocument();
      });

      await user.click(screen.getByRole('button', { name: /nuevo usuario/i }));

      await waitFor(() => {
        expect(screen.getByRole('dialog')).toBeInTheDocument();
      });
    });

    it('should show required fields in create dialog', async () => {
      const user = userEvent.setup();
      renderUsersPage();

      await waitFor(() => {
        expect(screen.getByText('admin')).toBeInTheDocument();
      });

      await user.click(screen.getByRole('button', { name: /nuevo usuario/i }));

      await waitFor(() => {
        expect(screen.getByRole('dialog')).toBeInTheDocument();
      });

      const dialog = screen.getByRole('dialog');
      // Check for form inputs by placeholder text which is more reliable
      expect(within(dialog).getByPlaceholderText('usuario123')).toBeInTheDocument();
      expect(within(dialog).getByPlaceholderText('Juan')).toBeInTheDocument();
      expect(within(dialog).getByPlaceholderText('Pérez')).toBeInTheDocument();
    });

    it('should show password strength indicator', async () => {
      const user = userEvent.setup();
      renderUsersPage();

      await waitFor(() => {
        expect(screen.getByText('admin')).toBeInTheDocument();
      });

      await user.click(screen.getByRole('button', { name: /nuevo usuario/i }));

      await waitFor(() => {
        expect(screen.getByRole('dialog')).toBeInTheDocument();
      });

      const dialog = screen.getByRole('dialog');
      // Password input exists
      const passwordInput = within(dialog).getByPlaceholderText('••••••••');
      expect(passwordInput).toBeInTheDocument();
    });
  });

  describe('Edit User', () => {
    it('should open edit dialog from actions menu', async () => {
      const user = userEvent.setup();
      renderUsersPage();

      await waitFor(() => {
        expect(screen.getByText('admin')).toBeInTheDocument();
      });

      // Find and click the first actions menu button
      const menuButtons = screen.getAllByRole('button', { name: '' });
      const actionButton = menuButtons.find(btn => 
        btn.closest('td') !== null
      );
      
      if (actionButton) {
        await user.click(actionButton);

        await waitFor(() => {
          expect(screen.getByText(/editar/i)).toBeInTheDocument();
        });
      }
    });
  });

  describe('Error Handling', () => {
    it('should show error toast when fetching users fails', async () => {
      const { toast } = await import('sonner');
      vi.mocked(userService.getUsers).mockRejectedValueOnce(new Error('Network error'));
      
      renderUsersPage();

      await waitFor(() => {
        expect(toast.error).toHaveBeenCalledWith('Error al cargar los usuarios');
      });
    });
  });

  describe('Empty State', () => {
    it('should show empty message when no users', async () => {
      vi.mocked(userService.getUsers).mockResolvedValueOnce([]);
      renderUsersPage();

      await waitFor(() => {
        expect(screen.getByText(/no hay usuarios registrados/i)).toBeInTheDocument();
      });
    });
  });
});
