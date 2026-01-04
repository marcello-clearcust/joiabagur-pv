/**
 * Component tests for LoginPage
 * Tests form validation, submission, error handling, and UX features
 */

import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter } from 'react-router-dom';
import { LoginPage } from './login';

// Mock useAuth hook - must be before component import
const mockLogin = vi.fn();
const mockUseAuth = vi.fn(() => ({
  login: mockLogin,
  isAuthenticated: false,
  isLoading: false,
  user: null,
}));

vi.mock('@/providers', () => ({
  useAuth: () => mockUseAuth(),
}));

function renderLoginPage() {
  return render(
    <MemoryRouter>
      <LoginPage />
    </MemoryRouter>
  );
}

describe('LoginPage', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    localStorage.clear();
  });

  // Helper to get form elements by placeholder
  const getUsernameInput = () => screen.getByPlaceholderText(/ingresa tu nombre de usuario/i);
  const getPasswordInput = () => screen.getByPlaceholderText(/ingresa tu contraseña/i);
  const getSubmitButton = () => screen.getByRole('button', { name: /iniciar sesión/i });

  describe('Rendering', () => {
    it('should render login form with all required elements', () => {
      renderLoginPage();

      // Use heading role to be specific
      expect(screen.getByRole('heading', { name: /iniciar sesión/i })).toBeInTheDocument();
      expect(getUsernameInput()).toBeInTheDocument();
      expect(getPasswordInput()).toBeInTheDocument();
      expect(getSubmitButton()).toBeInTheDocument();
    });

    it('should render remember username checkbox', () => {
      renderLoginPage();

      expect(screen.getByText(/recordar mi usuario/i)).toBeInTheDocument();
    });

    it('should render password visibility toggle', () => {
      renderLoginPage();

      const passwordInput = getPasswordInput();
      expect(passwordInput).toHaveAttribute('type', 'password');
    });
  });

  describe('Form Validation', () => {
    it('should show validation error when username is empty', async () => {
      const user = userEvent.setup();
      renderLoginPage();

      await user.click(getSubmitButton());

      await waitFor(() => {
        expect(screen.getByText(/el nombre de usuario es requerido/i)).toBeInTheDocument();
      });
    });

    it('should show validation error when password is empty', async () => {
      const user = userEvent.setup();
      renderLoginPage();

      await user.type(getUsernameInput(), 'testuser');
      await user.click(getSubmitButton());

      await waitFor(() => {
        expect(screen.getByText(/la contraseña es requerida/i)).toBeInTheDocument();
      });
    });

    it('should show validation error when username is too short', async () => {
      const user = userEvent.setup();
      renderLoginPage();

      await user.type(getUsernameInput(), 'ab');
      await user.type(getPasswordInput(), 'password123');
      await user.click(getSubmitButton());

      await waitFor(() => {
        expect(screen.getByText(/al menos 3 caracteres/i)).toBeInTheDocument();
      });
    });

    it('should show validation error when password is too short', async () => {
      const user = userEvent.setup();
      renderLoginPage();

      await user.type(getUsernameInput(), 'testuser');
      await user.type(getPasswordInput(), 'short');
      await user.click(getSubmitButton());

      await waitFor(() => {
        expect(screen.getByText(/al menos 8 caracteres/i)).toBeInTheDocument();
      });
    });
  });

  describe('Form Submission', () => {
    it('should call login with credentials on valid form submit', async () => {
      mockLogin.mockResolvedValueOnce(undefined);
      const user = userEvent.setup();
      renderLoginPage();

      await user.type(getUsernameInput(), 'testuser');
      await user.type(getPasswordInput(), 'password123');
      await user.click(getSubmitButton());

      await waitFor(() => {
        expect(mockLogin).toHaveBeenCalledWith({
          username: 'testuser',
          password: 'password123',
        });
      });
    });

    it('should show loading state during submission', async () => {
      // Make login hang
      mockLogin.mockImplementation(() => new Promise(() => {}));
      const user = userEvent.setup();
      renderLoginPage();

      await user.type(getUsernameInput(), 'testuser');
      await user.type(getPasswordInput(), 'password123');
      await user.click(getSubmitButton());

      await waitFor(() => {
        expect(screen.getByText(/iniciando sesión/i)).toBeInTheDocument();
      });
    });

    it('should show error message on login failure', async () => {
      mockLogin.mockRejectedValueOnce(new Error('Credenciales incorrectas'));
      const user = userEvent.setup();
      renderLoginPage();

      await user.type(getUsernameInput(), 'testuser');
      await user.type(getPasswordInput(), 'wrongpassword');
      await user.click(getSubmitButton());

      await waitFor(() => {
        expect(screen.getByText(/credenciales incorrectas/i)).toBeInTheDocument();
      });
    });
  });

  describe('Password Visibility Toggle', () => {
    it('should toggle password visibility when eye icon is clicked', async () => {
      const user = userEvent.setup();
      renderLoginPage();

      const passwordInput = getPasswordInput();
      expect(passwordInput).toHaveAttribute('type', 'password');

      // Find and click the toggle button (it's a button with tabindex -1)
      const toggleButtons = screen.getAllByRole('button');
      const eyeButton = toggleButtons.find(btn => btn.getAttribute('tabindex') === '-1');
      
      if (eyeButton) {
        await user.click(eyeButton);
        expect(passwordInput).toHaveAttribute('type', 'text');

        await user.click(eyeButton);
        expect(passwordInput).toHaveAttribute('type', 'password');
      }
    });
  });

  describe('Remember Username', () => {
    it('should save username to localStorage when remember is checked', async () => {
      mockLogin.mockResolvedValueOnce(undefined);
      const user = userEvent.setup();
      renderLoginPage();

      await user.type(getUsernameInput(), 'saveduser');
      await user.type(getPasswordInput(), 'password123');
      
      // Find and click checkbox
      const checkbox = screen.getByRole('checkbox');
      await user.click(checkbox);
      
      await user.click(getSubmitButton());

      await waitFor(() => {
        expect(localStorage.getItem('joiabagur_remembered_username')).toBe('saveduser');
      });
    });

    it('should load remembered username on mount', () => {
      localStorage.setItem('joiabagur_remembered_username', 'remembereduser');
      renderLoginPage();

      expect(getUsernameInput()).toHaveValue('remembereduser');
    });

    it('should remove username from localStorage when unchecked', async () => {
      localStorage.setItem('joiabagur_remembered_username', 'olduser');
      mockLogin.mockResolvedValueOnce(undefined);
      const user = userEvent.setup();
      renderLoginPage();

      // The checkbox should be checked (because username was remembered)
      const checkbox = screen.getByRole('checkbox');
      await user.click(checkbox); // Uncheck

      await user.clear(getUsernameInput());
      await user.type(getUsernameInput(), 'newuser');
      await user.type(getPasswordInput(), 'password123');
      await user.click(getSubmitButton());

      await waitFor(() => {
        expect(localStorage.getItem('joiabagur_remembered_username')).toBeNull();
      });
    });
  });
});
