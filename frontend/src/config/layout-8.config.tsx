import { HelpCircle } from 'lucide-react';
import { type MenuConfig } from './types';

// Re-export role-based menus for backwards compatibility
export { MENU_ADMINISTRATOR as MENU_SIDEBAR } from './menu.config';

/**
 * Help Menu - Minimal help link
 */
export const MENU_HELP: MenuConfig = [
  {
    title: 'Ayuda',
    icon: HelpCircle,
    path: '#',
  },
];
