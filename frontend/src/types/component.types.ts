/**
 * Component Management Types - EP10
 * Type definitions for product component management
 */

/**
 * Component from the master table
 */
export interface ProductComponent {
  id: string;
  description: string;
  costPrice: number | null;
  salePrice: number | null;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

/**
 * Paginated component list response
 */
export interface PaginatedComponentResult {
  items: ProductComponent[];
  totalCount: number;
  totalPages: number;
  currentPage: number;
  pageSize: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

/**
 * Request to create a component
 */
export interface CreateComponentRequest {
  description: string;
  costPrice?: number | null;
  salePrice?: number | null;
}

/**
 * Request to update a component
 */
export interface UpdateComponentRequest {
  description: string;
  costPrice?: number | null;
  salePrice?: number | null;
  isActive: boolean;
}

/**
 * Component query parameters
 */
export interface ComponentQueryParams {
  page?: number;
  pageSize?: number;
  isActive?: boolean;
  search?: string;
}

/**
 * Component assignment on a product
 */
export interface ComponentAssignment {
  id: string;
  componentId: string;
  componentDescription: string;
  quantity: number;
  costPrice: number;
  salePrice: number;
  displayOrder: number;
  masterCostPrice: number | null;
  masterSalePrice: number | null;
}

/**
 * Request to save component assignments (replaces all)
 */
export interface SaveComponentAssignmentsRequest {
  assignments: ComponentAssignmentItem[];
}

/**
 * Individual assignment item in a save request
 */
export interface ComponentAssignmentItem {
  componentId: string;
  quantity: number;
  costPrice: number;
  salePrice: number;
  displayOrder: number;
}

/**
 * Price sync preview
 */
export interface PriceSyncPreview {
  items: PriceSyncItem[];
}

/**
 * Individual item in price sync preview
 */
export interface PriceSyncItem {
  componentId: string;
  componentDescription: string;
  currentCostPrice: number;
  currentSalePrice: number;
  newCostPrice: number | null;
  newSalePrice: number | null;
  willBeUpdated: boolean;
}

/**
 * Component template
 */
export interface ComponentTemplate {
  id: string;
  name: string;
  description: string | null;
  items: ComponentTemplateItem[];
  createdAt: string;
  updatedAt: string;
}

/**
 * Template item (component + quantity)
 */
export interface ComponentTemplateItem {
  componentId: string;
  componentDescription: string;
  quantity: number;
}

/**
 * Request to create/update a template
 */
export interface SaveComponentTemplateRequest {
  name: string;
  description?: string | null;
  items: TemplateItemRequest[];
}

/**
 * Individual item in a template save request
 */
export interface TemplateItemRequest {
  componentId: string;
  quantity: number;
}

/**
 * Request to apply a template to a product
 */
export interface ApplyTemplateRequest {
  templateId: string;
}

/**
 * Result of applying a template
 */
export interface ApplyTemplateResult {
  assignments: ComponentAssignment[];
  addedComponents: string[];
  skippedComponents: string[];
}

/**
 * Margin report row
 */
export interface ProductMargin {
  productId: string;
  sku: string;
  productName: string;
  collectionName: string | null;
  officialPrice: number;
  totalCostPrice: number;
  totalSalePrice: number;
  marginAmount: number;
  marginPercent: number;
}

/**
 * Margin report response with aggregated totals
 */
export interface MarginReport {
  items: ProductMargin[];
  totalCount: number;
  totalPages: number;
  currentPage: number;
  pageSize: number;
  sumCostPrice: number;
  sumSalePrice: number;
  sumMargin: number;
}

/**
 * Margin report query params
 */
export interface MarginReportQueryParams {
  page?: number;
  pageSize?: number;
  collectionId?: string;
  maxMarginPercent?: number;
  search?: string;
}

/**
 * Product without components (report row)
 */
export interface ProductWithoutComponents {
  productId: string;
  sku: string;
  productName: string;
  price: number;
  collectionName: string | null;
}

/**
 * Products without components query params
 */
export interface ProductsWithoutComponentsQueryParams {
  page?: number;
  pageSize?: number;
  collectionId?: string;
  search?: string;
}
