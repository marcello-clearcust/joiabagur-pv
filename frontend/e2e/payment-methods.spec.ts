/**
 * E2E Tests for Payment Method Management
 * Tests payment method list, create, edit, and status toggle
 */

import { test, expect } from '@playwright/test';

// Helper to setup authenticated session
async function setupAuthenticatedSession(page: import('@playwright/test').Page) {
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

const mockPaymentMethods = [
  {
    id: '1',
    code: 'CASH',
    name: 'Efectivo',
    description: 'Pago en efectivo',
    isActive: true,
    createdAt: '2024-01-01T00:00:00Z',
    updatedAt: '2024-01-01T00:00:00Z',
  },
  {
    id: '2',
    code: 'BIZUM',
    name: 'Bizum',
    description: 'Pago mediante Bizum',
    isActive: true,
    createdAt: '2024-01-01T00:00:00Z',
    updatedAt: '2024-01-01T00:00:00Z',
  },
  {
    id: '3',
    code: 'TRANSFER',
    name: 'Transferencia bancaria',
    description: 'Pago por transferencia',
    isActive: true,
    createdAt: '2024-01-01T00:00:00Z',
    updatedAt: '2024-01-01T00:00:00Z',
  },
  {
    id: '4',
    code: 'CARD_OWN',
    name: 'Tarjeta TPV propio',
    description: 'Pago con tarjeta en TPV propio',
    isActive: true,
    createdAt: '2024-01-01T00:00:00Z',
    updatedAt: '2024-01-01T00:00:00Z',
  },
  {
    id: '5',
    code: 'CARD_POS',
    name: 'Tarjeta TPV punto de venta',
    description: 'Pago con tarjeta en TPV del punto de venta',
    isActive: false,
    createdAt: '2024-01-01T00:00:00Z',
    updatedAt: '2024-01-01T00:00:00Z',
  },
  {
    id: '6',
    code: 'PAYPAL',
    name: 'PayPal',
    description: 'Pago mediante PayPal',
    isActive: true,
    createdAt: '2024-01-01T00:00:00Z',
    updatedAt: '2024-01-01T00:00:00Z',
  },
];

test.describe('Payment Method Management', () => {
  test.beforeEach(async ({ page }) => {
    await setupAuthenticatedSession(page);

    // Mock payment methods list endpoint
    await page.route('**/api/payment-methods*', async (route) => {
      const url = route.request().url();
      const includeInactive = url.includes('includeInactive=true');

      if (route.request().method() === 'GET') {
        const methods = includeInactive
          ? mockPaymentMethods
          : mockPaymentMethods.filter((pm) => pm.isActive);

        await route.fulfill({ status: 200, json: methods });
      } else {
        await route.continue();
      }
    });
  });

  test.describe('Payment Methods List', () => {
    test('should display payment methods page', async ({ page }) => {
      await page.goto('/payment-methods');

      await expect(page.getByRole('heading', { name: /métodos de pago/i })).toBeVisible();
      await expect(page.getByRole('button', { name: /nuevo método de pago/i })).toBeVisible();
    });

    test('should display page description', async ({ page }) => {
      await page.goto('/payment-methods');

      await expect(
        page.getByText('Configuración de métodos de pago disponibles en el sistema')
      ).toBeVisible();
    });

    test('should display all active payment methods in the table', async ({ page }) => {
      await page.goto('/payment-methods');

      // Should show all 6 predefined methods (5 active by default)
      await expect(page.getByText('CASH')).toBeVisible();
      await expect(page.getByText('BIZUM')).toBeVisible();
      await expect(page.getByText('TRANSFER')).toBeVisible();
      await expect(page.getByText('CARD_OWN')).toBeVisible();
      await expect(page.getByText('PAYPAL')).toBeVisible();

      // Should not show inactive method
      await expect(page.getByText('CARD_POS')).not.toBeVisible();
    });

    test('should display payment method names', async ({ page }) => {
      await page.goto('/payment-methods');

      await expect(page.getByText('Efectivo')).toBeVisible();
      await expect(page.getByText('Bizum')).toBeVisible();
      await expect(page.getByText('Transferencia bancaria')).toBeVisible();
    });

    test('should display payment method descriptions', async ({ page }) => {
      await page.goto('/payment-methods');

      await expect(page.getByText('Pago en efectivo')).toBeVisible();
      await expect(page.getByText('Pago mediante Bizum')).toBeVisible();
    });

    test('should display correct count of active payment methods', async ({ page }) => {
      await page.goto('/payment-methods');

      await expect(page.getByText('5 métodos de pago')).toBeVisible();
    });

    test('should display info banner about predefined methods', async ({ page }) => {
      await page.goto('/payment-methods');

      await expect(page.getByText('Métodos de pago predefinidos')).toBeVisible();
      await expect(page.getByText(/El sistema incluye 6 métodos de pago predefinidos/i)).toBeVisible();
    });
  });

  test.describe('Show Inactive Toggle', () => {
    test('should show inactive toggle switch', async ({ page }) => {
      await page.goto('/payment-methods');

      await expect(page.getByText('Mostrar inactivos')).toBeVisible();
      const toggle = page.locator('[role="switch"]');
      await expect(toggle).toBeVisible();
    });

    test('should display inactive payment methods when toggle is enabled', async ({ page }) => {
      await page.goto('/payment-methods');

      // Initially, inactive method should not be visible
      await expect(page.getByText('CARD_POS')).not.toBeVisible();

      // Click toggle to show inactive
      const toggle = page.locator('[role="switch"]');
      await toggle.click();

      // Now inactive method should be visible
      await expect(page.getByText('CARD_POS')).toBeVisible();
      await expect(page.getByText('Tarjeta TPV punto de venta')).toBeVisible();

      // Count should include all 6 methods
      await expect(page.getByText('6 métodos de pago')).toBeVisible();
    });
  });

  test.describe('Payment Method Status', () => {
    test('should display active status with green indicator', async ({ page }) => {
      await page.goto('/payment-methods');

      // Active payment methods should show green "Activo" badge
      const activeStatuses = page.getByText('Activo');
      await expect(activeStatuses.first()).toBeVisible();
    });

    test('should display inactive status with gray indicator', async ({ page }) => {
      await page.goto('/payment-methods');

      // Enable showing inactive
      const toggle = page.locator('[role="switch"]');
      await toggle.click();

      // Inactive payment method should show gray "Inactivo" badge
      await expect(page.getByText('Inactivo')).toBeVisible();
    });
  });

  test.describe('Create Payment Method', () => {
    test('should open create dialog when clicking new button', async ({ page }) => {
      await page.goto('/payment-methods');

      const createButton = page.getByRole('button', { name: /nuevo método de pago/i });
      await createButton.click();

      // Dialog should open
      await expect(page.getByRole('dialog')).toBeVisible();
      await expect(page.getByText('Crear Método de Pago')).toBeVisible();
    });

    test('should display empty form fields in create dialog', async ({ page }) => {
      await page.goto('/payment-methods');

      await page.getByRole('button', { name: /nuevo método de pago/i }).click();

      // Check form fields are present and empty
      await expect(page.getByLabel(/código/i)).toBeVisible();
      await expect(page.getByLabel(/nombre/i)).toBeVisible();
      await expect(page.getByLabel(/descripción/i)).toBeVisible();

      // Code field should be enabled in create mode
      await expect(page.getByLabel(/código/i)).not.toBeDisabled();
    });

    test('should create new payment method successfully', async ({ page }) => {
      await page.goto('/payment-methods');

      // Mock create endpoint
      await page.route('**/api/payment-methods', async (route) => {
        if (route.request().method() === 'POST') {
          const body = route.request().postDataJSON();
          await route.fulfill({
            status: 201,
            json: {
              id: '7',
              ...body,
              isActive: true,
              createdAt: '2024-01-01T00:00:00Z',
              updatedAt: '2024-01-01T00:00:00Z',
            },
          });
        } else {
          await route.continue();
        }
      });

      // Open create dialog
      await page.getByRole('button', { name: /nuevo método de pago/i }).click();

      // Fill in form
      await page.getByLabel(/código/i).fill('NEW_METHOD');
      await page.getByLabel(/nombre/i).fill('Nuevo Método');
      await page.getByLabel(/descripción/i).fill('Método de prueba');

      // Submit form
      await page.getByRole('button', { name: /^crear$/i }).click();

      // Should show success message
      await expect(page.getByText('Método de pago creado correctamente')).toBeVisible();
    });

    test('should validate code format', async ({ page }) => {
      await page.goto('/payment-methods');

      await page.getByRole('button', { name: /nuevo método de pago/i }).click();

      // Try to enter lowercase code (should be converted to uppercase)
      await page.getByLabel(/código/i).fill('lowercase_code');
      await expect(page.getByLabel(/código/i)).toHaveValue('LOWERCASE_CODE');
    });
  });

  test.describe('Edit Payment Method', () => {
    test('should open edit dialog when clicking edit action', async ({ page }) => {
      await page.goto('/payment-methods');

      // Click on first payment method's action menu
      const actionButtons = page.getByRole('button', { name: /abrir menú/i });
      await actionButtons.first().click();

      // Click edit option
      await page.getByRole('menuitem', { name: /editar/i }).click();

      // Dialog should open in edit mode
      await expect(page.getByRole('dialog')).toBeVisible();
      await expect(page.getByText('Editar Método de Pago')).toBeVisible();
    });

    test('should display prepopulated form fields in edit dialog', async ({ page }) => {
      await page.goto('/payment-methods');

      // Open action menu and click edit
      await page.getByRole('button', { name: /abrir menú/i }).first().click();
      await page.getByRole('menuitem', { name: /editar/i }).click();

      // Form should be prepopulated with CASH data
      await expect(page.getByLabel(/código/i)).toHaveValue('CASH');
      await expect(page.getByLabel(/nombre/i)).toHaveValue('Efectivo');

      // Code field should be disabled in edit mode
      await expect(page.getByLabel(/código/i)).toBeDisabled();
    });

    test('should update payment method successfully', async ({ page }) => {
      await page.goto('/payment-methods');

      // Mock update endpoint
      await page.route('**/api/payment-methods/*', async (route) => {
        if (route.request().method() === 'PUT') {
          const body = route.request().postDataJSON();
          await route.fulfill({
            status: 200,
            json: {
              ...mockPaymentMethods[0],
              ...body,
              updatedAt: '2024-01-02T00:00:00Z',
            },
          });
        } else {
          await route.continue();
        }
      });

      // Open edit dialog
      await page.getByRole('button', { name: /abrir menú/i }).first().click();
      await page.getByRole('menuitem', { name: /editar/i }).click();

      // Update name
      await page.getByLabel(/nombre/i).fill('Efectivo Actualizado');

      // Submit form
      await page.getByRole('button', { name: /actualizar/i }).click();

      // Should show success message
      await expect(page.getByText('Método de pago actualizado correctamente')).toBeVisible();
    });
  });

  test.describe('Toggle Payment Method Status', () => {
    test('should display activate/deactivate option in actions menu', async ({ page }) => {
      await page.goto('/payment-methods');

      // Open action menu
      await page.getByRole('button', { name: /abrir menú/i }).first().click();

      // Should show deactivate option for active payment method
      await expect(page.getByRole('menuitem', { name: /desactivar/i })).toBeVisible();
    });

    test('should deactivate active payment method', async ({ page }) => {
      await page.goto('/payment-methods');

      // Mock status change endpoint
      await page.route('**/api/payment-methods/*/status', async (route) => {
        if (route.request().method() === 'PATCH') {
          await route.fulfill({
            status: 200,
            json: {
              ...mockPaymentMethods[0],
              isActive: false,
              updatedAt: '2024-01-02T00:00:00Z',
            },
          });
        } else {
          await route.continue();
        }
      });

      // Open action menu and deactivate
      await page.getByRole('button', { name: /abrir menú/i }).first().click();
      await page.getByRole('menuitem', { name: /desactivar/i }).click();

      // Should show success message
      await expect(page.getByText('Método de pago desactivado correctamente')).toBeVisible();
    });

    test('should activate inactive payment method', async ({ page }) => {
      await page.goto('/payment-methods');

      // Show inactive payment methods
      await page.locator('[role="switch"]').click();

      // Mock status change endpoint
      await page.route('**/api/payment-methods/*/status', async (route) => {
        if (route.request().method() === 'PATCH') {
          await route.fulfill({
            status: 200,
            json: {
              ...mockPaymentMethods[4],
              isActive: true,
              updatedAt: '2024-01-02T00:00:00Z',
            },
          });
        } else {
          await route.continue();
        }
      });

      // Find CARD_POS and activate it
      // Count buttons - the inactive one should be the 5th row
      const actionButtons = page.getByRole('button', { name: /abrir menú/i });
      await actionButtons.nth(4).click();
      
      await page.getByRole('menuitem', { name: /activar/i }).click();

      // Should show success message
      await expect(page.getByText('Método de pago activado correctamente')).toBeVisible();
    });
  });

  test.describe('Error Handling', () => {
    test('should display error when payment methods fail to load', async ({ page }) => {
      // Override route to simulate error
      await page.route('**/api/payment-methods*', async (route) => {
        await route.fulfill({ status: 500, json: { error: 'Server error' } });
      });

      await page.goto('/payment-methods');

      await expect(page.getByText('Error al cargar los métodos de pago')).toBeVisible();
    });

    test('should display error when create fails', async ({ page }) => {
      await page.goto('/payment-methods');

      // Mock create endpoint with error
      await page.route('**/api/payment-methods', async (route) => {
        if (route.request().method() === 'POST') {
          await route.fulfill({
            status: 409,
            json: { error: 'El código de método de pago ya está en uso' },
          });
        } else {
          await route.continue();
        }
      });

      // Open create dialog and fill form
      await page.getByRole('button', { name: /nuevo método de pago/i }).click();
      await page.getByLabel(/código/i).fill('CASH');
      await page.getByLabel(/nombre/i).fill('Duplicate');
      await page.getByRole('button', { name: /^crear$/i }).click();

      // Should show error message
      await expect(page.getByText('El código de método de pago ya está en uso')).toBeVisible();
    });
  });
});
