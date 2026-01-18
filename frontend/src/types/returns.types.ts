/**
 * Returns Module Types (EP5)
 */

/**
 * Return category enum values.
 */
export enum ReturnCategory {
  Defectuoso = 1,
  TamañoIncorrecto = 2,
  NoSatisfecho = 3,
  Otro = 4,
}

/**
 * Category display names mapping.
 */
export const ReturnCategoryLabels: Record<ReturnCategory, string> = {
  [ReturnCategory.Defectuoso]: 'Defectuoso',
  [ReturnCategory.TamañoIncorrecto]: 'Tamaño Incorrecto',
  [ReturnCategory.NoSatisfecho]: 'No Satisfecho',
  [ReturnCategory.Otro]: 'Otro',
};

/**
 * Sale association for creating a return.
 */
export interface SaleAssociationRequest {
  saleId: string;
  quantity: number;
}

/**
 * Request to create a new return.
 */
export interface CreateReturnRequest {
  productId: string;
  pointOfSaleId: string;
  quantity: number;
  category: ReturnCategory;
  reason?: string;
  saleAssociations: SaleAssociationRequest[];
  photoBase64?: string;
  photoFileName?: string;
}

/**
 * Response from creating a return.
 */
export interface CreateReturnResponse {
  returnData: Return;
  newStockQuantity?: number;
}

/**
 * Return-sale association details.
 */
export interface ReturnSale {
  saleId: string;
  saleDate: string;
  quantity: number;
  unitPrice: number;
  subtotal: number;
  paymentMethodName: string;
}

/**
 * Return details.
 */
export interface Return {
  id: string;
  productId: string;
  productSku: string;
  productName: string;
  pointOfSaleId: string;
  pointOfSaleName: string;
  userId: string;
  userName: string;
  quantity: number;
  category: ReturnCategory;
  categoryName: string;
  reason?: string;
  totalValue: number;
  hasPhoto: boolean;
  returnDate: string;
  createdAt: string;
  associatedSales: ReturnSale[];
}

/**
 * Request for returns history with filters.
 */
export interface ReturnsHistoryFilterRequest {
  startDate?: string;
  endDate?: string;
  pointOfSaleId?: string;
  productId?: string;
  page?: number;
  pageSize?: number;
}

/**
 * Paginated returns history response.
 */
export interface ReturnsHistoryResponse {
  returns: Return[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

/**
 * Eligible sale for return.
 */
export interface EligibleSale {
  saleId: string;
  saleDate: string;
  originalQuantity: number;
  returnedQuantity: number;
  availableForReturn: number;
  unitPrice: number;
  paymentMethodName: string;
  daysRemaining: number;
}

/**
 * Response for eligible sales query.
 */
export interface EligibleSalesResponse {
  eligibleSales: EligibleSale[];
  totalAvailableForReturn: number;
}
