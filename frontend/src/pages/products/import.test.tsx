/**
 * Component tests for ProductImportPage
 * Tests file upload, validation, and import functionality
 */

import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { BrowserRouter } from 'react-router-dom';
import { ProductImportPage } from './import';
import { productService } from '@/services/product.service';
import { ImportResult } from '@/types/product.types';

// Mock the product service
vi.mock('@/services/product.service', () => ({
  productService: {
    validateImport: vi.fn(),
    importProducts: vi.fn(),
  },
  default: {
    validateImport: vi.fn(),
    importProducts: vi.fn(),
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

const mockValidationSuccess: ImportResult = {
  success: true,
  totalRows: 5,
  createdCount: 0,
  updatedCount: 0,
  collectionsCreatedCount: 0,
  errors: [],
  warnings: [],
  message: 'Validation passed for 5 rows.',
};

const mockValidationError: ImportResult = {
  success: false,
  totalRows: 3,
  createdCount: 0,
  updatedCount: 0,
  collectionsCreatedCount: 0,
  errors: [
    { rowNumber: 2, field: 'SKU', message: 'SKU duplicado en el archivo: JOY-001', value: 'JOY-001' },
    { rowNumber: 3, field: 'Price', message: 'Price must be greater than zero', value: '0' },
  ],
  warnings: [],
  message: 'Found 2 validation error(s).',
};

const mockImportSuccess: ImportResult = {
  success: true,
  totalRows: 5,
  createdCount: 3,
  updatedCount: 2,
  collectionsCreatedCount: 1,
  errors: [],
  warnings: [],
  message: 'Import successful: 3 created, 2 updated.',
};

function createMockFile(name = 'products.xlsx', size = 1024): File {
  const content = new ArrayBuffer(size);
  return new File([content], name, {
    type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
  });
}

function renderPage() {
  return render(
    <BrowserRouter>
      <ProductImportPage />
    </BrowserRouter>
  );
}

describe('ProductImportPage', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  describe('Rendering', () => {
    it('should render page title and description', () => {
      renderPage();

      expect(screen.getByRole('heading', { name: /importar productos/i })).toBeInTheDocument();
      expect(screen.getByText(/carga productos de forma masiva/i)).toBeInTheDocument();
    });

    it('should render download template button', () => {
      renderPage();

      expect(screen.getByRole('button', { name: /descargar plantilla/i })).toBeInTheDocument();
    });

    it('should render file drop zone', () => {
      renderPage();

      // There may be multiple matching texts (in card description and drop zone)
      const dropZoneTexts = screen.getAllByText(/arrastra/i);
      expect(dropZoneTexts.length).toBeGreaterThan(0);
    });
  });

  describe('File Upload', () => {
    it('should show file name after selecting a file', async () => {
      const user = userEvent.setup();
      vi.mocked(productService.validateImport).mockResolvedValue(mockValidationSuccess);

      renderPage();

      const file = createMockFile('test-products.xlsx');
      const input = document.querySelector('input[type="file"]') as HTMLInputElement;

      await user.upload(input, file);

      await waitFor(() => {
        expect(screen.getByText('test-products.xlsx')).toBeInTheDocument();
      });
    });

    it('should call validateImport when file is selected', async () => {
      const user = userEvent.setup();
      vi.mocked(productService.validateImport).mockResolvedValue(mockValidationSuccess);

      renderPage();

      const file = createMockFile();
      const input = document.querySelector('input[type="file"]') as HTMLInputElement;

      await user.upload(input, file);

      await waitFor(() => {
        expect(productService.validateImport).toHaveBeenCalledWith(file);
      });
    });

    it('should show validation success message', async () => {
      const user = userEvent.setup();
      vi.mocked(productService.validateImport).mockResolvedValue(mockValidationSuccess);

      renderPage();

      const file = createMockFile();
      const input = document.querySelector('input[type="file"]') as HTMLInputElement;

      await user.upload(input, file);

      await waitFor(() => {
        expect(screen.getByText(/validación exitosa/i)).toBeInTheDocument();
        expect(screen.getByText(/5 filas encontradas/i)).toBeInTheDocument();
      });
    });

    it('should show import button when validation succeeds', async () => {
      const user = userEvent.setup();
      vi.mocked(productService.validateImport).mockResolvedValue(mockValidationSuccess);

      renderPage();

      const file = createMockFile();
      const input = document.querySelector('input[type="file"]') as HTMLInputElement;

      await user.upload(input, file);

      await waitFor(() => {
        expect(screen.getByRole('button', { name: /importar productos/i })).toBeInTheDocument();
      });
    });
  });

  describe('Validation Errors', () => {
    it('should display validation errors', async () => {
      const user = userEvent.setup();
      vi.mocked(productService.validateImport).mockResolvedValue(mockValidationError);

      renderPage();

      const file = createMockFile();
      const input = document.querySelector('input[type="file"]') as HTMLInputElement;

      await user.upload(input, file);

      await waitFor(() => {
        expect(screen.getByText(/errores de validación/i)).toBeInTheDocument();
        expect(screen.getByText(/sku duplicado/i)).toBeInTheDocument();
        expect(screen.getByText(/price must be greater than zero/i)).toBeInTheDocument();
      });
    });

    it('should not show import button when validation fails', async () => {
      const user = userEvent.setup();
      vi.mocked(productService.validateImport).mockResolvedValue(mockValidationError);

      renderPage();

      const file = createMockFile();
      const input = document.querySelector('input[type="file"]') as HTMLInputElement;

      await user.upload(input, file);

      await waitFor(() => {
        expect(screen.queryByRole('button', { name: /importar productos/i })).not.toBeInTheDocument();
      });
    });
  });

  describe('Import Process', () => {
    it('should show confirmation dialog when import button is clicked', async () => {
      const user = userEvent.setup();
      vi.mocked(productService.validateImport).mockResolvedValue(mockValidationSuccess);

      renderPage();

      const file = createMockFile();
      const input = document.querySelector('input[type="file"]') as HTMLInputElement;

      await user.upload(input, file);

      await waitFor(() => {
        expect(screen.getByRole('button', { name: /importar productos/i })).toBeInTheDocument();
      });

      await user.click(screen.getByRole('button', { name: /importar productos/i }));

      // The dialog should be visible with confirmation text
      expect(screen.getByRole('dialog')).toBeInTheDocument();
      expect(screen.getByText(/¿estás seguro/i)).toBeInTheDocument();
    });

    it('should call importProducts when confirmed', async () => {
      const user = userEvent.setup();
      vi.mocked(productService.validateImport).mockResolvedValue(mockValidationSuccess);
      vi.mocked(productService.importProducts).mockResolvedValue(mockImportSuccess);

      renderPage();

      const file = createMockFile();
      const input = document.querySelector('input[type="file"]') as HTMLInputElement;

      await user.upload(input, file);

      await waitFor(() => {
        expect(screen.getByRole('button', { name: /importar productos/i })).toBeInTheDocument();
      });

      await user.click(screen.getByRole('button', { name: /importar productos/i }));
      
      // Find the dialog and click confirm button inside it
      const dialog = screen.getByRole('dialog');
      const confirmButton = within(dialog).getByRole('button', { name: /confirmar/i });
      await user.click(confirmButton);

      await waitFor(() => {
        expect(productService.importProducts).toHaveBeenCalledWith(file);
      });
    });

    it('should show success results after import', async () => {
      const user = userEvent.setup();
      vi.mocked(productService.validateImport).mockResolvedValue(mockValidationSuccess);
      vi.mocked(productService.importProducts).mockResolvedValue(mockImportSuccess);

      renderPage();

      const file = createMockFile();
      const input = document.querySelector('input[type="file"]') as HTMLInputElement;

      await user.upload(input, file);

      await waitFor(() => {
        expect(screen.getByRole('button', { name: /importar productos/i })).toBeInTheDocument();
      });

      await user.click(screen.getByRole('button', { name: /importar productos/i }));
      
      // Find the dialog and click confirm button
      const dialog = screen.getByRole('dialog');
      const confirmButton = within(dialog).getByRole('button', { name: /confirmar/i });
      await user.click(confirmButton);

      await waitFor(() => {
        expect(screen.getByText(/importación completada/i)).toBeInTheDocument();
      });

      // Check for result counts (look for text containing the numbers)
      expect(screen.getByText(/productos creados/i)).toBeInTheDocument();
      expect(screen.getByText(/productos actualizados/i)).toBeInTheDocument();
    });
  });

  describe('Clear File', () => {
    it('should clear file when clear button is clicked', async () => {
      const user = userEvent.setup();
      vi.mocked(productService.validateImport).mockResolvedValue(mockValidationSuccess);

      renderPage();

      const file = createMockFile('test-file.xlsx');
      const input = document.querySelector('input[type="file"]') as HTMLInputElement;

      await user.upload(input, file);

      await waitFor(() => {
        expect(screen.getByText('test-file.xlsx')).toBeInTheDocument();
      });

      // Click the clear button
      const clearButton = screen.getByRole('button', { name: /quitar archivo/i });
      await user.click(clearButton);

      expect(screen.queryByText('test-file.xlsx')).not.toBeInTheDocument();
      // Check the drop zone is visible again (use getAllByText and check first element)
      const dropZoneTexts = screen.getAllByText(/arrastra y suelta/i);
      expect(dropZoneTexts.length).toBeGreaterThan(0);
    });
  });
});

