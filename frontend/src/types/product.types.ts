/**
 * Product type definitions
 */

/**
 * Product DTO
 */
export interface Product {
  id: string;
  sku: string;
  name: string;
  description?: string;
  price: number;
  collectionId?: string;
  collectionName?: string;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
  photos: ProductPhoto[];
}

/**
 * Product photo DTO
 */
export interface ProductPhoto {
  id: string;
  fileName: string;
  url?: string;
  displayOrder: number;
  isPrimary: boolean;
}

/**
 * Collection DTO
 */
export interface Collection {
  id: string;
  name: string;
  description?: string;
  createdAt: string;
  updatedAt: string;
  productCount: number;
}

/**
 * Create product request
 */
export interface CreateProductRequest {
  sku: string;
  name: string;
  description?: string;
  price: number;
  collectionId?: string;
}

/**
 * Update product request
 */
export interface UpdateProductRequest {
  name: string;
  description?: string;
  price: number;
  collectionId?: string;
  isActive: boolean;
}

/**
 * Import result from Excel upload
 */
export interface ImportResult {
  success: boolean;
  totalRows: number;
  createdCount: number;
  updatedCount: number;
  collectionsCreatedCount: number;
  errors: ImportError[];
  warnings: string[];
  message: string;
}

/**
 * Import error detail
 */
export interface ImportError {
  rowNumber: number;
  field: string;
  message: string;
  value?: string;
}

/**
 * SKU check response for uniqueness validation
 */
export interface SkuCheckResult {
  exists: boolean;
  productId?: string;
}

/**
 * Lightweight product DTO for catalog display
 */
export interface ProductListItem {
  id: string;
  sku: string;
  name: string;
  price: number;
  primaryPhotoUrl?: string;
  collectionName?: string;
  isActive: boolean;
  availableQuantity: number;
  createdAt: string;
}

/**
 * Paginated result wrapper
 */
export interface PaginatedResult<T> {
  items: T[];
  totalCount: number;
  totalPages: number;
  currentPage: number;
  pageSize: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

/**
 * Catalog query parameters
 */
export interface CatalogQueryParams {
  page?: number;
  pageSize?: number;
  sortBy?: 'name' | 'createdAt' | 'price';
  sortDirection?: 'asc' | 'desc';
  includeInactive?: boolean;
}

