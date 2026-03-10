/**
 * Cart Provider Tests
 * Tests for cart state management, constraints, persistence, and TTL expiration.
 */
import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { render, screen, act } from '@testing-library/react';
import { CartProvider, useCart } from '../cart-provider';

vi.mock('sonner', () => ({
  toast: {
    success: vi.fn(),
    error: vi.fn(),
    warning: vi.fn(),
  },
}));

const STORAGE_KEY = 'sales-cart';

function makeLine(overrides: Partial<Parameters<ReturnType<typeof useCart>['addLine']>[0]> = {}) {
  return {
    productId: 'prod-1',
    productSku: 'SKU-001',
    productName: 'Test Product',
    productPrice: 100,
    quantity: 2,
    pointOfSaleId: 'pos-1',
    pointOfSaleName: 'POS One',
    paymentMethodId: 'pm-1',
    paymentMethodName: 'Cash',
    ...overrides,
  };
}

let captured: ReturnType<typeof useCart>;

function CartTestConsumer({ onRender }: { onRender: (cart: ReturnType<typeof useCart>) => void }) {
  const cart = useCart();
  onRender(cart);
  return (
    <div>
      <span data-testid="line-count">{cart.lineCount}</span>
      <span data-testid="total">{cart.total}</span>
    </div>
  );
}

function renderCart() {
  return render(
    <CartProvider>
      <CartTestConsumer onRender={(c) => { captured = c; }} />
    </CartProvider>,
  );
}

describe('CartProvider', () => {
  beforeEach(() => {
    localStorage.clear();
    vi.clearAllMocks();
  });

  afterEach(() => {
    localStorage.clear();
  });

  it('adds a line to the cart', () => {
    renderCart();
    expect(captured.lineCount).toBe(0);

    act(() => {
      captured.addLine(makeLine());
    });

    expect(captured.lineCount).toBe(1);
    expect(captured.lines[0].productName).toBe('Test Product');
    expect(captured.lines[0].id).toBeDefined();
    expect(captured.lines[0].addedAt).toBeDefined();
  });

  it('rejects a line with a different pointOfSaleId', async () => {
    const { toast } = await import('sonner');
    renderCart();

    act(() => {
      captured.addLine(makeLine());
    });

    let result: boolean | undefined;
    act(() => {
      result = captured.addLine(makeLine({ pointOfSaleId: 'pos-2', pointOfSaleName: 'POS Two' }));
    });

    expect(result).toBe(false);
    expect(captured.lineCount).toBe(1);
    expect(toast.error).toHaveBeenCalledWith(
      expect.stringContaining('punto de venta'),
    );
  });

  it('rejects a line with a different paymentMethodId', async () => {
    const { toast } = await import('sonner');
    renderCart();

    act(() => {
      captured.addLine(makeLine());
    });

    let result: boolean | undefined;
    act(() => {
      result = captured.addLine(makeLine({ paymentMethodId: 'pm-2', paymentMethodName: 'Card' }));
    });

    expect(result).toBe(false);
    expect(captured.lineCount).toBe(1);
    expect(toast.error).toHaveBeenCalledWith(
      expect.stringContaining('método de pago'),
    );
  });

  it('removes a specific line by id', () => {
    renderCart();

    act(() => {
      captured.addLine(makeLine({ productId: 'prod-1', productName: 'Product A' }));
      captured.addLine(makeLine({ productId: 'prod-2', productName: 'Product B' }));
    });

    expect(captured.lineCount).toBe(2);
    const idToRemove = captured.lines[0].id;

    act(() => {
      captured.removeLine(idToRemove);
    });

    expect(captured.lineCount).toBe(1);
    expect(captured.lines[0].productName).toBe('Product B');
  });

  it('clears all lines', () => {
    renderCart();

    act(() => {
      captured.addLine(makeLine());
      captured.addLine(makeLine({ productId: 'prod-2', productName: 'Product B' }));
    });

    expect(captured.lineCount).toBe(2);

    act(() => {
      captured.clearCart();
    });

    expect(captured.lineCount).toBe(0);
    expect(localStorage.getItem(STORAGE_KEY)).toBeNull();
  });

  it('computes correct total from line prices and quantities', () => {
    renderCart();

    act(() => {
      captured.addLine(makeLine({ productPrice: 50, quantity: 3 }));
      captured.addLine(makeLine({ productId: 'prod-2', productPrice: 120, quantity: 1 }));
    });

    expect(captured.total).toBe(50 * 3 + 120 * 1);
  });

  it('uses override price for total when present', () => {
    renderCart();

    act(() => {
      captured.addLine(makeLine({ productPrice: 100, price: 80, quantity: 2 }));
    });

    expect(captured.total).toBe(80 * 2);
  });

  it('clears cart when data is stale (TTL expired)', () => {
    const expired = {
      lines: [
        {
          id: 'old-line',
          productId: 'prod-1',
          productSku: 'SKU-001',
          productName: 'Stale Product',
          productPrice: 50,
          quantity: 1,
          pointOfSaleId: 'pos-1',
          pointOfSaleName: 'POS One',
          paymentMethodId: 'pm-1',
          paymentMethodName: 'Cash',
          addedAt: new Date(Date.now() - 12 * 60 * 60 * 1000).toISOString(),
        },
      ],
      lastActivityAt: new Date(Date.now() - 11 * 60 * 60 * 1000).toISOString(),
    };
    localStorage.setItem(STORAGE_KEY, JSON.stringify(expired));

    renderCart();

    expect(captured.lineCount).toBe(0);
    expect(localStorage.getItem(STORAGE_KEY)).toBeNull();
  });

  it('persists cart across component remounts', () => {
    const { unmount } = renderCart();

    act(() => {
      captured.addLine(makeLine({ productName: 'Persisted Product' }));
    });

    expect(localStorage.getItem(STORAGE_KEY)).not.toBeNull();
    unmount();

    renderCart();

    expect(captured.lineCount).toBe(1);
    expect(captured.lines[0].productName).toBe('Persisted Product');
  });

  it('exposes pointOfSale and paymentMethod from first line', () => {
    renderCart();

    act(() => {
      captured.addLine(makeLine());
    });

    expect(captured.pointOfSaleId).toBe('pos-1');
    expect(captured.pointOfSaleName).toBe('POS One');
    expect(captured.paymentMethodId).toBe('pm-1');
    expect(captured.paymentMethodName).toBe('Cash');
  });

  it('returns nulls when cart is empty', () => {
    renderCart();

    expect(captured.pointOfSaleId).toBeNull();
    expect(captured.pointOfSaleName).toBeNull();
    expect(captured.paymentMethodId).toBeNull();
    expect(captured.paymentMethodName).toBeNull();
  });
});
