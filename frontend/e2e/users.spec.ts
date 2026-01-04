/**
 * E2E Tests for User Management CRUD Operations
 * Tests user list, create, edit, and assignments
 */

import { test, expect } from '@playwright/test';

// Helper to setup authenticated session
async function setupAuthenticatedSession(page: import('@playwright/test').Page) {
  // Mock auth endpoints
  await page.route('**/api/auth/me', async (route) => {
    await route.fulfill({
      status: 200,
      json: {
        userId: '1',
        username: 'admin',
        firstName: 'Admin',
        lastName: 'User',
        role: 'Administrator',
      },
    });
  });

  await page.route('**/api/auth/refresh', async (route) => {
    await route.fulfill({ status: 200, json: { message: 'Token refreshed' } });
  });
}

const mockUsers = [
  {
    id: '1',
    username: 'admin',
    firstName: 'Admin',
    lastName: 'User',
    email: 'admin@test.com',
    role: 'Administrator',
    isActive: true,
  },
  {
    id: '2',
    username: 'operator1',
    firstName: 'Juan',
    lastName: 'Pérez',
    email: 'juan@test.com',
    role: 'Operator',
    isActive: true,
  },
  {
    id: '3',
    username: 'operator2',
    firstName: 'María',
    lastName: 'García',
    email: null,
    role: 'Operator',
    isActive: false,
  },
];

test.describe('User Management', () => {
  test.beforeEach(async ({ page }) => {
    await setupAuthenticatedSession(page);

    // Mock users list endpoint
    await page.route('**/api/users', async (route) => {
      if (route.request().method() === 'GET') {
        await route.fulfill({ status: 200, json: mockUsers });
      } else {
        await route.continue();
      }
    });
  });

  test.describe('Users List', () => {
    test('should display users list page', async ({ page }) => {
      await page.goto('/users');

      await expect(page.getByRole('heading', { name: /usuarios/i })).toBeVisible();
      await expect(page.getByRole('button', { name: /nuevo usuario/i })).toBeVisible();
    });

    test('should display all users in the table', async ({ page }) => {
      await page.goto('/users');

      await expect(page.getByText('admin')).toBeVisible();
      await expect(page.getByText('operator1')).toBeVisible();
      await expect(page.getByText('operator2')).toBeVisible();
    });

    test('should display user full names', async ({ page }) => {
      await page.goto('/users');

      await expect(page.getByText('Admin User')).toBeVisible();
      await expect(page.getByText('Juan Pérez')).toBeVisible();
      await expect(page.getByText('María García')).toBeVisible();
    });

    test('should display role badges correctly', async ({ page }) => {
      await page.goto('/users');

      await expect(page.getByText('Administrador')).toBeVisible();
      // Multiple operators
      const operatorBadges = page.getByText('Operador');
      await expect(operatorBadges).toHaveCount(2);
    });

    test('should display active/inactive status', async ({ page }) => {
      await page.goto('/users');

      const activeLabels = page.getByText('Activo', { exact: true });
      const inactiveLabels = page.getByText('Inactivo');
      
      await expect(activeLabels).toHaveCount(2);
      await expect(inactiveLabels).toHaveCount(1);
    });
  });

  test.describe('Create User', () => {
    test('should open create user dialog', async ({ page }) => {
      await page.goto('/users');

      await page.getByRole('button', { name: /nuevo usuario/i }).click();

      await expect(page.getByRole('dialog')).toBeVisible();
      await expect(page.getByText(/nuevo usuario/i)).toBeVisible();
    });

    test('should display all form fields in create dialog', async ({ page }) => {
      await page.goto('/users');
      await page.getByRole('button', { name: /nuevo usuario/i }).click();

      const dialog = page.getByRole('dialog');
      await expect(dialog.getByLabel(/nombre de usuario/i)).toBeVisible();
      await expect(dialog.getByLabel(/^nombre$/i)).toBeVisible();
      await expect(dialog.getByLabel(/apellido/i)).toBeVisible();
      await expect(dialog.getByLabel(/email/i)).toBeVisible();
      await expect(dialog.getByLabel(/rol/i)).toBeVisible();
      await expect(dialog.getByLabel(/contraseña/i)).toBeVisible();
    });

    test('should create a new user successfully', async ({ page }) => {
      // Mock create user endpoint
      await page.route('**/api/users', async (route) => {
        if (route.request().method() === 'POST') {
          await route.fulfill({
            status: 201,
            json: {
              id: '4',
              username: 'newuser',
              firstName: 'Nuevo',
              lastName: 'Usuario',
              email: 'nuevo@test.com',
              role: 'Operator',
              isActive: true,
            },
          });
        } else {
          await route.continue();
        }
      });

      await page.goto('/users');
      await page.getByRole('button', { name: /nuevo usuario/i }).click();

      const dialog = page.getByRole('dialog');
      await dialog.getByLabel(/nombre de usuario/i).fill('newuser');
      await dialog.getByLabel(/^nombre$/i).fill('Nuevo');
      await dialog.getByLabel(/apellido/i).fill('Usuario');
      await dialog.getByLabel(/email/i).fill('nuevo@test.com');
      await dialog.getByLabel(/contraseña/i).fill('password123');

      // Select role
      await dialog.getByLabel(/rol/i).click();
      await page.getByRole('option', { name: /operador/i }).click();

      await dialog.getByRole('button', { name: /crear usuario/i }).click();

      // Dialog should close on success
      await expect(page.getByRole('dialog')).not.toBeVisible();
    });

    test('should show validation errors for empty required fields', async ({ page }) => {
      await page.goto('/users');
      await page.getByRole('button', { name: /nuevo usuario/i }).click();

      const dialog = page.getByRole('dialog');
      await dialog.getByRole('button', { name: /crear usuario/i }).click();

      await expect(dialog.getByText(/nombre de usuario es requerido/i)).toBeVisible();
    });

    test('should show error for duplicate username', async ({ page }) => {
      await page.route('**/api/users', async (route) => {
        if (route.request().method() === 'POST') {
          await route.fulfill({
            status: 409,
            json: { error: 'El nombre de usuario ya está en uso' },
          });
        } else {
          await route.continue();
        }
      });

      await page.goto('/users');
      await page.getByRole('button', { name: /nuevo usuario/i }).click();

      const dialog = page.getByRole('dialog');
      await dialog.getByLabel(/nombre de usuario/i).fill('admin');
      await dialog.getByLabel(/^nombre$/i).fill('Test');
      await dialog.getByLabel(/apellido/i).fill('User');
      await dialog.getByLabel(/contraseña/i).fill('password123');
      await dialog.getByLabel(/rol/i).click();
      await page.getByRole('option', { name: /operador/i }).click();

      await dialog.getByRole('button', { name: /crear usuario/i }).click();

      // Should show error toast or message
      await expect(page.getByText(/ya está en uso/i)).toBeVisible();
    });
  });

  test.describe('Edit User', () => {
    test('should open edit dialog from actions menu', async ({ page }) => {
      await page.goto('/users');

      // Click the first row's action menu
      const actionButtons = page.locator('button').filter({ has: page.locator('svg') });
      const menuButton = actionButtons.nth(1); // First is the "Nuevo Usuario" button
      await menuButton.click();

      await page.getByRole('menuitem', { name: /editar/i }).click();

      await expect(page.getByRole('dialog')).toBeVisible();
      await expect(page.getByText(/editar usuario/i)).toBeVisible();
    });

    test('should show current user data in edit form', async ({ page }) => {
      await page.route('**/api/users/2', async (route) => {
        await route.fulfill({
          status: 200,
          json: mockUsers[1],
        });
      });

      await page.goto('/users');

      // Find operator1's row and click edit
      const row = page.getByRole('row').filter({ hasText: 'operator1' });
      const menuButton = row.locator('button');
      await menuButton.click();
      await page.getByRole('menuitem', { name: /editar/i }).click();

      const dialog = page.getByRole('dialog');
      await expect(dialog.getByLabel(/^nombre$/i)).toHaveValue('Juan');
      await expect(dialog.getByLabel(/apellido/i)).toHaveValue('Pérez');
    });

    test('should update user successfully', async ({ page }) => {
      await page.route('**/api/users/*', async (route) => {
        if (route.request().method() === 'PUT') {
          await route.fulfill({
            status: 200,
            json: {
              ...mockUsers[1],
              firstName: 'Updated',
            },
          });
        } else {
          await route.continue();
        }
      });

      await page.goto('/users');

      const row = page.getByRole('row').filter({ hasText: 'operator1' });
      const menuButton = row.locator('button');
      await menuButton.click();
      await page.getByRole('menuitem', { name: /editar/i }).click();

      const dialog = page.getByRole('dialog');
      await dialog.getByLabel(/^nombre$/i).clear();
      await dialog.getByLabel(/^nombre$/i).fill('Updated');
      await dialog.getByRole('button', { name: /guardar cambios/i }).click();

      await expect(page.getByRole('dialog')).not.toBeVisible();
    });

    test('should toggle user active status', async ({ page }) => {
      await page.route('**/api/users/*', async (route) => {
        if (route.request().method() === 'PUT') {
          await route.fulfill({
            status: 200,
            json: { ...mockUsers[1], isActive: false },
          });
        } else {
          await route.continue();
        }
      });

      await page.goto('/users');

      const row = page.getByRole('row').filter({ hasText: 'operator1' });
      const menuButton = row.locator('button');
      await menuButton.click();
      await page.getByRole('menuitem', { name: /editar/i }).click();

      const dialog = page.getByRole('dialog');
      // Toggle the active switch
      const activeSwitch = dialog.locator('button[role="switch"]');
      await activeSwitch.click();

      await dialog.getByRole('button', { name: /guardar cambios/i }).click();

      await expect(page.getByRole('dialog')).not.toBeVisible();
    });
  });

  test.describe('Point of Sale Assignments', () => {
    test('should open assignments dialog from actions menu', async ({ page }) => {
      await page.route('**/api/users/*/point-of-sales', async (route) => {
        await route.fulfill({
          status: 200,
          json: [
            { pointOfSaleId: 'pos1', name: 'Tienda 1', code: 'T1', assignedAt: '2024-01-01', isActive: true },
          ],
        });
      });

      await page.goto('/users');

      const row = page.getByRole('row').filter({ hasText: 'operator1' });
      const menuButton = row.locator('button');
      await menuButton.click();
      await page.getByRole('menuitem', { name: /puntos de venta/i }).click();

      await expect(page.getByRole('dialog')).toBeVisible();
      await expect(page.getByText(/asignaciones/i)).toBeVisible();
    });

    test('should show admin message for administrator users', async ({ page }) => {
      await page.goto('/users');

      const row = page.getByRole('row').filter({ hasText: 'admin' });
      const menuButton = row.locator('button');
      await menuButton.click();
      await page.getByRole('menuitem', { name: /puntos de venta/i }).click();

      await expect(page.getByText(/tienen acceso a todos los puntos de venta/i)).toBeVisible();
    });
  });

  test.describe('Access Control', () => {
    test('should show access denied for non-admin users', async ({ page }) => {
      // Override auth to return operator role
      await page.route('**/api/auth/me', async (route) => {
        await route.fulfill({
          status: 200,
          json: {
            userId: '2',
            username: 'operator1',
            firstName: 'Operator',
            lastName: 'User',
            role: 'Operator',
          },
        });
      });

      await page.goto('/users');

      // Should show access denied or redirect
      await expect(page.getByText(/acceso denegado|no tienes permisos/i)).toBeVisible();
    });
  });
});
