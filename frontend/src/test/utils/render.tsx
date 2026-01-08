/**
 * Custom render utility for testing
 * Wraps components with all necessary providers (Router, Auth, Theme)
 * 
 * Query Priority (as per Testing Library best practices):
 * 1. getByRole - Accessible roles (button, heading, textbox, etc.)
 * 2. getByLabelText - Form fields with labels
 * 3. getByPlaceholderText - Form fields with placeholder text
 * 4. getByText - Non-interactive text content
 * 5. getByDisplayValue - Current value of form field
 * 6. getByAltText - Images with alt text
 * 7. getByTitle - Elements with title attribute
 * 8. getByTestId - Last resort when no accessible query available
 */

import { ReactElement, ReactNode } from 'react';
import { render, RenderOptions } from '@testing-library/react';
import { BrowserRouter } from 'react-router-dom';
import { ThemeProvider } from 'next-themes';
import { AuthProvider } from '@/providers/auth-provider';
import type { AuthUser } from '@/types/auth.types';

/**
 * Options for custom render with provider overrides
 */
interface CustomRenderOptions extends Omit<RenderOptions, 'wrapper'> {
  /**
   * Initial route for React Router
   * @default '/'
   */
  initialRoute?: string;
  
  /**
   * Mock authenticated user for AuthProvider
   * If null, user is not authenticated
   * @default null
   */
  authUser?: AuthUser | null;
  
  /**
   * Whether auth is in loading state
   * @default false
   */
  authLoading?: boolean;

  /**
   * Initial theme mode
   * @default 'light'
   */
  theme?: 'light' | 'dark';
}

/**
 * Wrapper component that includes all providers needed for testing
 */
function AllProviders({ 
  children, 
  initialRoute = '/',
  theme = 'light',
}: { 
  children: ReactNode;
  initialRoute?: string;
  theme?: string;
}) {
  // Set initial route if provided
  if (initialRoute !== '/') {
    window.history.pushState({}, '', initialRoute);
  }

  return (
    <BrowserRouter>
      <ThemeProvider attribute="class" defaultTheme={theme} enableSystem={false}>
        <AuthProvider>
          {children}
        </AuthProvider>
      </ThemeProvider>
    </BrowserRouter>
  );
}

/**
 * Custom render that wraps the component with all necessary providers
 * 
 * @example
 * ```tsx
 * // Basic usage
 * render(<MyComponent />);
 * 
 * // With authenticated user
 * render(<MyComponent />, {
 *   authUser: createMockUser({ role: 'Admin' })
 * });
 * 
 * // With initial route
 * render(<MyComponent />, {
 *   initialRoute: '/dashboard'
 * });
 * ```
 */
function customRender(
  ui: ReactElement,
  options?: CustomRenderOptions,
) {
  const {
    initialRoute = '/',
    theme = 'light',
    ...renderOptions
  } = options || {};

  return render(ui, { 
    wrapper: ({ children }) => (
      <AllProviders initialRoute={initialRoute} theme={theme}>
        {children}
      </AllProviders>
    ),
    ...renderOptions 
  });
}

// Re-export everything from testing-library
export * from '@testing-library/react';
export { userEvent } from '@testing-library/user-event';

// Override render with our custom version
export { customRender as render };
