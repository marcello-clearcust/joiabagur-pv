/**
 * Sales Cart Provider
 * Manages cart state with localStorage persistence and 10-hour TTL expiration.
 */

import {
  createContext,
  useContext,
  useState,
  useEffect,
  useCallback,
  useMemo,
  type ReactNode,
} from 'react';
import { toast } from 'sonner';
import type { CartLine } from '@/types/sales.types';

const STORAGE_KEY = 'sales-cart';
const TTL_MS = 10 * 60 * 60 * 1000; // 10 hours

interface CartState {
  lines: CartLine[];
  lastActivityAt: string;
}

interface CartContextValue {
  lines: CartLine[];
  lineCount: number;
  total: number;
  pointOfSaleId: string | null;
  pointOfSaleName: string | null;
  paymentMethodId: string | null;
  paymentMethodName: string | null;
  addLine: (line: Omit<CartLine, 'id' | 'addedAt'>) => boolean;
  removeLine: (lineId: string) => void;
  clearCart: () => void;
}

const CartContext = createContext<CartContextValue | null>(null);

function loadCartState(): CartState | null {
  try {
    const raw = localStorage.getItem(STORAGE_KEY);
    if (!raw) return null;
    return JSON.parse(raw) as CartState;
  } catch {
    localStorage.removeItem(STORAGE_KEY);
    return null;
  }
}

function saveCartState(state: CartState): void {
  localStorage.setItem(STORAGE_KEY, JSON.stringify(state));
}

function isExpired(state: CartState): boolean {
  const elapsed = Date.now() - new Date(state.lastActivityAt).getTime();
  return elapsed > TTL_MS;
}

export function CartProvider({ children }: { children: ReactNode }) {
  const [lines, setLines] = useState<CartLine[]>(() => {
    const state = loadCartState();
    if (!state) return [];
    if (isExpired(state)) {
      localStorage.removeItem(STORAGE_KEY);
      return [];
    }
    return state.lines;
  });
  const [expired, setExpired] = useState(false);

  // Check for expiration on mount
  useEffect(() => {
    const state = loadCartState();
    if (state && state.lines.length > 0 && isExpired(state)) {
      setExpired(true);
      setLines([]);
      localStorage.removeItem(STORAGE_KEY);
    }
  }, []);

  // Show expiration toast once
  useEffect(() => {
    if (expired) {
      toast.warning('Tu carrito anterior expiró por inactividad y fue eliminado.');
      setExpired(false);
    }
  }, [expired]);

  // Persist to localStorage on every change
  useEffect(() => {
    if (lines.length === 0) {
      localStorage.removeItem(STORAGE_KEY);
      return;
    }
    saveCartState({ lines, lastActivityAt: new Date().toISOString() });
  }, [lines]);

  const addLine = useCallback(
    (line: Omit<CartLine, 'id' | 'addedAt'>): boolean => {
      // Enforce same POS and payment method across all lines
      const currentLines = lines;
      if (currentLines.length > 0) {
        const first = currentLines[0];
        if (line.pointOfSaleId !== first.pointOfSaleId) {
          toast.error(
            `No se puede agregar: el carrito usa el punto de venta "${first.pointOfSaleName}". Todas las líneas deben usar el mismo punto de venta.`,
          );
          return false;
        }
        if (line.paymentMethodId !== first.paymentMethodId) {
          toast.error(
            `No se puede agregar: el carrito usa el método de pago "${first.paymentMethodName}". Todas las líneas deben usar el mismo método de pago.`,
          );
          return false;
        }
      }

      const newLine: CartLine = {
        ...line,
        id: crypto.randomUUID(),
        addedAt: new Date().toISOString(),
      };
      setLines((prev) => [...prev, newLine]);
      return true;
    },
    [lines],
  );

  const removeLine = useCallback((lineId: string) => {
    setLines((prev) => prev.filter((l) => l.id !== lineId));
  }, []);

  const clearCart = useCallback(() => {
    setLines([]);
    localStorage.removeItem(STORAGE_KEY);
  }, []);

  const value = useMemo<CartContextValue>(() => {
    const firstLine = lines[0] ?? null;
    return {
      lines,
      lineCount: lines.length,
      total: lines.reduce(
        (sum, l) => sum + (l.price ?? l.productPrice) * l.quantity,
        0,
      ),
      pointOfSaleId: firstLine?.pointOfSaleId ?? null,
      pointOfSaleName: firstLine?.pointOfSaleName ?? null,
      paymentMethodId: firstLine?.paymentMethodId ?? null,
      paymentMethodName: firstLine?.paymentMethodName ?? null,
      addLine,
      removeLine,
      clearCart,
    };
  }, [lines, addLine, removeLine, clearCart]);

  return <CartContext.Provider value={value}>{children}</CartContext.Provider>;
}

export function useCart(): CartContextValue {
  const ctx = useContext(CartContext);
  if (!ctx) {
    throw new Error('useCart must be used within a CartProvider');
  }
  return ctx;
}
