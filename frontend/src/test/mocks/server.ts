/**
 * MSW Server Setup
 * Configures the mock service worker for testing
 */

import { setupServer } from 'msw/node';
import { handlers } from './handlers';

/**
 * Mock server instance
 * Use in tests to intercept network requests
 */
export const server = setupServer(...handlers);
