/**
 * Component tests for ProductCreatePage
 * Tests form rendering, validation, and submission functionality
 */

import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { BrowserRouter } from 'react-router-dom';
import { ProductCreatePage } from './create';
import { productService } from '@/services/product.service';
import { Product, Collection } from '@/types/product.types';

// Make debounce synchronous for deterministic tests
vi.mock('@/hooks/use-debounced-callback', () => ({
  useDebouncedCallback: <T extends (...args: any[]) => any>(cb: T) => cb,
}));

// Mock the product service
vi.mock('@/services/product.service', () => ({
  productService: {
    createProduct: vi.fn(),
    getCollections: vi.fn(),
    checkSkuExists: vi.fn(),
  },
  default: {
    createProduct: vi.fn(),
    getCollections: vi.fn(),
    checkSkuExists: vi.fn(),
  },
}));

// Mock sonner toast
vi.mock('sonner', () => ({
  toast: {
    success: vi.fn(),
    error: vi.fn(),
    warning: vi.fn(),
    info: vi.fn(),
  },
}));

// Mock react-router-dom useNavigate
const mockNavigate = vi.fn();
vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual('react-router-dom');
  return {
    ...actual,
    useNavigate: () => mockNavigate,
  };
});

const mockCollections: Collection[] = [
  {
    id: 'col-1',
    name: 'Summer 2024',
    description: 'Summer collection',
    createdAt: '2024-01-01T00:00:00Z',
    updatedAt: '2024-01-01T00:00:00Z',
    productCount: 5,
  },
  {
    id: 'col-2',
    name: 'Winter 2024',
    description: 'Winter collection',
    createdAt: '2024-01-01T00:00:00Z',
    updatedAt: '2024-01-01T00:00:00Z',
    productCount: 3,
  },
];

const mockCreatedProduct: Product = {
  id: 'prod-1',
  sku: 'JOY-001',
  name: 'Gold Ring 18K',
  description: 'Beautiful gold ring',
  price: 299.99,
  collectionId: 'col-1',
  collectionName: 'Summer 2024',
  isActive: true,
  createdAt: '2024-01-01T00:00:00Z',
  updatedAt: '2024-01-01T00:00:00Z',
  photos: [],
};

function renderPage() {
  return render(
    <BrowserRouter>
      <ProductCreatePage />
    </BrowserRouter>
  );
}

describe('ProductCreatePage', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    vi.mocked(productService.getCollections).mockResolvedValue(mockCollections);
    vi.mocked(productService.checkSkuExists).mockResolvedValue(false);
  });

  describe('Rendering', () => {
    it('should render page title and description', async () => {
      renderPage();

      expect(screen.getByRole('heading', { name: /crear producto/i })).toBeInTheDocument();
      expect(screen.getByText(/añade un nuevo producto/i)).toBeInTheDocument();
    });

    it('should render all form fields', async () => {
      renderPage();

      // Wait for collections to load
      await waitFor(() => {
        expect(productService.getCollections).toHaveBeenCalled();
      });

      // Check for all form fields using labels
      expect(screen.getByLabelText(/sku/i)).toBeInTheDocument();
      expect(screen.getByLabelText(/nombre \*/i)).toBeInTheDocument();
      expect(screen.getByLabelText(/descripción/i)).toBeInTheDocument();
      expect(screen.getByLabelText(/precio/i)).toBeInTheDocument();
      expect(screen.getByLabelText(/colección/i)).toBeInTheDocument();
    });

    it('should render submit and cancel buttons', async () => {
      renderPage();

      expect(screen.getByRole('button', { name: /crear producto/i })).toBeInTheDocument();
      expect(screen.getByRole('button', { name: /cancelar/i })).toBeInTheDocument();
    });

    it('should render back navigation link', async () => {
      renderPage();

      const backLink = screen.getByRole('link', { name: /volver a productos/i });
      expect(backLink).toBeInTheDocument();
    });
  });

  describe('Collection Dropdown', () => {
    it('should load collections from API', async () => {
      renderPage();

      await waitFor(() => {
        expect(productService.getCollections).toHaveBeenCalled();
      });
    });

    it('should populate dropdown with collections', async () => {
      const user = userEvent.setup();
      renderPage();

      await waitFor(() => {
        expect(productService.getCollections).toHaveBeenCalled();
      });

      // Open the collection dropdown
      const collectionTrigger = screen.getByRole('combobox', { name: /colección/i });
      await user.click(collectionTrigger);

      // Check collections are in the dropdown
      await waitFor(() => {
        expect(screen.getByRole('option', { name: /summer 2024/i })).toBeInTheDocument();
        expect(screen.getByRole('option', { name: /winter 2024/i })).toBeInTheDocument();
      });
    });

    it('should have "Sin colección" option', async () => {
      const user = userEvent.setup();
      renderPage();

      await waitFor(() => {
        expect(productService.getCollections).toHaveBeenCalled();
      });

      // Open the collection dropdown
      const collectionTrigger = screen.getByRole('combobox', { name: /colección/i });
      await user.click(collectionTrigger);

      await waitFor(() => {
        expect(screen.getByRole('option', { name: /sin colección/i })).toBeInTheDocument();
      });
    });
  });

  describe('Required Field Validation', () => {
    it('should show validation error when SKU is empty', async () => {
      const user = userEvent.setup();
      renderPage();

      // Try to submit without filling required fields
      const skuInput = screen.getByLabelText(/sku/i);
      await user.click(skuInput);
      await user.tab(); // Blur the field

      await waitFor(() => {
        expect(screen.getByText(/el sku es requerido/i)).toBeInTheDocument();
      });
    });

    it('should show validation error when name is empty', async () => {
      const user = userEvent.setup();
      renderPage();

      const nameInput = screen.getByLabelText(/nombre \*/i);
      await user.click(nameInput);
      await user.tab();

      await waitFor(() => {
        expect(screen.getByText(/el nombre es requerido/i)).toBeInTheDocument();
      });
    });

    it('should show validation error when price is empty', async () => {
      const user = userEvent.setup();
      renderPage();

      const priceInput = screen.getByLabelText(/precio/i);
      await user.click(priceInput);
      await user.tab();

      await waitFor(() => {
        expect(screen.getByText(/el precio es requerido/i)).toBeInTheDocument();
      });
    });
  });

  describe('Price Validation', () => {
    it('should show error when price is zero', async () => {
      const user = userEvent.setup();
      renderPage();

      const priceInput = screen.getByLabelText(/precio/i);
      await user.type(priceInput, '0');
      await user.tab();

      await waitFor(() => {
        expect(screen.getByText(/el precio debe ser mayor que 0/i)).toBeInTheDocument();
      });
    });

    it('should show error when price is negative', async () => {
      const user = userEvent.setup();
      renderPage();

      const priceInput = screen.getByLabelText(/precio/i);
      await user.type(priceInput, '-10');
      await user.tab();

      await waitFor(() => {
        expect(screen.getByText(/el precio debe ser mayor que 0/i)).toBeInTheDocument();
      });
    });

    it('should accept valid positive price', async () => {
      const user = userEvent.setup();
      renderPage();

      const priceInput = screen.getByLabelText(/precio/i);
      await user.type(priceInput, '299.99');
      await user.tab();

      await waitFor(() => {
        expect(screen.queryByText(/el precio debe ser mayor que 0/i)).not.toBeInTheDocument();
      });
    });
  });

  describe('SKU Uniqueness Validation', () => {
    it('should check SKU uniqueness when typing', async () => {
      const user = userEvent.setup();
      vi.mocked(productService.checkSkuExists).mockResolvedValue(false);

      renderPage();

      const skuInput = screen.getByLabelText(/sku/i);
      await user.type(skuInput, 'JOY-001');

      await waitFor(() => {
        expect(productService.checkSkuExists).toHaveBeenCalledWith('JOY-001');
      });
    });

    it('should show error when SKU already exists', async () => {
      const user = userEvent.setup();
      vi.mocked(productService.checkSkuExists).mockResolvedValue(true);

      renderPage();

      const skuInput = screen.getByLabelText(/sku/i);
      await user.type(skuInput, 'EXISTING-SKU');

      await waitFor(() => {
        expect(screen.getByText(/el sku ya está en uso/i)).toBeInTheDocument();
      });
    });

    it('should show loading indicator while checking SKU', async () => {
      const user = userEvent.setup();
      // Create a promise that doesn't resolve immediately
      let resolveCheck: (value: boolean) => void;
      vi.mocked(productService.checkSkuExists).mockImplementation(
        () => new Promise((resolve) => { resolveCheck = resolve; })
      );

      renderPage();

      const skuInput = screen.getByLabelText(/sku/i);
      await user.type(skuInput, 'TEST-SKU');

      // Resolve the check (debounce is mocked to be immediate)
      resolveCheck!(false);

      await waitFor(() => {
        expect(productService.checkSkuExists).toHaveBeenCalled();
      });
    });
  });

  describe('Form Submission', () => {
    it('should call createProduct with form data on submit', async () => {
      const user = userEvent.setup();
      vi.mocked(productService.createProduct).mockResolvedValue(mockCreatedProduct);

      renderPage();

      await waitFor(() => {
        expect(productService.getCollections).toHaveBeenCalled();
      });

      // Fill in the form
      await user.type(screen.getByLabelText(/sku/i), 'JOY-001');
      await user.type(screen.getByLabelText(/nombre \*/i), 'Gold Ring 18K');
      await user.type(screen.getByLabelText(/descripción/i), 'Beautiful gold ring');
      await user.type(screen.getByLabelText(/precio/i), '299.99');

      // Submit the form
      await user.click(screen.getByRole('button', { name: /crear producto/i }));

      await waitFor(() => {
        expect(productService.createProduct).toHaveBeenCalledWith({
          sku: 'JOY-001',
          name: 'Gold Ring 18K',
          description: 'Beautiful gold ring',
          price: 299.99,
          collectionId: undefined,
        });
      });
    });

    it('should show success feedback after creation', async () => {
      const user = userEvent.setup();
      vi.mocked(productService.createProduct).mockResolvedValue(mockCreatedProduct);

      renderPage();

      await waitFor(() => {
        expect(productService.getCollections).toHaveBeenCalled();
      });

      // Fill in required fields
      await user.type(screen.getByLabelText(/sku/i), 'JOY-001');
      await user.type(screen.getByLabelText(/nombre \*/i), 'Gold Ring 18K');
      await user.type(screen.getByLabelText(/precio/i), '299.99');

      // Submit the form
      await user.click(screen.getByRole('button', { name: /crear producto/i }));

      await waitFor(() => {
        expect(screen.getByText(/producto creado exitosamente/i)).toBeInTheDocument();
      });
    });

    it('should show "Create Another" button after success', async () => {
      const user = userEvent.setup();
      vi.mocked(productService.createProduct).mockResolvedValue(mockCreatedProduct);

      renderPage();

      await waitFor(() => {
        expect(productService.getCollections).toHaveBeenCalled();
      });

      // Fill and submit
      await user.type(screen.getByLabelText(/sku/i), 'JOY-001');
      await user.type(screen.getByLabelText(/nombre \*/i), 'Gold Ring 18K');
      await user.type(screen.getByLabelText(/precio/i), '299.99');
      await user.click(screen.getByRole('button', { name: /crear producto/i }));

      await waitFor(() => {
        expect(screen.getByRole('button', { name: /crear otro producto/i })).toBeInTheDocument();
      });
    });

    it('should reset form when "Create Another" is clicked', async () => {
      const user = userEvent.setup();
      vi.mocked(productService.createProduct).mockResolvedValue(mockCreatedProduct);

      renderPage();

      await waitFor(() => {
        expect(productService.getCollections).toHaveBeenCalled();
      });

      // Fill and submit
      await user.type(screen.getByLabelText(/sku/i), 'JOY-001');
      await user.type(screen.getByLabelText(/nombre \*/i), 'Gold Ring 18K');
      await user.type(screen.getByLabelText(/precio/i), '299.99');
      await user.click(screen.getByRole('button', { name: /crear producto/i }));

      await waitFor(() => {
        expect(screen.getByRole('button', { name: /crear otro producto/i })).toBeInTheDocument();
      });

      // Click "Create Another"
      await user.click(screen.getByRole('button', { name: /crear otro producto/i }));

      // Form should be shown again with empty fields
      await waitFor(() => {
        expect(screen.getByLabelText(/sku/i)).toHaveValue('');
      });
    });
  });

  describe('API Error Handling', () => {
    it('should display error message when API fails', async () => {
      const user = userEvent.setup();
      vi.mocked(productService.createProduct).mockRejectedValue(new Error('Server error'));

      renderPage();

      await waitFor(() => {
        expect(productService.getCollections).toHaveBeenCalled();
      });

      // Fill and submit
      await user.type(screen.getByLabelText(/sku/i), 'JOY-001');
      await user.type(screen.getByLabelText(/nombre \*/i), 'Gold Ring 18K');
      await user.type(screen.getByLabelText(/precio/i), '299.99');
      await user.click(screen.getByRole('button', { name: /crear producto/i }));

      // Form should still be visible (not success screen)
      await waitFor(() => {
        expect(screen.getByLabelText(/sku/i)).toBeInTheDocument();
      });
    });

    it('should show SKU error when API returns conflict', async () => {
      const user = userEvent.setup();
      vi.mocked(productService.createProduct).mockRejectedValue(
        new Error('SKU already exists')
      );

      renderPage();

      await waitFor(() => {
        expect(productService.getCollections).toHaveBeenCalled();
      });

      // Fill and submit
      await user.type(screen.getByLabelText(/sku/i), 'EXISTING-SKU');
      await user.type(screen.getByLabelText(/nombre \*/i), 'Test Product');
      await user.type(screen.getByLabelText(/precio/i), '99.99');
      await user.click(screen.getByRole('button', { name: /crear producto/i }));

      await waitFor(() => {
        expect(screen.getByText(/el sku ya está en uso/i)).toBeInTheDocument();
      });
    });
  });

  describe('Loading States', () => {
    it('should disable form inputs during submission', async () => {
      const user = userEvent.setup();
      
      // Create a promise that doesn't resolve immediately
      let resolveCreate: (value: Product) => void;
      vi.mocked(productService.createProduct).mockImplementation(
        () => new Promise((resolve) => { resolveCreate = resolve; })
      );

      renderPage();

      await waitFor(() => {
        expect(productService.getCollections).toHaveBeenCalled();
      });

      // Fill and submit
      await user.type(screen.getByLabelText(/sku/i), 'JOY-001');
      await user.type(screen.getByLabelText(/nombre \*/i), 'Gold Ring 18K');
      await user.type(screen.getByLabelText(/precio/i), '299.99');
      await user.click(screen.getByRole('button', { name: /crear producto/i }));

      // Button should show loading state
      await waitFor(() => {
        expect(screen.getByText(/creando/i)).toBeInTheDocument();
      });

      // Inputs should be disabled
      expect(screen.getByLabelText(/sku/i)).toBeDisabled();
      expect(screen.getByLabelText(/nombre \*/i)).toBeDisabled();

      // Resolve the create
      resolveCreate!(mockCreatedProduct);

      await waitFor(() => {
        expect(screen.getByText(/producto creado exitosamente/i)).toBeInTheDocument();
      });
    });

    it('should show loading text in submit button', async () => {
      const user = userEvent.setup();
      
      let resolveCreate: (value: Product) => void;
      vi.mocked(productService.createProduct).mockImplementation(
        () => new Promise((resolve) => { resolveCreate = resolve; })
      );

      renderPage();

      await waitFor(() => {
        expect(productService.getCollections).toHaveBeenCalled();
      });

      await user.type(screen.getByLabelText(/sku/i), 'JOY-001');
      await user.type(screen.getByLabelText(/nombre \*/i), 'Gold Ring 18K');
      await user.type(screen.getByLabelText(/precio/i), '299.99');
      await user.click(screen.getByRole('button', { name: /crear producto/i }));

      await waitFor(() => {
        expect(screen.getByText(/creando/i)).toBeInTheDocument();
      });

      resolveCreate!(mockCreatedProduct);
    });
  });

  describe('Cancel Button', () => {
    it('should navigate to products page when cancel is clicked', async () => {
      const user = userEvent.setup();
      renderPage();

      await user.click(screen.getByRole('button', { name: /cancelar/i }));

      expect(mockNavigate).toHaveBeenCalledWith('/products');
    });
  });
});
