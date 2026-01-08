/**
 * Inventory type definitions
 * Matches backend DTOs for inventory management
 */

/**
 * Movement type enum
 */
export enum MovementType {
  Sale = 1,
  Return = 2,
  Adjustment = 3,
  Import = 4,
}

/**
 * Inventory DTO
 */
export interface Inventory {
  id: string;
  productId: string;
  productSku: string;
  productName: string;
  pointOfSaleId: string;
  pointOfSaleName: string;
  quantity: number;
  isActive: boolean;
  lastUpdatedAt: string;
  createdAt: string;
}

/**
 * Centralized stock view
 */
export interface CentralizedStock {
  productId: string;
  productSku: string;
  productName: string;
  totalQuantity: number;
  breakdown: PointOfSaleStock[];
}

/**
 * Stock at a specific point of sale
 */
export interface PointOfSaleStock {
  pointOfSaleId: string;
  pointOfSaleName: string;
  quantity: number;
}

/**
 * Paginated inventory result
 */
export interface PaginatedInventoryResult {
  items: Inventory[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

/**
 * Paginated centralized stock result
 */
export interface PaginatedCentralizedStockResult {
  items: CentralizedStock[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

/**
 * Assignment request
 */
export interface AssignProductRequest {
  productId: string;
  pointOfSaleId: string;
}

/**
 * Bulk assignment request
 */
export interface BulkAssignProductsRequest {
  productIds: string[];
  pointOfSaleId: string;
}

/**
 * Unassignment request
 */
export interface UnassignProductRequest {
  productId: string;
  pointOfSaleId: string;
}

/**
 * Assignment result
 */
export interface AssignmentResult {
  success: boolean;
  inventory?: Inventory;
  errorMessage?: string;
  wasReactivated: boolean;
}

/**
 * Bulk assignment result
 */
export interface BulkAssignmentResult {
  success: boolean;
  assignedCount: number;
  reactivatedCount: number;
  failedCount: number;
  skippedCount: number;
  inventories: Inventory[];
  errors: AssignmentError[];
  message: string;
}

/**
 * Assignment error detail
 */
export interface AssignmentError {
  productId: string;
  message: string;
}

/**
 * Stock adjustment request
 */
export interface StockAdjustmentRequest {
  productId: string;
  pointOfSaleId: string;
  quantityChange: number;
  reason: string;
}

/**
 * Stock adjustment result
 */
export interface StockAdjustmentResult {
  success: boolean;
  inventory?: Inventory;
  quantityBefore: number;
  quantityAfter: number;
  errorMessage?: string;
}

/**
 * Stock import result
 */
export interface StockImportResult {
  success: boolean;
  totalRows: number;
  stockUpdatedCount: number;
  assignmentsCreatedCount: number;
  errors: StockImportError[];
  warnings: string[];
  message: string;
}

/**
 * Stock import error
 */
export interface StockImportError {
  rowNumber: number;
  field: string;
  message: string;
  value?: string;
}

/**
 * Inventory movement DTO
 */
export interface InventoryMovement {
  id: string;
  inventoryId: string;
  productId: string;
  productSku: string;
  productName: string;
  pointOfSaleId: string;
  pointOfSaleName: string;
  movementType: MovementType;
  movementTypeName: string;
  quantityChange: number;
  quantityBefore: number;
  quantityAfter: number;
  reason?: string;
  userId: string;
  userName: string;
  movementDate: string;
  createdAt: string;
}

/**
 * Paginated movement result
 */
export interface PaginatedMovementResult {
  items: InventoryMovement[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

/**
 * Movement history filter
 */
export interface MovementHistoryFilter {
  productId?: string;
  pointOfSaleId?: string;
  startDate?: string;
  endDate?: string;
  page?: number;
  pageSize?: number;
}

