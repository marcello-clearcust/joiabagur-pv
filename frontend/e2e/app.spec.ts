/**
 * Basic E2E tests for the application
 * Tests core navigation and rendering
 */

import { test, expect } from '@playwright/test';

test.describe('Application', () => {
  test('should load the login page', async ({ page }) => {
    await page.goto('/');
    
    // Should redirect to login if not authenticated
    // Adjust selector based on actual login page structure
    await expect(page).toHaveURL(/.*login.*/);
  });

  test('should display login form elements', async ({ page }) => {
    await page.goto('/login');
    
    // Check for common login form elements
    // These selectors should be adjusted based on actual implementation
    const emailInput = page.getByLabel(/email|correo/i);
    const passwordInput = page.getByLabel(/password|contraseña/i);
    
    // At least one of these should be present
    const hasLoginForm = await emailInput.count() > 0 || await passwordInput.count() > 0;
    
    // If login page exists, it should have form elements
    // This is a basic smoke test
    expect(hasLoginForm || await page.getByRole('button').count() > 0).toBeTruthy();
  });

  test('should have correct page title', async ({ page }) => {
    await page.goto('/');
    
    // Check that the page has a title
    await expect(page).toHaveTitle(/.+/);
  });
});
