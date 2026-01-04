import { useMemo } from 'react';
import { type MenuConfig } from '@/config/types';
import { MENU_ADMINISTRATOR, MENU_OPERATOR, getMenuForRole } from '@/config/menu.config';
import { useAuth } from '@/providers';

/**
 * Hook to get the appropriate menu based on user role
 *
 * Returns the menu configuration for the authenticated user's role.
 * Defaults to Operator menu if role is not available.
 *
 * @returns Menu configuration for the current user's role
 */
export function useRoleMenu(): MenuConfig {
  const { user } = useAuth();
  const role = user?.role ?? 'Operator';

  const menu = useMemo(() => {
    return getMenuForRole(role);
  }, [role]);

  return menu;
}

/**
 * Export menu constants for direct access when needed
 */
export { MENU_ADMINISTRATOR, MENU_OPERATOR };
