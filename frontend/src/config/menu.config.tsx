import {
  LayoutGrid,
  Package,
  Warehouse,
  ShoppingCart,
  RotateCcw,
  FileText,
  Settings,
  CreditCard,
  Users,
  Store,
} from 'lucide-react';
import { type MenuConfig } from './types';
import { ROUTES } from '@/routing/routes';

/**
 * Administrator Sidebar Menu
 * Full access to all modules including configuration and reports
 */
export const MENU_ADMINISTRATOR: MenuConfig = [
  {
    title: 'Dashboard',
    icon: LayoutGrid,
    path: ROUTES.DASHBOARD,
  },
  {
    title: 'Inventario',
    icon: Warehouse,
    path: ROUTES.INVENTORY,
  },
  {
    title: 'Ventas',
    icon: ShoppingCart,
    path: ROUTES.SALES,
  },
  {
    title: 'Devoluciones',
    icon: RotateCcw,
    path: ROUTES.RETURNS,
  },
  {
    title: 'Reportes',
    icon: FileText,
    path: ROUTES.REPORTS,
  },
  {
    title: 'Configuración',
    icon: Settings,
    children: [
      {
        title: 'Productos',
        icon: Package,
        path: ROUTES.PRODUCTS,
      },
      {
        title: 'Métodos de Pago',
        icon: CreditCard,
        path: ROUTES.PAYMENT_METHODS,
      },
      {
        title: 'Usuarios',
        icon: Users,
        path: ROUTES.USERS,
      },
      {
        title: 'Puntos de Venta',
        icon: Store,
        path: ROUTES.POINTS_OF_SALE,
      },
    ],
  },
];

/**
 * Operator Sidebar Menu
 * Limited access to sales-related functions only
 */
export const MENU_OPERATOR: MenuConfig = [
  {
    title: 'Dashboard',
    icon: LayoutGrid,
    path: ROUTES.DASHBOARD,
  },
  {
    title: 'Ventas',
    icon: ShoppingCart,
    path: ROUTES.SALES,
  },
  {
    title: 'Devoluciones',
    icon: RotateCcw,
    path: ROUTES.RETURNS,
  },
  {
    title: 'Inventario',
    icon: Warehouse,
    path: ROUTES.INVENTORY,
  },
];

/**
 * Get menu configuration based on user role
 * @param role - User role ('Administrator' or 'Operator')
 * @returns Menu configuration for the role
 */
export function getMenuForRole(role: 'Administrator' | 'Operator'): MenuConfig {
  switch (role) {
    case 'Administrator':
      return MENU_ADMINISTRATOR;
    case 'Operator':
      return MENU_OPERATOR;
    default:
      return MENU_OPERATOR;
  }
}
