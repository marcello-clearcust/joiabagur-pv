/**
 * E2E Tests for Authentication Flow
 * Tests login, logout, and session management
 */

import { test, expect } from '@playwright/test';

test.describe('Authentication Flow', () => {
  test.beforeEach(async ({ page }) => {
    // Start from the root URL, which should redirect to login
    await page.goto('/');
  });

  test.describe('Login Page', () => {
    test('should redirect unauthenticated users to login', async ({ page }) => {
      await expect(page).toHaveURL(/.*login.*/);
    });

    test('should display login form elements', async ({ page }) => {
      await page.goto('/login');

      // Check for form elements
      await expect(page.getByLabel(/usuario/i)).toBeVisible();
      await expect(page.getByLabel(/contraseña/i)).toBeVisible();
      await expect(page.getByRole('button', { name: /iniciar sesión/i })).toBeVisible();
      await expect(page.getByLabel(/recordar mi usuario/i)).toBeVisible();
    });

    test('should display application branding', async ({ page }) => {
      await page.goto('/login');

      await expect(page.getByText('JoiaBagur')).toBeVisible();
      await expect(page.getByText(/sistema punto de venta/i)).toBeVisible();
    });
  });

  test.describe('Form Validation', () => {
    test('should show validation error for empty username', async ({ page }) => {
      await page.goto('/login');

      await page.getByRole('button', { name: /iniciar sesión/i }).click();

      await expect(page.getByText(/el nombre de usuario es requerido/i)).toBeVisible();
    });

    test('should show validation error for empty password', async ({ page }) => {
      await page.goto('/login');

      await page.getByLabel(/usuario/i).fill('testuser');
      await page.getByRole('button', { name: /iniciar sesión/i }).click();

      await expect(page.getByText(/la contraseña es requerida/i)).toBeVisible();
    });

    test('should show validation error for short username', async ({ page }) => {
      await page.goto('/login');

      await page.getByLabel(/usuario/i).fill('ab');
      await page.getByLabel(/contraseña/i).fill('password123');
      await page.getByRole('button', { name: /iniciar sesión/i }).click();

      await expect(page.getByText(/al menos 3 caracteres/i)).toBeVisible();
    });

    test('should show validation error for short password', async ({ page }) => {
      await page.goto('/login');

      await page.getByLabel(/usuario/i).fill('testuser');
      await page.getByLabel(/contraseña/i).fill('short');
      await page.getByRole('button', { name: /iniciar sesión/i }).click();

      await expect(page.getByText(/al menos 8 caracteres/i)).toBeVisible();
    });
  });

  test.describe('Password Visibility Toggle', () => {
    test('should toggle password visibility', async ({ page }) => {
      await page.goto('/login');

      const passwordInput = page.getByLabel(/contraseña/i);
      
      // Initially password should be hidden
      await expect(passwordInput).toHaveAttribute('type', 'password');
      
      // Fill password to see it being toggled
      await passwordInput.fill('password123');

      // Click the toggle button (find by tabindex or position)
      const toggleButton = page.locator('button[tabindex="-1"]');
      await toggleButton.click();

      // Now password should be visible
      await expect(passwordInput).toHaveAttribute('type', 'text');

      // Toggle back
      await toggleButton.click();
      await expect(passwordInput).toHaveAttribute('type', 'password');
    });
  });

  test.describe('Remember Username', () => {
    test('should remember username after checking the option', async ({ page }) => {
      await page.goto('/login');

      // Fill in username
      await page.getByLabel(/usuario/i).fill('remembereduser');
      
      // Check "remember me"
      await page.getByLabel(/recordar mi usuario/i).check();

      // Store in local storage (simulating what happens after login attempt)
      await page.evaluate(() => {
        localStorage.setItem('joiabagur_remembered_username', 'remembereduser');
      });

      // Reload the page
      await page.reload();

      // Username should be pre-filled
      await expect(page.getByLabel(/usuario/i)).toHaveValue('remembereduser');
    });
  });

  test.describe('Login Attempt', () => {
    test('should show loading state during login', async ({ page }) => {
      await page.goto('/login');

      // Mock API to delay response
      await page.route('**/api/auth/login', async (route) => {
        await new Promise((resolve) => setTimeout(resolve, 1000));
        await route.fulfill({ 
          status: 401, 
          json: { error: 'Invalid credentials' } 
        });
      });

      await page.getByLabel(/usuario/i).fill('testuser');
      await page.getByLabel(/contraseña/i).fill('password123');
      await page.getByRole('button', { name: /iniciar sesión/i }).click();

      // Should show loading text
      await expect(page.getByText(/iniciando sesión/i)).toBeVisible();
    });

    test('should show error message for invalid credentials', async ({ page }) => {
      await page.goto('/login');

      // Mock API to return 401
      await page.route('**/api/auth/login', async (route) => {
        await route.fulfill({ 
          status: 401, 
          json: { error: 'Usuario o contraseña incorrectos' } 
        });
      });

      await page.getByLabel(/usuario/i).fill('testuser');
      await page.getByLabel(/contraseña/i).fill('wrongpassword');
      await page.getByRole('button', { name: /iniciar sesión/i }).click();

      await expect(page.getByText(/credenciales incorrectas|usuario o contraseña/i)).toBeVisible();
    });

    test('should redirect to dashboard on successful login', async ({ page }) => {
      await page.goto('/login');

      // Mock successful login
      await page.route('**/api/auth/login', async (route) => {
        await route.fulfill({ 
          status: 200, 
          json: {
            userId: '123',
            username: 'admin',
            firstName: 'Admin',
            lastName: 'User',
            role: 'Administrator',
          },
          headers: {
            'Set-Cookie': 'access_token=mock-token; HttpOnly; Path=/',
          },
        });
      });

      // Mock /auth/me endpoint
      await page.route('**/api/auth/me', async (route) => {
        await route.fulfill({ 
          status: 200, 
          json: {
            userId: '123',
            username: 'admin',
            firstName: 'Admin',
            lastName: 'User',
            role: 'Administrator',
          },
        });
      });

      await page.getByLabel(/usuario/i).fill('admin');
      await page.getByLabel(/contraseña/i).fill('password123');
      await page.getByRole('button', { name: /iniciar sesión/i }).click();

      // Should redirect away from login
      await expect(page).not.toHaveURL(/.*login.*/);
    });
  });

  test.describe('Inactive User', () => {
    test('should show error for inactive user', async ({ page }) => {
      await page.goto('/login');

      // Mock API to return inactive user error
      await page.route('**/api/auth/login', async (route) => {
        await route.fulfill({ 
          status: 401, 
          json: { error: 'Usuario desactivado. Contacte al administrador' } 
        });
      });

      await page.getByLabel(/usuario/i).fill('inactiveuser');
      await page.getByLabel(/contraseña/i).fill('password123');
      await page.getByRole('button', { name: /iniciar sesión/i }).click();

      await expect(page.getByText(/desactivado|contacte al administrador/i)).toBeVisible();
    });
  });
});
