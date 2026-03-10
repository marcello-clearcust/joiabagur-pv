/**
 * Sales Cart Page Tests
 * Tests for the cart page rendering, line management, and checkout flow.
 */
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { BrowserRouter } from 'react-router-dom';
import { SalesCartPage } from '../cart';

vi.mock('@/services/sales.service');
vi.mock('@/services/inventory.service');

vi.mock('sonner', () => ({
  toast: {
    success: vi.fn(),
    error: vi.fn(),
    warning: vi.fn(),
  },
}));

const mockCartState = {
  lines: [] as any[],
  lineCount: 0,
  total: 0,
  pointOfSaleId: null as string | null,
  pointOfSaleName: null as string | null,
  paymentMethodId: null as string | null,
  paymentMethodName: null as string | null,
  addLine: vi.fn(),
  removeLine: vi.fn(),
  clearCart: vi.fn(),
};

vi.mock('@/providers/cart-provider', () => ({
  CartProvider: ({ children }: { children: React.ReactNode }) => <div>{children}</div>,
  useCart: () => mockCartState,
}));

function renderPage() {
  return render(
    <BrowserRouter>
      <SalesCartPage />
    </BrowserRouter>,
  );
}

function resetCartState() {
  mockCartState.lines = [];
  mockCartState.lineCount = 0;
  mockCartState.total = 0;
  mockCartState.pointOfSaleId = null;
  mockCartState.pointOfSaleName = null;
  mockCartState.paymentMethodId = null;
  mockCartState.paymentMethodName = null;
  mockCartState.addLine = vi.fn();
  mockCartState.removeLine = vi.fn();
  mockCartState.clearCart = vi.fn();
}

function populateCart() {
  mockCartState.lines = [
    {
      id: 'line-1',
      productId: 'prod-1',
      productSku: 'SKU-001',
      productName: 'Test Product 1',
      productPrice: 100,
      quantity: 2,
      pointOfSaleId: 'pos-1',
      pointOfSaleName: 'Test POS',
      paymentMethodId: 'pm-1',
      paymentMethodName: 'Cash',
      addedAt: new Date().toISOString(),
    },
  ];
  mockCartState.lineCount = 1;
  mockCartState.total = 200;
  mockCartState.pointOfSaleId = 'pos-1';
  mockCartState.pointOfSaleName = 'Test POS';
  mockCartState.paymentMethodId = 'pm-1';
  mockCartState.paymentMethodName = 'Cash';
}

describe('SalesCartPage', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    resetCartState();
  });

  it('renders empty state when cart has no lines', () => {
    renderPage();

    expect(screen.getByText('No hay productos en el carrito')).toBeInTheDocument();
    expect(screen.getByText('Registrar Venta')).toBeInTheDocument();
  });

  it('renders line items with product names, prices, and quantities', async () => {
    const { inventoryService } = await import('@/services/inventory.service');
    vi.mocked(inventoryService.getProductStockBreakdown).mockResolvedValue({
      productId: 'prod-1',
      productSku: 'SKU-001',
      productName: 'Test Product 1',
      totalQuantity: 10,
      breakdown: [{ pointOfSaleId: 'pos-1', pointOfSaleName: 'Test POS', quantity: 10 }],
    });

    populateCart();
    renderPage();

    expect(screen.getByText('Test Product 1')).toBeInTheDocument();
    expect(screen.getByText('SKU: SKU-001')).toBeInTheDocument();
    expect(screen.getByText('x2')).toBeInTheDocument();
    expect(screen.getByText('€100.00 c/u')).toBeInTheDocument();
    expect(screen.getAllByText('€200.00').length).toBeGreaterThanOrEqual(1);
  });

  it('calls removeLine when the delete button is clicked', async () => {
    const user = userEvent.setup();
    const { inventoryService } = await import('@/services/inventory.service');
    vi.mocked(inventoryService.getProductStockBreakdown).mockResolvedValue({
      productId: 'prod-1',
      productSku: 'SKU-001',
      productName: 'Test Product 1',
      totalQuantity: 10,
      breakdown: [{ pointOfSaleId: 'pos-1', pointOfSaleName: 'Test POS', quantity: 10 }],
    });

    populateCart();
    renderPage();

    const deleteButtons = screen.getAllByRole('button').filter(
      (btn) => btn.querySelector('svg.lucide-trash-2') !== null || btn.classList.contains('text-destructive'),
    );

    expect(deleteButtons.length).toBeGreaterThan(0);
    await user.click(deleteButtons[0]);

    expect(mockCartState.removeLine).toHaveBeenCalledWith('line-1');
  });

  it('disables checkout button when stock is insufficient', async () => {
    const { inventoryService } = await import('@/services/inventory.service');
    vi.mocked(inventoryService.getProductStockBreakdown).mockResolvedValue({
      productId: 'prod-1',
      productSku: 'SKU-001',
      productName: 'Test Product 1',
      totalQuantity: 1,
      breakdown: [{ pointOfSaleId: 'pos-1', pointOfSaleName: 'Test POS', quantity: 1 }],
    });

    populateCart();
    renderPage();

    await waitFor(() => {
      expect(screen.getByText('Stock insuficiente')).toBeInTheDocument();
    });

    const checkoutButton = screen.getByRole('button', { name: /realizar checkout/i });
    expect(checkoutButton).toBeDisabled();
  });

  it('renders checkout summary with point of sale and payment method', async () => {
    const { inventoryService } = await import('@/services/inventory.service');
    vi.mocked(inventoryService.getProductStockBreakdown).mockResolvedValue({
      productId: 'prod-1',
      productSku: 'SKU-001',
      productName: 'Test Product 1',
      totalQuantity: 10,
      breakdown: [{ pointOfSaleId: 'pos-1', pointOfSaleName: 'Test POS', quantity: 10 }],
    });

    populateCart();
    renderPage();

    expect(screen.getByText('Resumen')).toBeInTheDocument();
    expect(screen.getAllByText('Test POS').length).toBeGreaterThan(0);
    expect(screen.getAllByText('Cash').length).toBeGreaterThan(0);
  });
});
